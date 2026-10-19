#version 450

layout (std140, set = MATRIX_BUFFER_SET, binding = 0) uniform MatrixBuffer {
    mat4x4 uProjection;
    mat4x4 uView;
};

layout (std140, set = TRANSFORMER_BUFFER_SET, binding = 0) uniform TransformBuffer {
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

layout (location = 0) out vec3 fTerrainPosition;
layout (location = 1) out vec4 fColor;
layout (location = 2) out vec3 fSurfacePosition;
layout (location = 3) noperspective out vec3 fBarycentric;
layout (location = 4) out vec2 fTexCoords;
layout (location = 5) out vec3 fWorldPosition;
layout (location = 6) out vec3 fWorldNormal;
layout (location = 7) out mat3 fTbn;
layout (location = 10) out vec3 fViewPosition;
const float EPSILON = 0.000001F;

vec3 safeNormalize(vec3 value, vec3 fallback) {
    float lengthSquared = dot(value, value);
    return lengthSquared > EPSILON ? value * inversesqrt(lengthSquared) : fallback;
}
void main() {
    fTerrainPosition = vPosition;
    fColor = vColor;
    fTexCoords = vTexCoords;
    
    // Assign barycentric coordinates based on the vertex's corner within its triangle.
    int triangleVertex = gl_VertexIndex % 3;
    
    if (triangleVertex == 0) {
        fBarycentric = vec3(1.0F, 0.0F, 0.0F);
    }
    else if (triangleVertex == 1) {
        fBarycentric = vec3(0.0F, 1.0F, 0.0F);
    }
    else {
        fBarycentric = vec3(0.0F, 0.0F, 1.0F);
    }
    
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
    
    vec3 normal = safeNormalize(normalMatrix * vNormal, vec3(0.0F, 1.0F, 0.0F));
    
    vec3 tangent = normalMatrix * vTangent.xyz;
    tangent = tangent - normal * dot(tangent, normal);
    tangent = safeNormalize(tangent, vec3(1.0F, 0.0F, 0.0F));
    
    if (abs(dot(tangent, normal)) > 0.95F) {
        tangent = vec3(0.0F, 0.0F, 1.0F) - normal * dot(vec3(0.0F, 0.0F, 1.0F), normal);
        tangent = safeNormalize(tangent, vec3(1.0F, 0.0F, 0.0F));
    }
    
    float tangentSign = abs(vTangent.w) > EPSILON ? sign(vTangent.w) : 1.0F;
    vec3 bitangent = safeNormalize(cross(normal, tangent) * tangentSign, vec3(0.0F, 0.0F, 1.0F));
    
    fWorldNormal = normal;
    fTbn = mat3(tangent, bitangent, normal);
    
    // Calculate surface position.
    fSurfacePosition = vPosition - inverse(uView * transformation)[3].xyz;
    
    // Calculate view position.
    fViewPosition = inverse(uView)[3].xyz;
    
    // Calculate final position.
    gl_Position = uProjection * uView * worldPosition;
}