#version 450

layout (set = 0, binding = 0) uniform texture2D fSceneColor;
layout (set = 0, binding = 1) uniform sampler fSceneColorSampler;

layout (set = 1, binding = 0) uniform texture2D fSsrColor;
layout (set = 1, binding = 1) uniform sampler fSsrColorSampler;

layout (set = 2, binding = 0) uniform texture2D fSsrMask;
layout (set = 2, binding = 1) uniform sampler fSsrMaskSampler;

layout (location = 0) in vec2 fTexCoords;

layout (location = 0) out vec4 fFragColor;

/*
 * Reads a single SSR texel without filtering and converts it to premultiplied alpha.
 *
 * Premultiplying before interpolation prevents the color of fully transparent
 * texels (misses) from bleeding into neighboring reflections. The coordinate
 * is clamped to the texture bounds, which emulates clamp-to-edge addressing.
 */
vec4 FetchPremul(ivec2 p, ivec2 size) {
    vec4 c = texelFetch(sampler2D(fSsrColor, fSsrColorSampler), clamp(p, ivec2(0), size - 1), 0);
    return vec4(c.rgb * c.a, c.a);
}

void main() {
    vec2 uv = clamp(fTexCoords, vec2(0.0F), vec2(1.0F));
    vec3 sceneColor = texture(sampler2D(fSceneColor, fSceneColorSampler), uv).rgb;
    
    // The SSR texture has its own resolution, so the bilinear filtering is done manually (see FetchPremul for why).
    ivec2 size = textureSize(sampler2D(fSsrColor, fSsrColorSampler), 0);
    
    // Continuous position in texel space. The -0.5 aligns the position to texel centers, so that floor() returns the top-left texel of the 2x2 block that surrounds the pixel.
    vec2 texelPos = uv * vec2(size) - 0.5F;
    ivec2 baseTexel = ivec2(floor(texelPos));
    
    // The sub-texel offset doubles as the bilinear weights (x = right, y = down).
    vec2 blendFactor = fract(texelPos);
    
    // Bilinear interpolation: blend horizontally along both rows, then vertically between the rows.
    vec4 ssr = mix(
        mix(FetchPremul(baseTexel, size), FetchPremul(baseTexel + ivec2(1, 0), size), blendFactor.x),
        mix(FetchPremul(baseTexel + ivec2(0, 1), size), FetchPremul(baseTexel + ivec2(1, 1), size), blendFactor.x),
        blendFactor.y
    );
    
    // The mask is read at full resolution and decides how strongly this material shows reflections.
    float mask = clamp(texture(sampler2D(fSsrMask, fSsrMaskSampler), uv).r, 0.0F, 1.0F);
    
    // Premultiplied "over": dim the scene by the reflection opacity, then add the already premultiplied reflection color.
    vec3 color = sceneColor * (1.0F - ssr.a * mask) + ssr.rgb * mask;
    
    fFragColor = vec4(color, 1.0F);
}