#version 450

const float PI = 3.14159265359F;

layout (set = 1, binding = 0) uniform textureCube fEnvironmentMap;
layout (set = 1, binding = 1) uniform sampler fEnvironmentSampler;

layout (location = 0) in vec3 fLocalPosition;

layout (location = 0) out vec4 fFragColor;

void main() {
    vec3 normal = normalize(fLocalPosition);
    vec3 irradiance = vec3(0.0F);
    
    // Build a tangent frame around the normal so samples can be moved from tangent space into world space.
    vec3 up = vec3(0.0F, 1.0F, 0.0F);
    vec3 right = normalize(cross(up, normal));
    up = normalize(cross(normal, right));
    
    // A smaller delta gives a smoother result at the cost of more samples.
    float sampleDelta = 0.025F;
    float sampleCount = 0.0F;
    
    // Walk around the full circle (phi) and from the pole down to the horizon (theta).
    for (float phi = 0.0F; phi < 2.0F * PI; phi += sampleDelta) {
        for (float theta = 0.0F; theta < 0.5F * PI; theta += sampleDelta) {

            // Convert the spherical coordinates into a tangent-space direction, where the normal points along +Z.
            vec3 tangentSample = vec3(
                sin(theta) * cos(phi),
                sin(theta) * sin(phi),
                cos(theta)
            );
            
            // Rotate the tangent-space direction into the space of the cubemap.
            vec3 sampleVector = tangentSample.x * right + tangentSample.y * up + tangentSample.z * normal;
            
            // Weight the light by cos(theta) for Lambert's law, and by sin(theta) because samples near the pole cover a smaller solid angle than samples near the horizon.
            irradiance += texture(samplerCube(fEnvironmentMap, fEnvironmentSampler), sampleVector).rgb * cos(theta) * sin(theta);
            sampleCount++;
        }
    }
    
    // Average the samples and scale by PI to account for the integration over the hemisphere.
    irradiance = PI * irradiance / sampleCount;
    
    fFragColor = vec4(irradiance, 1.0F);
}