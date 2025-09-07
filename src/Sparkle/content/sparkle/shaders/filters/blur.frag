#version 450
#define MAX_RADIUS 15

struct Parameters {
    vec2 resolution;
    float intensity;
    int radius;
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
    int radius = clamp(parameters.radius, 1, MAX_RADIUS);
    vec2 texelSize = 1.0F / parameters.resolution;
    float sigma = max(parameters.intensity, 0.01F);
    float twoSigmaSq = 2.0F * sigma * sigma;
    
    float weightSumH = 0.0F;
    vec4 colorSumH = vec4(0.0F);
    
    // Horizontal blur.
    for (int x = -radius; x <= radius; ++x) {
        vec2 offset = vec2(x, 0.0F) * texelSize;
        float distSq = float(x * x);
        float weight = exp(-distSq / twoSigmaSq);
        
        vec2 sampleCoord = clamp(fTexCoords + offset, vec2(0.0F), vec2(1.0F));
        vec4 texColor = texture(sampler2D(fTexture, fTextureSampler), sampleCoord);
        colorSumH += texColor * weight;
        weightSumH += weight;
    }
    
    float weightSumV = 0.0F;
    vec4 colorSumV = vec4(0.0F);
    
    // Vertical blur.
    for (int y = -radius; y <= radius; ++y) {
        vec2 offset = vec2(0.0F, y) * texelSize;
        float distSq = float(y * y);
        float weight = exp(-distSq / twoSigmaSq);

        vec2 sampleCoord = clamp(fTexCoords + offset, vec2(0.0F), vec2(1.0F));
        vec4 texColor = texture(sampler2D(fTexture, fTextureSampler), sampleCoord);
        colorSumV += texColor * weight;
        weightSumV += weight;
    }
    
    // Combine horizontal and vertical blur by averaging.
    fFragColor = (colorSumH / weightSumH + colorSumV / weightSumV) * 0.5F;
}