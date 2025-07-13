#version 450

struct Parameters {
    vec2 resolution;
    float reduceMin;
    float reduceMul;
    float spanMax;
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
    
    // Calculate the inverse of the screen resolution.
    vec2 inverseResolution = 1.0F / parameters.resolution;
    
    // Sample neighboring pixels based on the resolution.
    vec3 rgbNW = texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(-1.0F, -1.0F) * inverseResolution).xyz;
    vec3 rgbNE = texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(1.0F, -1.0F) * inverseResolution).xyz;
    vec3 rgbSW = texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(-1.0F, 1.0F) * inverseResolution).xyz;
    vec3 rgbSE = texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(1.0F, 1.0F) * inverseResolution).xyz;
    
    // Sample the central pixel.
    vec3 rgbM = texture(sampler2D(fTexture, fTextureSampler), fTexCoords).xyz;
    
    // Define luminance weights (standard for RGB to grayscale conversion).
    const vec3 lumaWeights = vec3(0.299F, 0.587F, 0.114F);
    
    // Compute luminance for each sampled pixel.
    float lumaNW = dot(rgbNW, lumaWeights);
    float lumaNE = dot(rgbNE, lumaWeights);
    float lumaSW = dot(rgbSW, lumaWeights);
    float lumaSE = dot(rgbSE, lumaWeights);
    float lumaM = dot(rgbM, lumaWeights);
    
    // Determine the minimum and maximum luminance values.
    float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
    float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));
    
    // Calculate the direction vector based on luminance differences.
    vec2 dir = vec2(-(lumaNW + lumaNE - lumaSW - lumaSE), lumaNW + lumaSW - lumaNE - lumaSE);
    
    // Normalize the direction vector with reduction to avoid instability.
    float dirReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * (0.25F * parameters.reduceMul), parameters.reduceMin);
    float rcpDirMin = 1.0F / (min(abs(dir.x), abs(dir.y)) + dirReduce);
    dir = clamp(dir * rcpDirMin, -parameters.spanMax, parameters.spanMax) * inverseResolution;
    
    // Sample along the calculated direction for antialiasing.
    vec3 rgbA = 0.5F * (texture(sampler2D(fTexture, fTextureSampler), fTexCoords + dir * (1.0F / 3.0F - 0.5F)).xyz + texture(sampler2D(fTexture, fTextureSampler), fTexCoords + dir * (2.0F / 3.0F - 0.5F)).xyz);
    vec3 rgbB = rgbA * 0.5F + 0.25F * (texture(sampler2D(fTexture, fTextureSampler), fTexCoords + dir * -0.5F).xyz + texture(sampler2D(fTexture, fTextureSampler), fTexCoords + dir * 0.5F).xyz);
    
    // Compute luminance for the final blended colors.
    float lumaB = dot(rgbB, lumaWeights);
    
    // Pick the color based on luminance range checks.
    fFragColor = (lumaB < lumaMin || lumaB > lumaMax) ? vec4(rgbA, 1.0F) : vec4(rgbB, 1.0F);
}