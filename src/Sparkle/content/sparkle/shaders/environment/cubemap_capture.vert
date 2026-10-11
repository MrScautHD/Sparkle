#version 450

layout (std140, set = 0, binding = 0) uniform MatrixBuffer {
    mat4x4 uProjection;
    mat4x4 uView;
};

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec4 vColor;

layout (location = 0) out vec3 fLocalPosition;

void main() {
    fLocalPosition = vPosition;
    gl_Position = uProjection * uView * vec4(vPosition, 1.0F);
}