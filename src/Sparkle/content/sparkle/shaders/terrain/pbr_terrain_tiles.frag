#version 450

#define MAX_MAPS_COUNT 12

#define EPSILON 0.000001F

const int MAP_ALBEDO = 0;
const int MAP_NORMAL = 1;
const int MAP_METALLIC = 2;
const int MAP_ROUGHNESS = 3;
const int MAP_OCCLUSION = 4;
const int MAP_EMISSION = 5;
const int MAP_REFLECTION = 6;
const int MAP_SSR_REFLECTION = 7;

struct MaterialMap {
    vec4 color;
    float value;
};

layout (std140, set = MATERIAL_BUFFER_SET, binding = 0) uniform MaterialBuffer {
    int renderMode;
    MaterialMap maps[MAX_MAPS_COUNT];
};

layout (set = ALBEDO_TEXTURE_SET, binding = 0) uniform texture2D fAlbedo;
layout (set = ALBEDO_TEXTURE_SET, binding = 1) uniform sampler fAlbedoSampler;

layout (location = 0) in vec3 fTerrainPosition;
layout (location = 1) in vec4 fColor;
layout (location = 2) in vec3 fSurfacePosition;
layout (location = 3) noperspective in vec3 fBarycentric;
layout (location = 4) in vec2 fTexCoords;
layout (location = 5) in vec3 fWorldPosition;
layout (location = 6) in vec3 fWorldNormal;
layout (location = 7) in mat3 fTbn;
layout (location = 10) in vec3 fViewPosition;

layout (location = 0) out vec4 fFragColor;
layout (location = 1) out vec4 fSsrMaskColor;
layout (location = 2) out vec4 fBloomMaskColor;

#include <sparkle/include/pbr.glsl>
#include <sparkle/include/terrain_tiles.glsl>

/*
 * Safely normalizes a vector using a fallback for near-zero vectors.
 */
vec3 safeNormalize(vec3 value, vec3 fallback) {
    float lengthSquared = dot(value, value);
    return lengthSquared > EPSILON ? value * inversesqrt(lengthSquared) : fallback;
}

/*
 * Calculates a stable terrain normal by constructing an orthonormal TBN basis, applying the terrain normal map, and transforming it into world space.
 */
vec3 getSafeTerrainNormal() {
    vec3 normal = safeNormalize(fWorldNormal, vec3(0.0F, 1.0F, 0.0F));
    
    vec3 tangent = fTbn[0];
    
    // Remove any component of tangent that points along the normal.
    tangent = tangent - normal * dot(tangent, normal);
    tangent = safeNormalize(tangent, vec3(1.0F, 0.0F, 0.0F));
    
    // If the fallback tangent is almost parallel to the normal, use Z instead.
    if (abs(dot(tangent, normal)) > 0.95F) {
        tangent = safeNormalize(vec3(0.0F, 0.0F, 1.0F) - normal * dot(vec3(0.0F, 0.0F, 1.0F), normal), vec3(1.0F, 0.0F, 0.0F));
    }
    
    vec3 bitangent = safeNormalize(cross(normal, tangent), vec3(0.0F, 0.0F, 1.0F));
    
    mat3 tbn = mat3(tangent, bitangent, normal);
    
    vec3 sampledNormal = texture(sampler2D(fNormal, fNormalSampler), fTexCoords).rgb;
    vec3 decodedNormal = sampledNormal * 2.0F - 1.0F;
    
    // If the sampled normal texture is invalid/black-ish, fall back to neutral tangent-space normal.
    decodedNormal = safeNormalize(decodedNormal, vec3(0.0F, 0.0F, 1.0F));
    
    decodedNormal.xy *= maps[MAP_NORMAL].value;
    
    return safeNormalize(tbn * decodedNormal, normal);
}

/*
 * Calculates the physically based terrain color by combining material properties,
 * direct and image-based lighting, ambient illumination, and emission, then applying Reinhard tone mapping and gamma correction.
 */
vec4 getTerrainPbrColor(vec4 terrainTexel) {
    
    // Combine the terrain material color with the vertex and material tints.
    vec3 albedo = terrainTexel.rgb * fColor.rgb * maps[MAP_ALBEDO].color.rgb;
    float alpha = terrainTexel.a * fColor.a * maps[MAP_ALBEDO].color.a;
    
    // Read the material properties.
    float metallic = clamp(maps[MAP_METALLIC].value, 0.0F, 1.0F);
    float roughness = clamp(maps[MAP_ROUGHNESS].value, 0.04F, 1.0F);
    float ambientOcclusion = clamp(maps[MAP_OCCLUSION].value, 0.0F, 1.0F);
    float reflectionStrength = clamp(maps[MAP_REFLECTION].value, 0.0F, 1.0F);
    
    // Use the geometric terrain normal.
    vec3 normal = getSafeTerrainNormal();
    
    // Calculate the direction from the surface toward the camera.
    vec3 viewDirection = safeNormalize(fViewPosition - fWorldPosition, vec3(0.0F, 0.0F, 1.0F));
    
    // Evaluate direct lighting and shadowing.
    float shadowFactor = getShadowFactor(normal);
    
    vec3 directLighting = getDirectLightColor(albedo, normal, viewDirection, metallic, roughness, reflectionStrength, shadowFactor);
    
    // Evaluate image-based lighting, including ambient occlusion.
    vec3 ambientLighting = getImageBasedLightColor(albedo, normal, viewDirection, metallic, roughness, ambientOcclusion, reflectionStrength);
    
    // Add the flat ambient contribution for non-metallic surfaces.
    ambientLighting += ambientColor.rgb * ambientColor.w * albedo * (1.0F - metallic) * ambientOcclusion;
    
    // Emission is independent of direct lighting and shadowing.
    vec3 emission = getEmissionColor();
    
    // Combine the lighting contributions in linear color space.
    vec3 finalColor = ambientLighting + directLighting + emission;
    
    // Apply Reinhard tone mapping.
    finalColor = finalColor / (finalColor + vec3(1.0F));
    
    // Convert the tone-mapped color to display space.
    finalColor = pow(finalColor, vec3(1.0F / 2.2F));
    
    return vec4(finalColor, alpha);
}

void main() {
    vec4 color = GetTerrainColor();
    
    // Light the terrain through the PBR path.
    vec4 texelColor = getTerrainPbrColor(color);
    
    // Overlay the brush wire preview if enabled.
    texelColor = applyBrushWirePreview(texelColor, fTerrainPosition);
    
    // Set render mode.
    switch (renderMode) {
        
        // Solid.
        case 0:
            texelColor.a = 1.0F;
            break;
        
        // Cutout.
        case 1:
            if (texelColor.a < 0.99F) {
                discard;
            }
            break;
    }
    
    fFragColor = texelColor;
    fSsrMaskColor = vec4(clamp(maps[MAP_SSR_REFLECTION].value, 0.0F, 1.0F), 0.0F, 0.0F, 1.0F);
    fBloomMaskColor = vec4(getEmissionColor(), 1.0F);
}