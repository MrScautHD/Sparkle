#version 450

layout(set = 0, binding = 0) uniform ProjectionViewBuffer {
    mat4x4 uProjection;
    mat4x4 uView;
};

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec4 vColor;

layout (location = 0) out vec3 fTexCoords;
layout (location = 1) out vec4 fColor;

void main() {
    fTexCoords = vPosition;
    fColor = vColor;

    vec4 v4Pos = vec4(vPosition, 1.0F);
    gl_Position = uProjection * mat4(mat3(uView)) * v4Pos;
}