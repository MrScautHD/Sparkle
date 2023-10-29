using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.registry.types;

namespace Sparkle.csharp.entity.component; 

public class Light : Component {

    public static int LightCount { get; private set; }
    
    private bool _enabled;
    private LightType _type;
    private Vector3 _target;
    private Color _color;

    private int _enabledLoc;
    private int _typeLoc;
    private int _posLoc;
    private int _targetLoc;
    private int _colorLoc;
    
    public Light(LightType type, Vector3 target, Color color) {
        this._enabled = true;
        this._type = type;
        this._target = target;
        this._color = color;
    }
    
    public bool Enabled {
        get => this._enabled;
        set {
            this._enabled = value;
            this.UpdateValues();
        }
    }

    public LightType Type {
        get => this._type;
        set {
            this._type = value;
            this.UpdateValues();
        }
    }
    
    public Vector3 Target {
        get => this._target;
        set {
            this._target = value;
            this.UpdateValues();
        }
    }
    
    public Color Color {
        get => this._color;
        set {
            this._color = value;
            this.UpdateValues();
        }
    }
    
    protected internal override unsafe void Init() {
        base.Init();
        ShaderRegistry.Light.locs[(int) ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW] = ShaderHelper.GetLocation(ShaderRegistry.Light, "viewPos");

        float[] ambient = new[] {
            0.1f,
            0.1f,
            0.1f,
            1.0f
        };
        
        int ambientLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, "ambient");
        ShaderHelper.SetValue(ShaderRegistry.Light, ambientLoc, ambient, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        
        this._enabledLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, $"lights[{LightCount}].enabled");
        this._typeLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, $"lights[{LightCount}].type");
        this._posLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, $"lights[{LightCount}].position");
        this._targetLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, $"lights[{LightCount}].target");
        this._colorLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, $"lights[{LightCount}].color");
        LightCount++;
    }
    
    private void UpdateValues() {
        ShaderHelper.SetValue(ShaderRegistry.Light, this._enabledLoc, this._enabled ? 1 : 0, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Light, this._typeLoc, (int) this._type, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Light, this._posLoc, this.Entity.Position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        ShaderHelper.SetValue(ShaderRegistry.Light, this._targetLoc, this._target, ShaderUniformDataType.SHADER_UNIFORM_VEC3);

        float[] color = new[] {
            this._color.r / 255.0F,
            this._color.g / 255.0F,
            this._color.b / 255.0F,
            this._color.a / 255.0F
        };
        
        ShaderHelper.SetValue(ShaderRegistry.Light, this._colorLoc, color, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
    }
    
    public enum LightType {
        Directional,
        Pointed
    }

    protected override void Dispose(bool disposing) { }
}