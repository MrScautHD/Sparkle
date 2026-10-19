#version 450

#define MAX_MAPS_COUNT 12

struct MaterialMap {
    vec4 color;
    float value;
};

layout (std140, set = MATERIAL_BUFFER_SET, binding = 0) uniform MaterialBuffer {
    int renderMode; // Render mode: (Solid = 0, Cutout = 1, Translucent = 2).
    MaterialMap maps[MAX_MAPS_COUNT]; // The material parameters.
};

layout (location = 0) in vec3 fTerrainPosition;
layout (location = 1) in vec4 fColor;
layout (location = 2) in vec3 fSurfacePosition;
layout (location = 3) noperspective in vec3 fBarycentric;

layout (location = 0) out vec4 fFragColor;

#include <sparkle/include/terrain_tiles.glsl>

void main() {
    vec4 color = GetTerrainColor();
    
    // Apply vertex color and global material tint.
    vec4 texelColor = color * fColor * maps[0].color;
    
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
}