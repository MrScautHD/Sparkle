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

void main() {
    float x = 1.0F / parameters.resolution.x;
    float y = 1.0F / parameters.resolution.y;
    
    vec4 horizEdge = vec4(0.0F);
    horizEdge -= texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(-x, -y)) * 1.0F;
    horizEdge -= texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(-x, 0.0F)) * 2.0F;
    horizEdge -= texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(-x, y)) * 1.0F;
    horizEdge += texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(x, -y)) * 1.0F;
    horizEdge += texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(x, 0.0F)) * 2.0F;
    horizEdge += texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(x, y)) * 1.0F;
    
    vec4 vertEdge = vec4(0.0F);
    vertEdge -= texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(-x, -y)) * 1.0F;
    vertEdge -= texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(0.0F, -y)) * 2.0F;
    vertEdge -= texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(x, -y)) * 1.0F;
    vertEdge += texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(-x, y)) * 1.0F;
    vertEdge += texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(0.0F, y)) * 2.0F;
    vertEdge += texture(sampler2D(fTexture, fTextureSampler), fTexCoords + vec2(x, y)) * 1.0F;
    
    vec3 edge = sqrt((horizEdge.rgb * horizEdge.rgb) + (vertEdge.rgb * vertEdge.rgb));
    float alpha = texture(sampler2D(fTexture, fTextureSampler), fTexCoords).a;
    
    fFragColor = vec4(edge, alpha);
}