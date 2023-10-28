using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.entity; 

public class Light : Entity {

    public Shader Shader;
    
    public bool Enabled;
    public LightType Type;
    public Vector3 Position;
    public Vector3 Target;
    public Color Color;

    public int EnabledLoc;
    public int TypeLoc;
    public int PosLoc;
    public int TargetLoc;
    public int ColorLoc;
    
    public int LightCount { get; private set; }

    public Light(Vector3 position, LightType type, Vector3 target, Color color) : base(position) {
        this.Enabled = true;
        this.Type = type;
        this.Target = target;
        this.Color = color;
    }

    protected internal override void Init() {
        base.Init();
        this.LoadShader();
    }

    public unsafe void LoadShader() {
        this.Shader = Game.Instance.Content.Load<Shader>("shaders/lighting");
        this.LightCount++;
        
        this.Shader.locs[(int) ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW] = ShaderHelper.GetLocationAttribute(this.Shader, "viewPos");

        int ambientLoc = ShaderHelper.GetLocationAttribute(this.Shader, "ambient");
        float[] ambient = new[] { 0.1f, 0.1f, 0.1f, 1.0f };
        ShaderHelper.SetValue(this.Shader, ambientLoc, ambient, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        
        string enabledName = "lights[" + this.LightCount + "].enabled";
        string typeName = "lights[" + this.LightCount + "].type";
        string posName = "lights[" + this.LightCount + "].position";
        string targetName = "lights[" + this.LightCount + "].target";
        string colorName = "lights[" + this.LightCount + "].color";
        
        this.EnabledLoc = ShaderHelper.GetLocationAttribute(this.Shader, enabledName);
        this.TypeLoc = ShaderHelper.GetLocationAttribute(this.Shader, typeName);
        this.PosLoc = ShaderHelper.GetLocationAttribute(this.Shader, posName);
        this.TargetLoc = ShaderHelper.GetLocationAttribute(this.Shader, targetName);
        this.ColorLoc = ShaderHelper.GetLocationAttribute(this.Shader, colorName);
        
        this.UpdateLightValues();
    }
    
    public void UpdateLightValues() {
        // Send to shader light enabled state and type
        ShaderHelper.SetValue(
            this.Shader,
            this.EnabledLoc,
            this.Enabled ? 1 : 0,
            ShaderUniformDataType.SHADER_UNIFORM_INT
        );
        
        ShaderHelper.SetValue(this.Shader, this.TypeLoc, (int) this.Type, ShaderUniformDataType.SHADER_UNIFORM_INT);

        // Send to shader light target position values
        ShaderHelper.SetValue(this.Shader, this.PosLoc, this.Position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);

        // Send to shader light target position values
        ShaderHelper.SetValue(this.Shader, this.TargetLoc, this.Target, ShaderUniformDataType.SHADER_UNIFORM_VEC3);

        // Send to shader light color values
        float[] color = new[] {
            (float) this.Color.r / (float)255,
            (float) this.Color.g / (float)255,
            (float) this.Color.b / (float)255,
            (float) this.Color.a / (float)255
        };
        
        ShaderHelper.SetValue(this.Shader, this.ColorLoc, color, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
    }
    
    public enum LightType {
        Directed,
        Point
    }
}