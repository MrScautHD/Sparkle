#version 450

struct Parameters {
    vec2 resolution;
    float samples;
    float quality;
};

layout(set = 0, binding = 0) uniform ParameterBuffer {
    Parameters parameters;
};

layout (set = 1, binding = 0) uniform texture2D fTexture;
layout (set = 1, binding = 1) uniform sampler fTextureSampler;

layout (location = 0) in vec2 fTexCoords;
layout (location = 1) in vec4 fColor;

layout (location = 0) out vec4 fFragColor;

void main() {
    vec4 texelColor = texture(sampler2D(fTexture, fTextureSampler), fTexCoords);
    vec2 sizeFactor = vec2(1) / parameters.resolution * parameters.quality;
    
    vec4 sum = vec4(0.0F);
    int range = int((parameters.samples - 1.0F) / 2.0F);
    
    for (int x = -range; x <= range; x++) {
        for (int y = -range; y <= range; y++) {
            sum += texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(x, y) * sizeFactor);
        }
    }
    
    fFragColor = ((sum / (parameters.samples * parameters.samples)) + texelColor) * fColor;
}