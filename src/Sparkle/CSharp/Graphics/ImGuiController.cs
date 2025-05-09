using System.Numerics;
using System.Runtime.CompilerServices;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Interact.Mice;
using ImGuiNET;
using Veldrid;

namespace Sparkle.CSharp.Graphics;

/// <summary>
/// A modified version of Veldrid.ImGui's ImGuiRenderer.
/// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
/// </summary>
public class ImGuiController : IDisposable
{
    private struct ResourceSetInfo(IntPtr imGuiBinding, ResourceSet resourceSet)
    {
        public readonly IntPtr ImGuiBinding = imGuiBinding;
        public readonly ResourceSet ResourceSet = resourceSet;
    }
    
    private GraphicsDevice _graphicsDevice;
    private bool _frameBegun;

    // Veldrid objects
    private DeviceBuffer _vertexBuffer = null!;
    private DeviceBuffer _indexBuffer = null!;
    private DeviceBuffer _projMatrixBuffer = null!;
    private Texture _fontTexture = null!;
    private TextureView _fontTextureView = null!;
    private Shader _vertexShader = null!;
    private Shader _fragmentShader = null!;
    private ResourceLayout _layout = null!;
    private ResourceLayout _textureLayout = null!;
    private Pipeline _pipeline = null!;
    private ResourceSet _mainResourceSet = null!;
    private ResourceSet _fontTextureResourceSet = null!;

    private const IntPtr FontAtlasId = 1;

    private int _windowWidth;
    private int _windowHeight;
    private readonly Vector2 _scaleFactor = Vector2.One;

    // Image trackers
    private readonly Dictionary<TextureView, ResourceSetInfo> _setsByView = new();
    private readonly Dictionary<Texture, TextureView?> _autoViewsByTexture = new();
    private readonly Dictionary<IntPtr, ResourceSetInfo> _viewsById = new();
    private readonly List<IDisposable?> _ownedResources = [];
    private int _lastAssignedId = 100;

    // Input
    private static readonly Dictionary<KeyboardKey, ImGuiKey> KeyMap = new();
    private static bool _lastControlPressed;
    private static bool _lastShiftPressed;
    private static bool _lastAltPressed;
    private static bool _lastSuperPressed;
    
    /// <summary>
    /// Constructs a new ImGuiController.
    /// </summary>
    public ImGuiController(GraphicsDevice graphicsDevice, OutputDescription outputDescription, int width, int height)
    {
        _graphicsDevice = graphicsDevice;
        _windowWidth = width;
        _windowHeight = height;

        SetupKeymap();
        
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard |
                          ImGuiConfigFlags.DockingEnable;
        io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

        CreateDeviceResources(graphicsDevice, outputDescription);
        SetPerFrameImGuiData(1f / 60f);
        ImGui.NewFrame();
        _frameBegun = true;
    }

