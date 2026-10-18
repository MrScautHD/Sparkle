#version 450

#define MAX_MAPS_COUNT 12

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
    int renderMode; // Render mode: (Solid = 0, Cutout = 1, Translucent = 2).
    MaterialMap maps[MAX_MAPS_COUNT]; // The material parameters.
};

layout (set = ALBEDO_TEXTURE_SET, binding = 0) uniform texture2D fAlbedo;
layout (set = ALBEDO_TEXTURE_SET, binding = 1) uniform sampler fAlbedoSampler;

layout (location = 0) in vec2 fTexCoords;
layout (location = 1) in vec4 fColor;
layout (location = 2) in vec3 fWorldPosition;
layout (location = 3) in vec3 fWorldNormal;
layout (location = 4) in mat3 fTbn;
layout (location = 7) in vec3 fViewPosition;

layout (location = 0) out vec4 fFragColor;
layout (location = 1) out vec4 fSsrMaskColor;
layout (location = 2) out vec4 fBloomMaskColor;

#include <sparkle/include/pbr.glsl>

void main() {
    vec4 texelColor = getPbrColor();
    
    // The SSR pass reads this mask to decide how reflective this pixel is.
    float ssrReflectionMask = clamp(maps[MAP_SSR_REFLECTION].value, 0.0F, 1.0F);
    
    // Raw emission bloom source.
    vec3 bloomColor = getEmissionColor();
    
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
    fSsrMaskColor = vec4(ssrReflectionMask, 0.0F, 0.0F, 1.0F);
    fBloomMaskColor = vec4(bloomColor, 1.0F);
}