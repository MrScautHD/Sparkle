#version 450

const float PI = 3.14159265359F;

layout (std140, set = 1, binding = 0) uniform PrefilterBuffer {
    float roughness;
    float sampleCount;
};

layout (set = 2, binding = 0) uniform textureCube fEnvironmentMap;
layout (set = 2, binding = 1) uniform sampler fEnvironmentSampler;

layout (location = 0) in vec3 fLocalPosition;

layout (location = 0) out vec4 fFragColor;

/*
 * Calculates the Van der Corput radical inverse of an integer.
 *
 * The bits are mirrored around the decimal point, which produces a low-discrepancy sequence in the range [0, 1).
 */
float radicalInverseVdc(uint bits) {
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    
    return float(bits) * 2.3283064365386963e-10F;
}

/*
 * Returns the i-th point of an n-point Hammersley set.
 *
 * The points are evenly spread over the unit square, so the integration
 * converges with far fewer samples than purely random numbers would need.
 */
vec2 hammersley(uint i, uint n) {
    return vec2(float(i) / float(n), radicalInverseVdc(i));
}

/*
 * Generates a GGX-distributed halfway vector around the given normal.
 *
 * Samples are concentrated where the GGX distribution has the most energy,
 * which keeps the noise low for glossy surfaces. The result is a unit vector
 * in the same space as the supplied normal.
 *
 * The roughness parameter is named roughnessValue to avoid clashing with the roughness uniform.
 */
vec3 importanceSampleGgx(vec2 xi, vec3 normal, float roughnessValue) {
    float alpha = roughnessValue * roughnessValue;
    
    // Convert the random point into spherical coordinates following the GGX distribution.
    float phi = 2.0F * PI * xi.x;
    float cosTheta = sqrt((1.0F - xi.y) / (1.0F + (alpha * alpha - 1.0F) * xi.y));
    float sinTheta = sqrt(max(1.0F - cosTheta * cosTheta, 0.0F));
    
    // Convert the spherical coordinates into a tangent-space vector.
    vec3 halfway = vec3(
        cos(phi) * sinTheta,
        sin(phi) * sinTheta,
        cosTheta
    );
    
    // Build a tangent frame around the normal, avoiding a parallel up vector.
    vec3 up = abs(normal.z) < 0.999F ? vec3(0.0F, 0.0F, 1.0F) : vec3(1.0F, 0.0F, 0.0F);
    vec3 tangent = normalize(cross(up, normal));
    vec3 bitangent = cross(normal, tangent);
    
    // Rotate the tangent-space vector into the space of the normal.
    return normalize(tangent * halfway.x + bitangent * halfway.y + normal * halfway.z);
}

void main() {
    vec3 normal = normalize(fLocalPosition);
    vec3 reflection = normal;
    vec3 view = reflection;
    
    vec3 prefilteredColor = vec3(0.0F);
    float totalWeight = 0.0F;
    
    uint samples = uint(sampleCount);
    
    for (uint i = 0u; i < samples; i++) {
        
        // Pick a GGX-distributed halfway vector for this sample.
        vec2 xi = hammersley(i, samples);
        vec3 halfway = importanceSampleGgx(xi, normal, roughness);
        
        // Reflect the view vector around the halfway vector to get the light direction.
        vec3 light = normalize(2.0F * dot(view, halfway) * halfway - view);
        
        float nDotL = max(dot(normal, light), 0.0F);
        
        // Light coming from below the surface does not contribute.
        if (nDotL > 0.0F) {
            
            // Weight each sample by NdotL so directions closer to the normal count more.
            prefilteredColor += texture(samplerCube(fEnvironmentMap, fEnvironmentSampler), light).rgb * nDotL;
            totalWeight += nDotL;
        }
    }
    
    // Normalize by the accumulated weight. The divisor is clamped so a texel without valid samples can never divide by zero.
    prefilteredColor /= max(totalWeight, 0.001F);
    
    fFragColor = vec4(prefilteredColor, 1.0F);
}