    public void Resize(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    private void CreateDeviceResources(GraphicsDevice gd, OutputDescription outputDescription)
    {
        _graphicsDevice = gd;
        var factory = gd.ResourceFactory;
        
        _vertexBuffer = factory.CreateBuffer(new BufferDescription(10000, 
            BufferUsage.VertexBuffer | BufferUsage.Dynamic)
        );
        _vertexBuffer.Name = "ImGui.NET Vertex Buffer";
        
        _indexBuffer = factory.CreateBuffer(new BufferDescription(2000, 
            BufferUsage.IndexBuffer | BufferUsage.Dynamic)
        );
        _indexBuffer.Name = "ImGui.NET Index Buffer";
        
        RecreateFontDeviceTexture(gd);

        _projMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, 
            BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );
        _projMatrixBuffer.Name = "ImGui.NET Projection Buffer";

        var vertexShaderBytes = LoadEmbeddedShaderCode(gd.ResourceFactory, "imgui-vertex");
        var fragmentShaderBytes = LoadEmbeddedShaderCode(gd.ResourceFactory, "imgui-frag");
        
        _vertexShader = factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, 
            gd.BackendType == GraphicsBackend.Metal ? "VS" : "main")
        );
        
        _fragmentShader = factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, 
            gd.BackendType == GraphicsBackend.Metal ? "FS" : "main")
        );

        VertexLayoutDescription[] vertexLayouts =
        [new(
            new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2), 
            new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4Norm))
        ];

        _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment))
        );
        
        _textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "MainTexture", 
                ResourceKind.TextureReadOnly, ShaderStages.Fragment)
            )
        );

        var pipelineDescription = new GraphicsPipelineDescription(
            BlendStateDescription.SINGLE_ALPHA_BLEND,
            new DepthStencilStateDescription(false, false, ComparisonKind.Always),
            new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, false, true),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(vertexLayouts, [_vertexShader, _fragmentShader]),
            [_layout, _textureLayout],
            outputDescription,
            ResourceBindingModel.Default
        );
        
        _pipeline = factory.CreateGraphicsPipeline(ref pipelineDescription);

        _mainResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
            _layout, _projMatrixBuffer, gd.PointSampler
        ));

        _fontTextureResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
            _textureLayout, _fontTextureView
        ));
    }

    /// <summary>
    /// Gets or creates a handle for a texture to be drawn with ImGui.
    /// Pass the returned handle to Image() or ImageButton().
    /// </summary>
    public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, TextureView textureView)
    {
        if (_setsByView.TryGetValue(textureView, out var rsi)) return rsi.ImGuiBinding;
        
        var resourceSet = factory.CreateResourceSet(new ResourceSetDescription(_textureLayout, textureView));
        rsi = new ResourceSetInfo(GetNextImGuiBindingId(), resourceSet);

        _setsByView.Add(textureView, rsi);
        _viewsById.Add(rsi.ImGuiBinding, rsi);
        _ownedResources.Add(resourceSet);

        return rsi.ImGuiBinding;
    }

    private IntPtr GetNextImGuiBindingId() => _lastAssignedId++;

    /// <summary>
    /// Gets or creates a handle for a texture to be drawn with ImGui.
    /// Pass the returned handle to Image() or ImageButton().
    /// </summary>
    public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, Texture texture)
    {
        if (_autoViewsByTexture.TryGetValue(texture, out var textureView))
            return GetOrCreateImGuiBinding(factory, textureView!);
        
        textureView = factory.CreateTextureView(texture);
        _autoViewsByTexture.Add(texture, textureView);
        _ownedResources.Add(textureView);

        return GetOrCreateImGuiBinding(factory, textureView);
    }

    /// <summary>
    /// Retrieves the shader texture binding for the given helper handle.
    /// </summary>
    public ResourceSet GetImageResourceSet(IntPtr imGuiBinding)
    {
        if (!_viewsById.TryGetValue(imGuiBinding, out var tvi))
            throw new InvalidOperationException("No registered ImGui binding with id " + imGuiBinding);

        return tvi.ResourceSet;
    }

    public void ClearCachedImageResources()
    {
        foreach (var resource in _ownedResources) 
            resource?.Dispose();

        _ownedResources.Clear();
        _setsByView.Clear();
        _viewsById.Clear();
        _autoViewsByTexture.Clear();
        _lastAssignedId = 100;
    }

    private static byte[] LoadEmbeddedShaderCode(ResourceFactory factory, string name)
    {
        switch (factory.BackendType)
        {
            case GraphicsBackend.Direct3D11:
            {
                var resourceName = name + ".hlsl.bytes";
                return GetEmbeddedResourceBytes(resourceName);
            }
            case GraphicsBackend.OpenGL:
            {
                var resourceName = name + ".glsl";
                return GetEmbeddedResourceBytes(resourceName);
            }
            case GraphicsBackend.Vulkan:
            {
                var resourceName = name + ".spv";
                return GetEmbeddedResourceBytes(resourceName);
            }
            case GraphicsBackend.Metal:
            {
                var resourceName = name + ".metallib";
                return GetEmbeddedResourceBytes(resourceName);
            }
            case GraphicsBackend.OpenGLES:
            default:
                throw new Exception("Unsupported GraphicsBackend");
        }
    }

    private static byte[] GetEmbeddedResourceBytes(string resourceName)
    {
        var assembly = typeof(ImGuiController).Assembly;
        using var s = assembly.GetManifestResourceStream(resourceName);
        var ret = new byte[s!.Length];
        _ = s.Read(ret, 0, (int)s.Length);
        return ret;
    }

    /// <summary>
    /// Recreates the device texture used to render text.
    /// </summary>
    public void RecreateFontDeviceTexture(GraphicsDevice gd)
    {
        var io = ImGui.GetIO();
        
        // Build
        io.Fonts.GetTexDataAsRGBA32(
            out IntPtr pixels, 
            out var width, out var height, 
            out var bytesPerPixel
        );
        
        // Store our identifier
        io.Fonts.SetTexID(FontAtlasId);

        _fontTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            (uint)width,
            (uint)height,
            1,
            1,
            PixelFormat.R8G8B8A8UNorm,
            TextureUsage.Sampled)
        );
        
        _fontTexture.Name = "ImGui.NET Font Texture";
        gd.UpdateTexture(
            _fontTexture,
            pixels,
            (uint)(bytesPerPixel * width * height),
            0,
            0,
            0,
            (uint)width,
            (uint)height,
            1,
            0,
            0
        );
        
        _fontTextureView = gd.ResourceFactory.CreateTextureView(_fontTexture);

        io.Fonts.ClearTexData();
    }

    /// <summary>
    /// Renders the ImGui draw list data.
    /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
    /// or index data has increased beyond the capacity of the existing buffers.
    /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
    /// </summary>
    public void Render(GraphicsDevice gd, CommandList cl)
    {
        if (!_frameBegun) return;
        
        _frameBegun = false;
        ImGui.Render();
        RenderImDrawData(ImGui.GetDrawData(), gd, cl);
    }

    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update(float deltaSeconds)
    {
        if (_frameBegun) 
            ImGui.Render();

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput();

        _frameBegun = true;
        ImGui.NewFrame();
    }

    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(
            _windowWidth / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y
        );
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

    private static void SetupKeymap()
    {
        if (KeyMap.Count > 0)
            return;

        // build up a map of raylib keys to ImGuiKeys
        //_keyMap[KeyboardKey.Apostrophe] = ImGuiKey.Apostrophe;
        KeyMap[KeyboardKey.Comma] = ImGuiKey.Comma;
        KeyMap[KeyboardKey.Minus] = ImGuiKey.Minus;
        KeyMap[KeyboardKey.Period] = ImGuiKey.Period;
        KeyMap[KeyboardKey.Slash] = ImGuiKey.Slash;
        KeyMap[KeyboardKey.Number0] = ImGuiKey._0;
        KeyMap[KeyboardKey.Number1] = ImGuiKey._1;
        KeyMap[KeyboardKey.Number2] = ImGuiKey._2;
        KeyMap[KeyboardKey.Number3] = ImGuiKey._3;
        KeyMap[KeyboardKey.Number4] = ImGuiKey._4;
        KeyMap[KeyboardKey.Number5] = ImGuiKey._5;
        KeyMap[KeyboardKey.Number6] = ImGuiKey._6;
        KeyMap[KeyboardKey.Number7] = ImGuiKey._7;
        KeyMap[KeyboardKey.Number8] = ImGuiKey._8;
        KeyMap[KeyboardKey.Number9] = ImGuiKey._9;
        KeyMap[KeyboardKey.Semicolon] = ImGuiKey.Semicolon;
        //_keyMap[KeyboardKey.Equal] = ImGuiKey.Equal;
        KeyMap[KeyboardKey.A] = ImGuiKey.A;
        KeyMap[KeyboardKey.B] = ImGuiKey.B;
        KeyMap[KeyboardKey.C] = ImGuiKey.C;
        KeyMap[KeyboardKey.D] = ImGuiKey.D;
        KeyMap[KeyboardKey.E] = ImGuiKey.E;
        KeyMap[KeyboardKey.F] = ImGuiKey.F;
        KeyMap[KeyboardKey.G] = ImGuiKey.G;
        KeyMap[KeyboardKey.H] = ImGuiKey.H;
        KeyMap[KeyboardKey.I] = ImGuiKey.I;
        KeyMap[KeyboardKey.J] = ImGuiKey.J;
        KeyMap[KeyboardKey.K] = ImGuiKey.K;
        KeyMap[KeyboardKey.L] = ImGuiKey.L;
        KeyMap[KeyboardKey.M] = ImGuiKey.M;
        KeyMap[KeyboardKey.N] = ImGuiKey.N;
        KeyMap[KeyboardKey.O] = ImGuiKey.O;
        KeyMap[KeyboardKey.P] = ImGuiKey.P;
        KeyMap[KeyboardKey.Q] = ImGuiKey.Q;
        KeyMap[KeyboardKey.R] = ImGuiKey.R;
        KeyMap[KeyboardKey.S] = ImGuiKey.S;
        KeyMap[KeyboardKey.T] = ImGuiKey.T;
        KeyMap[KeyboardKey.U] = ImGuiKey.U;
        KeyMap[KeyboardKey.V] = ImGuiKey.V;
        KeyMap[KeyboardKey.W] = ImGuiKey.W;
        KeyMap[KeyboardKey.X] = ImGuiKey.X;
        KeyMap[KeyboardKey.Y] = ImGuiKey.Y;
        KeyMap[KeyboardKey.Z] = ImGuiKey.Z;
        KeyMap[KeyboardKey.Space] = ImGuiKey.Space;
        KeyMap[KeyboardKey.Escape] = ImGuiKey.Escape;
        KeyMap[KeyboardKey.Enter] = ImGuiKey.Enter;
        KeyMap[KeyboardKey.Tab] = ImGuiKey.Tab;
        KeyMap[KeyboardKey.BackSpace] = ImGuiKey.Backspace;
        KeyMap[KeyboardKey.Insert] = ImGuiKey.Insert;
        KeyMap[KeyboardKey.Delete] = ImGuiKey.Delete;
        KeyMap[KeyboardKey.Right] = ImGuiKey.RightArrow;
        KeyMap[KeyboardKey.Left] = ImGuiKey.LeftArrow;
        KeyMap[KeyboardKey.Down] = ImGuiKey.DownArrow;
        KeyMap[KeyboardKey.Up] = ImGuiKey.UpArrow;
        KeyMap[KeyboardKey.PageUp] = ImGuiKey.PageUp;
        KeyMap[KeyboardKey.PageDown] = ImGuiKey.PageDown;
        KeyMap[KeyboardKey.Home] = ImGuiKey.Home;
        KeyMap[KeyboardKey.End] = ImGuiKey.End;
        KeyMap[KeyboardKey.CapsLock] = ImGuiKey.CapsLock;
        KeyMap[KeyboardKey.ScrollLock] = ImGuiKey.ScrollLock;
        KeyMap[KeyboardKey.NumLock] = ImGuiKey.NumLock;
        KeyMap[KeyboardKey.PrintScreen] = ImGuiKey.PrintScreen;
        KeyMap[KeyboardKey.Pause] = ImGuiKey.Pause;
        KeyMap[KeyboardKey.F1] = ImGuiKey.F1;
        KeyMap[KeyboardKey.F2] = ImGuiKey.F2;
        KeyMap[KeyboardKey.F3] = ImGuiKey.F3;
        KeyMap[KeyboardKey.F4] = ImGuiKey.F4;
        KeyMap[KeyboardKey.F5] = ImGuiKey.F5;
        KeyMap[KeyboardKey.F6] = ImGuiKey.F6;
        KeyMap[KeyboardKey.F7] = ImGuiKey.F7;
        KeyMap[KeyboardKey.F8] = ImGuiKey.F8;
        KeyMap[KeyboardKey.F9] = ImGuiKey.F9;
        KeyMap[KeyboardKey.F10] = ImGuiKey.F10;
        KeyMap[KeyboardKey.F11] = ImGuiKey.F11;
        KeyMap[KeyboardKey.F12] = ImGuiKey.F12;
        KeyMap[KeyboardKey.ShiftLeft] = ImGuiKey.LeftShift;
        KeyMap[KeyboardKey.ControlLeft] = ImGuiKey.LeftCtrl;
        KeyMap[KeyboardKey.AltLeft] = ImGuiKey.LeftAlt;
        KeyMap[KeyboardKey.WinLeft] = ImGuiKey.LeftSuper;
        KeyMap[KeyboardKey.ShiftRight] = ImGuiKey.RightShift;
        KeyMap[KeyboardKey.ControlRight] = ImGuiKey.RightCtrl;
        KeyMap[KeyboardKey.AltRight] = ImGuiKey.RightAlt;
        KeyMap[KeyboardKey.WinRight] = ImGuiKey.RightSuper;
        KeyMap[KeyboardKey.Menu] = ImGuiKey.Menu;
        KeyMap[KeyboardKey.BracketLeft] = ImGuiKey.LeftBracket;
        KeyMap[KeyboardKey.BackSlash] = ImGuiKey.Backslash;
        KeyMap[KeyboardKey.BracketRight] = ImGuiKey.RightBracket;
        KeyMap[KeyboardKey.Grave] = ImGuiKey.GraveAccent;
        KeyMap[KeyboardKey.Keypad0] = ImGuiKey.Keypad0;
        KeyMap[KeyboardKey.Keypad1] = ImGuiKey.Keypad1;
        KeyMap[KeyboardKey.Keypad2] = ImGuiKey.Keypad2;
        KeyMap[KeyboardKey.Keypad3] = ImGuiKey.Keypad3;
        KeyMap[KeyboardKey.Keypad4] = ImGuiKey.Keypad4;
        KeyMap[KeyboardKey.Keypad5] = ImGuiKey.Keypad5;
        KeyMap[KeyboardKey.Keypad6] = ImGuiKey.Keypad6;
        KeyMap[KeyboardKey.Keypad7] = ImGuiKey.Keypad7;
        KeyMap[KeyboardKey.Keypad8] = ImGuiKey.Keypad8;
        KeyMap[KeyboardKey.Keypad9] = ImGuiKey.Keypad9;
        KeyMap[KeyboardKey.KeypadDecimal] = ImGuiKey.KeypadDecimal;
        KeyMap[KeyboardKey.KeypadDivide] = ImGuiKey.KeypadDivide;
        KeyMap[KeyboardKey.KeypadMultiply] = ImGuiKey.KeypadMultiply;
        KeyMap[KeyboardKey.KeypadMinus] = ImGuiKey.KeypadSubtract;
        KeyMap[KeyboardKey.KeypadPlus] = ImGuiKey.KeypadAdd;
        KeyMap[KeyboardKey.KeypadEnter] = ImGuiKey.KeypadEnter;
        //_keyMap[KeyboardKey.keypadEqual] = ImGuiKey.KeypadEqual;
    }

    private void UpdateImGuiInput()
    {
        var io = ImGui.GetIO();

        if (!Input.IsRelativeMouseModeEnabled()) {
            var mousePosition = Input.GetMousePosition();
            io.AddMousePosEvent(mousePosition.X, mousePosition.Y);
            io.AddMouseButtonEvent(0, Input.IsMouseButtonDown(MouseButton.Left));
            io.AddMouseButtonEvent(1, Input.IsMouseButtonDown(MouseButton.Right));
            io.AddMouseButtonEvent(2, Input.IsMouseButtonDown(MouseButton.Middle));
            io.AddMouseButtonEvent(3, Input.IsMouseButtonDown(MouseButton.X1));
            io.AddMouseButtonEvent(4, Input.IsMouseButtonDown(MouseButton.X2));
            if (Input.IsMouseScrolling(out var wheelDelta))
                io.AddMouseWheelEvent(0f, wheelDelta.Y);
        }

        foreach (var ch in Input.GetPressedChars()) 
            io.AddInputCharacter(ch);
        
        var ctrlDown = Input.IsKeyDown(KeyboardKey.ControlLeft);
        if (ctrlDown != _lastControlPressed)
            io.AddKeyEvent(ImGuiKey.ModCtrl, ctrlDown);
        _lastControlPressed = ctrlDown;

        var shiftDown = Input.IsKeyDown(KeyboardKey.ShiftLeft);
        if (shiftDown != _lastShiftPressed)
            io.AddKeyEvent(ImGuiKey.ModShift, shiftDown);
        _lastShiftPressed = shiftDown;

        var altDown = Input.IsKeyDown(KeyboardKey.AltLeft);
        if (altDown != _lastAltPressed)
            io.AddKeyEvent(ImGuiKey.ModAlt, altDown);
        _lastAltPressed = altDown;

        var superDown = Input.IsKeyDown(KeyboardKey.WinLeft);
        if (superDown != _lastSuperPressed)
            io.AddKeyEvent(ImGuiKey.ModSuper, superDown);
        _lastSuperPressed = superDown;
        
        foreach (var (key, imGuiKey) in KeyMap)
        {
            if (Input.IsKeyDown(key))
                io.AddKeyEvent(imGuiKey, true);
        }

        foreach (var (key, imGuiKey) in KeyMap)
        {
            if (Input.IsKeyReleased(key))
                io.AddKeyEvent(imGuiKey, false);
        }
    }

    private void RenderImDrawData(ImDrawDataPtr drawData, GraphicsDevice gd, CommandList cl)
    {
        uint vertexOffsetInVertices = 0;
        uint indexOffsetInElements = 0;

        if (drawData.CmdListsCount == 0)
            return;

        var totalVbSize = (uint)(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
        if (totalVbSize > _vertexBuffer.SizeInBytes)
        {
            gd.DisposeWhenIdle(_vertexBuffer);
            _vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint)(totalVbSize * 1.5f), 
                BufferUsage.VertexBuffer | BufferUsage.Dynamic
            ));
        }

        var totalIbSize = (uint)(drawData.TotalIdxCount * sizeof(ushort));
        if (totalIbSize > _indexBuffer.SizeInBytes)
        {
            gd.DisposeWhenIdle(_indexBuffer);
            _indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint)(totalIbSize * 1.5f), 
                BufferUsage.IndexBuffer | BufferUsage.Dynamic
            ));
        }

        for (var i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdLists[i];

            cl.UpdateBuffer(
                _vertexBuffer,
                vertexOffsetInVertices * (uint)Unsafe.SizeOf<ImDrawVert>(),
                cmdList.VtxBuffer.Data,
                (uint)(cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>())
            );

            cl.UpdateBuffer(
                _indexBuffer,
                indexOffsetInElements * sizeof(ushort),
                cmdList.IdxBuffer.Data,
                (uint)(cmdList.IdxBuffer.Size * sizeof(ushort))
            );

            vertexOffsetInVertices += (uint)cmdList.VtxBuffer.Size;
            indexOffsetInElements += (uint)cmdList.IdxBuffer.Size;
        }

        // Setup orthographic projection matrix into our constant buffer
        var io = ImGui.GetIO();
        var mvp = Matrix4x4.CreateOrthographicOffCenter(
            0f,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f
        );

        _graphicsDevice.UpdateBuffer(_projMatrixBuffer, 0, ref mvp);

        cl.SetVertexBuffer(0, _vertexBuffer);
        cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        cl.SetPipeline(_pipeline);
        cl.SetGraphicsResourceSet(0, _mainResourceSet);

        drawData.ScaleClipRects(io.DisplayFramebufferScale);

        // Render command lists
        var vtxOffset = 0;
        var idxOffset = 0;
        for (var n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdLists[n];
            for (var cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
            {
                var imDrawCmdPtr = cmdList.CmdBuffer[cmdI];
                if (imDrawCmdPtr.UserCallback != IntPtr.Zero)
                    throw new Exception();

                if (imDrawCmdPtr.TextureId != IntPtr.Zero)
                {
                    cl.SetGraphicsResourceSet(1,
                        imDrawCmdPtr.TextureId == FontAtlasId
                            ? _fontTextureResourceSet
                            : GetImageResourceSet(imDrawCmdPtr.TextureId)
                    );
                }

                cl.SetScissorRect(
                    0,
                    (uint)imDrawCmdPtr.ClipRect.X,
                    (uint)imDrawCmdPtr.ClipRect.Y,
                    (uint)(imDrawCmdPtr.ClipRect.Z - imDrawCmdPtr.ClipRect.X),
                    (uint)(imDrawCmdPtr.ClipRect.W - imDrawCmdPtr.ClipRect.Y)
                );

                cl.DrawIndexed(imDrawCmdPtr.ElemCount, 1, 
                    imDrawCmdPtr.IdxOffset + (uint)idxOffset, 
                    (int)imDrawCmdPtr.VtxOffset + vtxOffset, 0
                );
            }
            vtxOffset += cmdList.VtxBuffer.Size;
            idxOffset += cmdList.IdxBuffer.Size;
        }
    }

    /// <summary>
    /// Frees all graphics resources used by the renderer.
    /// </summary>
    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _projMatrixBuffer.Dispose();
        _fontTexture.Dispose();
        _fontTextureView.Dispose();
        _vertexShader.Dispose();
        _fragmentShader.Dispose();
        _layout.Dispose();
        _textureLayout.Dispose();
        _pipeline.Dispose();
        _mainResourceSet.Dispose();

        foreach (var resource in _ownedResources) 
            resource?.Dispose();
        
        GC.SuppressFinalize(this);
    }
}