#version 450

struct Parameters {
    vec2 resolution;
    vec2 pixelSize;
};

layout(set = 0, binding = 0) uniform ParameterBuffer {
    Parameters parameters;
};

layout(set = 1, binding = 0) uniform texture2D fTexture;
layout(set = 1, binding = 1) uniform sampler fTextureSampler;

layout(location = 0) in vec2 fTexCoords;
layout(location = 1) in vec4 fColor;

layout(location = 0) out vec4 fFragColor;

void main() {
    
    // Convert UV to screen coordinates.
    vec2 uvScreen = fTexCoords * parameters.resolution;
    
    // Snap to pixel block.
    vec2 snapped = floor(uvScreen / parameters.pixelSize) * parameters.pixelSize;

    // Convert back to UV space.
    vec2 pixelUV = snapped / parameters.resolution;

    // Sample the texture with snapped UVs.
    fFragColor = texture(sampler2D(fTexture, fTextureSampler), pixelUV) * fColor;
}