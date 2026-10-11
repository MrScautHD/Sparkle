#version 450

layout (location = 0) in vec3 fWorldNormal;

layout (location = 0) out vec4 fNormalColor;

void main() {
    vec3 normal = normalize(fWorldNormal);
    fNormalColor = vec4(normal * 0.5F + 0.5F, 1.0F);
}