#version 450

#define MAX_MAPS_COUNT 8

struct MaterialMap {
    vec4 color;
    float value;
};

layout(std140, set = 3, binding = 0) uniform MaterialBuffer {
    int renderMode;
    MaterialMap maps[MAX_MAPS_COUNT];
};

layout (set = 4, binding = 0) uniform texture2D fAlbedo;
layout (set = 4, binding = 1) uniform sampler fAlbedoSampler;

layout (location = 0) in vec2 fTexCoords;
layout (location = 1) in vec4 fColor;

layout (location = 0) out float fDepth;

void main() {
    vec4 texelColor = texture(sampler2D(fAlbedo, fAlbedoSampler), fTexCoords);
    float alpha = texelColor.a * fColor.a * maps[0].color.a;
    
    // Set render mode.
    switch (renderMode) {
        
        // Solid.
        case 0:
            break;
        
        // Cutout.
        case 1:
            if (alpha < 0.99F) {
                discard;
            }
            break;
        
        // Translucent.
        case 2:
            discard;
            break;
    }
    
    fDepth = gl_FragCoord.z;
}