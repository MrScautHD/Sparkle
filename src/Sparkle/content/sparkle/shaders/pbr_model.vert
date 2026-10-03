#version 450

layout(std140, set = 0, binding = 0) uniform MatrixBuffer {
    mat4x4 uProjection;
    mat4x4 uView;
};

layout(std140, set = 1, binding = 0) uniform TransformBuffer {
    mat4x4 uTransformation;
};

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec2 vTexCoords;
layout (location = 2) in vec2 vTexCoords2;
layout (location = 3) in vec3 vNormal;
layout (location = 4) in vec4 vTangent;
layout (location = 5) in vec4 vColor;

#if USE_INSTANCING
layout (location = 6) in vec4 iModel0;
layout (location = 7) in vec4 iModel1;
layout (location = 8) in vec4 iModel2;
layout (location = 9) in vec4 iModel3;
#endif

layout(location = 0) out vec2 fTexCoords;
layout(location = 1) out vec4 fColor;
layout(location = 2) out vec3 fWorldPosition;
layout(location = 3) out vec3 fWorldNormal;
layout(location = 4) out mat3 fTbn;
layout(location = 7) out vec3 fViewPosition;

void main() {
    fTexCoords = vTexCoords;
    fColor = vColor;

    // Calculate transformion.
    #if USE_INSTANCING
    mat4x4 transformation = mat4x4(iModel0, iModel1, iModel2, iModel3);
    #else
    mat4x4 transformation = uTransformation;
    #endif

    // Calculate world position.
    vec4 worldPosition = transformation * vec4(vPosition, 1.0F);
    fWorldPosition = worldPosition.xyz;

    // Calculate normal matrix.
    mat3 normalMatrix = transpose(inverse(mat3(transformation)));

    vec3 normal = normalize(normalMatrix * vNormal);
    vec3 tangent = normalize(normalMatrix * vTangent.xyz);
    vec3 bitangent = normalize(cross(normal, tangent) * vTangent.w);

    fWorldNormal = normal;
    fTbn = mat3(tangent, bitangent, normal);

    // Calculate view position.
    fViewPosition = inverse(uView)[3].xyz;

    // Calculate final position.
    gl_Position = uProjection * uView * worldPosition;
}