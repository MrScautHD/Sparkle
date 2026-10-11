#version 450

const float PI = 3.14159265359F;
const uint SAMPLE_COUNT = 1024;

layout (location = 0) in vec2 fTexCoords;

layout (location = 0) out vec2 fFragColor;

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
 */
vec3 importanceSampleGgx(vec2 xi, vec3 normal, float roughness) {
    float alpha = roughness * roughness;
    
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

/*
 * Evaluates the Schlick-GGX geometry term for a single direction.
 *
 * Describes how much of the surface is not shadowed by its own microfacets.
 * The IBL variant of k (alpha / 2) is used instead of the direct lighting
 * variant, because this LUT is only used for image-based lighting.
 */
float geometrySchlickGgx(float nDotV, float roughness) {
    float a = roughness;
    float k = (a * a) / 2.0F;
    
    // The denominator is clamped so grazing angles can never divide by zero.
    return nDotV / max(nDotV * (1.0F - k) + k, 0.001F);
}

/*
 * Evaluates the Smith geometry term for a view and light direction pair.
 *
 * Shadowing (light) and masking (view) are treated as independent, so the
 * combined term is the product of both Schlick-GGX terms.
 */
float geometrySmith(vec3 normal, vec3 view, vec3 light, float roughness) {
    float nDotV = max(dot(normal, view), 0.0F);
    float nDotL = max(dot(normal, light), 0.0F);
    
    float ggxV = geometrySchlickGgx(nDotV, roughness);
    float ggxL = geometrySchlickGgx(nDotL, roughness);
    
    return ggxV * ggxL;
}

/*
 * Integrates the specular BRDF for one NdotV and roughness pair.
 *
 * Returns the scale (x) and bias (y) that are applied to F0 later on.
 * The integral is solved with importance sampling in tangent space, where
 * the surface normal always points along +Z.
 */
vec2 integrateBrdf(float nDotV, float roughness) {
    
    // Build the view vector from NdotV, placed in the XZ plane. The BRDF is isotropic, so the azimuth does not matter.
    vec3 view = vec3(
        sqrt(max(1.0F - nDotV * nDotV, 0.0F)),
        0.0F,
        nDotV
    );
    
    // Accumulators for the scale (a) and bias (b) terms.
    float a = 0.0F;
    float b = 0.0F;
    
    vec3 normal = vec3(0.0F, 0.0F, 1.0F);
    
    for (uint i = 0u; i < SAMPLE_COUNT; i++) {
        
        // Pick a GGX-distributed halfway vector for this sample.
        vec2 xi = hammersley(i, SAMPLE_COUNT);
        vec3 halfway = importanceSampleGgx(xi, normal, roughness);
        
        // Reflect the view vector around the halfway vector to get the light direction.
        vec3 light = normalize(2.0F * dot(view, halfway) * halfway - view);
        
        float nDotL = max(light.z, 0.0F);
        float nDotH = max(halfway.z, 0.0F);
        float vDotH = max(dot(view, halfway), 0.0F);
        
        // Light coming from below the surface does not contribute.
        if (nDotL > 0.0F) {
            float geometry = geometrySmith(normal, view, light, roughness);
            
            // Combine the geometry term with the sampling weights (the PDF is already folded in).
            float geometryVisible = geometry * vDotH / max(nDotH * nDotV, 0.001F);
            
            // Schlick Fresnel weight, split so F0 can be factored out afterwards.
            float fresnel = pow(1.0F - vDotH, 5.0F);
            
            // Scale covers the part multiplied by F0, bias covers the constant part.
            a += (1.0F - fresnel) * geometryVisible;
            b += fresnel * geometryVisible;
        }
    }
    
    // Average over all samples to finish the Monte Carlo estimate.
    a /= float(SAMPLE_COUNT);
    b /= float(SAMPLE_COUNT);
    
    return vec2(a, b);
}

void main() {
    float nDotV = clamp(fTexCoords.x, 0.0F, 1.0F);
    float roughness = clamp(fTexCoords.y, 0.0F, 1.0F);
    
    fFragColor = integrateBrdf(nDotV, roughness);
}