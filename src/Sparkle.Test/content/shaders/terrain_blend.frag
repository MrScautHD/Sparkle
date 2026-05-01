#version 450

#define MAX_MAPS_COUNT 8

struct MaterialMap {
    vec4 color;
    float value;
};

layout(std140, set = 2, binding = 0) uniform MaterialBuffer {
    int renderMode;
    MaterialMap maps[MAX_MAPS_COUNT];
};

layout (set = 3, binding = 0) uniform texture2D fAlbedo;
layout (set = 3, binding = 1) uniform sampler fAlbedoSampler;
layout (set = 4, binding = 0) uniform texture2D fMetallic;
layout (set = 4, binding = 1) uniform sampler fMetallicSampler;
layout (set = 5, binding = 0) uniform texture2D fRoughness;
layout (set = 5, binding = 1) uniform sampler fRoughnessSampler;

layout (location = 0) in vec2 fTexCoords;
layout (location = 1) in vec3 fWorldPos;
layout (location = 2) in vec3 fWorldNormal;

layout (location = 0) out vec4 fFragColor;

void main() {
    vec3 grass = texture(sampler2D(fAlbedo, fAlbedoSampler), fTexCoords).rgb;
    vec3 dirt = texture(sampler2D(fMetallic, fMetallicSampler), fTexCoords).rgb;
    vec3 rock = texture(sampler2D(fRoughness, fRoughnessSampler), fTexCoords).rgb;

    float up = clamp(abs(normalize(fWorldNormal).y), 0.0, 1.0);
    float slope = 1.0 - up;
    float heightY = fWorldPos.y;

    float rockBySlope = smoothstep(0.22, 0.65, slope);
    float rockByHeight = smoothstep(11.0, 20.0, heightY);
    float rockWeight = max(rockBySlope, rockByHeight);

    float dirtBand = smoothstep(4.0, 10.0, heightY) * (1.0 - smoothstep(14.0, 20.0, heightY));
    float dirtWeight = dirtBand * (1.0 - rockWeight);
    float grassWeight = max(0.0, 1.0 - rockWeight - dirtWeight);

    float weightSum = max(0.0001, grassWeight + dirtWeight + rockWeight);
    grassWeight /= weightSum;
    dirtWeight /= weightSum;
    rockWeight /= weightSum;

    vec3 blended = (grass * grassWeight) + (dirt * dirtWeight) + (rock * rockWeight);
    fFragColor = vec4(blended, 1.0);
}
