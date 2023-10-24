using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.entity; 

public class DirectedLight : Entity {

    public Shader Shader;
    
    public DirectedLight(Vector3 position) : base(position) { } // OR MAYBE GIVE HIM THE SHADER HERE

    protected internal override void Init() {
        base.Init();
        /*

        unsafe {
            // TODO MOVE THAT TO THE MAIN CLASS OR TO A SHADER MANAGER OR CONTENTMANAGER
            this.Shader = ShaderHelper.Load("lighting.vs", "fog.fs");
            this.Shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_MODEL] = ShaderHelper.GetLocationAttribute(this.Shader, "matModel");
            this.Shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW] = ShaderHelper.GetLocationAttribute(this.Shader, "viewPos");
        }
        
        int ambientLoc = ShaderHelper.GetLocationAttribute(this.Shader, "ambient");
        ShaderHelper.SetValue<>(this.Shader, ambientLoc, new float[] { 0.2f, 0.2f, 0.2f, 1.0f }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);*/
    }
}