#version 450

layout (location = 0) out vec2 fTexCoords;

void main() {
    vec2 positions[3] = vec2[](
        vec2(-1.0F, -1.0F),
        vec2(3.0F, -1.0F),
        vec2(-1.0F, 3.0F)
    );
    
    vec2 texCoords[3] = vec2[](
        vec2(0.0F, 1.0F),
        vec2(2.0F, 1.0F),
        vec2(0.0F, -1.0F)
    );
    
    gl_Position = vec4(positions[gl_VertexIndex], 0.0F, 1.0F);
    fTexCoords = texCoords[gl_VertexIndex];
}