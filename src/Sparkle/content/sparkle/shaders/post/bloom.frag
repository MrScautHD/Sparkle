#version 450

layout (std140, set = 0, binding = 0) uniform BloomBuffer {
    vec2 screenSize;
    float samples;
    float quality;
    float strength;
    float threshold;
    vec2 direction;
};

layout (set = 1, binding = 0) uniform texture2D fBloomMask;
layout (set = 1, binding = 1) uniform sampler fBloomMaskSampler;

layout (location = 0) in vec2 fTexCoords;

layout (location = 0) out vec4 fFragColor;

/*
 * Keeps only the bright part of a color.
 *
 * The brightness is the largest color channel, so a saturated color such as pure red
 * counts as bright. The mask rises smoothly over a range of 0.25 above the threshold,
 * so surfaces do not pop in and out of the glow when their brightness crosses it.
 */
vec3 thresholdBloom(vec3 color) {
    float brightness = max(max(color.r, color.g), color.b);
    float mask = smoothstep(threshold, threshold + 0.25F, brightness);
    return color * mask;
}

void main() {
    vec2 texelSize = vec2(1.0F) / max(screenSize, vec2(1.0F));
    vec2 blurStep = direction * texelSize * quality;
    
    vec3 color = thresholdBloom(texture(sampler2D(fBloomMask, fBloomMaskSampler), fTexCoords).rgb) * 0.227027F;
    
    color += thresholdBloom(texture(sampler2D(fBloomMask, fBloomMaskSampler), fTexCoords + blurStep * 1.384615F).rgb) * 0.316216F;
    color += thresholdBloom(texture(sampler2D(fBloomMask, fBloomMaskSampler), fTexCoords - blurStep * 1.384615F).rgb) * 0.316216F;
    
    color += thresholdBloom(texture(sampler2D(fBloomMask, fBloomMaskSampler), fTexCoords + blurStep * 3.230769F).rgb) * 0.070270F;
    color += thresholdBloom(texture(sampler2D(fBloomMask, fBloomMaskSampler), fTexCoords - blurStep * 3.230769F).rgb) * 0.070270F;
    
    fFragColor = vec4(color * strength, 1.0F);
}