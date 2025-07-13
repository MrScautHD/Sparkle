#version 450

struct Parameters {
    vec2 resolution;
};

layout(set = 0, binding = 0) uniform ParameterBuffer {
    Parameters parameters;
};

layout(set = 1, binding = 0) uniform texture2D fTexture;
layout(set = 1, binding = 1) uniform sampler fTextureSampler;

layout(location = 0) in vec2 fTexCoords;
layout(location = 1) in vec4 fColor;

layout(location = 0) out vec4 fFragColor;

const vec3 colors[6] = vec3[6](
    // Blue.
    vec3(0.0F, 0.0F, 1.0F),
    // Cyan.
    vec3(0.0F, 1.0F, 1.0F),
    // Green.
    vec3(0.0F, 1.0F, 0.0F),
    // Yellow.
    vec3(1.0F, 1.0F, 0.0F),
    // Red.
    vec3(1.0F, 0.0F, 0.0F),
    // White.
    vec3(1.0F, 1.0F, 1.0F)
);

// Luminance is a weighted sum derived from RGB values.
float computeLuminance(vec3 color) {
    return dot(color, vec3(0.2126F, 0.7152F, 0.0722F));
}

// Function to compute a thermal gradient based on an input value.
vec3 thermalGradient(float value) {
    
    // Clamp input range.
    value = clamp(value, 0.0F, 1.0F);
    
    // Scale the value to map to indices in the color array.
    float scaled = value * 5.0F;
    int index = int(floor(scaled));
    float t = fract(scaled);
    
    // Clamp index to avoid out-of-bounds access.
    index = clamp(index, 0, 4);
    
    // Interpolate between two gradient colors
    return mix(colors[index], colors[index + 1], t);
}

void main() {
    vec3 texelColor = texture(sampler2D(fTexture, fTextureSampler), fTexCoords).rgb;
    float luminance = computeLuminance(texelColor);
    vec3 thermalColor = thermalGradient(luminance);
    
    fFragColor = vec4(thermalColor, 1.0F);
}