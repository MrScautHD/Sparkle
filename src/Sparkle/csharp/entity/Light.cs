using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.entity; 
/*
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
        this.Shader = ShaderHelper.Load("content/shaders/lighting.vs", "content/shaders/lighting.fs");

        this.Shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW] = ShaderHelper.GetLocationAttribute(this.Shader, "viewPos");

        int ambientLoc = ShaderHelper.GetLocationAttribute(this.Shader, "ambient");
        float[] ambient = new[] { 0.1f, 0.1f, 0.1f, 1.0f };
        Raylib.SetShaderValue(this.Shader, ambientLoc, ambient, ShaderUniformDataType.SHADER_UNIFORM_VEC4);

        // MODEL NEED TO SET TO A SHADER!
        
        string enabledName = "lights[" + this.Id + "].enabled";
        string typeName = "lights[" + this.Id + "].type";
        string posName = "lights[" + this.Id + "].position";
        string targetName = "lights[" + this.Id + "].target";
        string colorName = "lights[" + this.Id + "].color";
        
        this.EnabledLoc = ShaderHelper.GetLocationAttribute(this.Shader, enabledName);
        this.TypeLoc = ShaderHelper.GetLocationAttribute(this.Shader, typeName);
        this.PosLoc = ShaderHelper.GetLocationAttribute(this.Shader, posName);
        this.TargetLoc = ShaderHelper.GetLocationAttribute(this.Shader, targetName);
        this.ColorLoc = ShaderHelper.GetLocationAttribute(this.Shader, colorName);
        
        this.UpdateLightValues();
    }
    
    public void UpdateLightValues() {
        // Send to shader light enabled state and type
        Raylib.SetShaderValue(
            this.Shader,
            this.EnabledLoc,
            this.Enabled ? 1 : 0,
            ShaderUniformDataType.SHADER_UNIFORM_INT
        );
        
        Raylib.SetShaderValue(this.Shader, this.TypeLoc, (int) this.Type, ShaderUniformDataType.SHADER_UNIFORM_INT);

        // Send to shader light target position values
        Raylib.SetShaderValue(this.Shader, this.PosLoc, this.Position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);

        // Send to shader light target position values
        Raylib.SetShaderValue(this.Shader, this.TargetLoc, this.Target, ShaderUniformDataType.SHADER_UNIFORM_VEC3);

        // Send to shader light color values
        float[] color = new[] {
            (float) this.Color.r / (float)255,
            (float) this.Color.g / (float)255,
            (float) this.Color.b / (float)255,
            (float) this.Color.a / (float)255
        };
        
        Raylib.SetShaderValue(this.Shader, this.ColorLoc, color, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
    }
    
    public enum LightType {
        Directorional,
        Point
    }
}
*/