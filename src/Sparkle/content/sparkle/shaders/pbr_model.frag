#version 450

#define MAX_MAPS_COUNT 8
#define MAX_LIGHT_COUNT 512

const float PI = 3.14159265359F;
const float EPSILON = 0.000001F;

struct MaterialMap {
    vec4 color;
    float value;
};

layout(std140, set = 2, binding = 0) uniform MaterialBuffer {
    int renderMode;
    MaterialMap maps[MAX_MAPS_COUNT];
};

struct Light {
    int type; // Type: (Direction = 0, Point = 1, Spot = 2).
    float range; // Range.
    float innerSpotAngle; // Inner SpotAngle.
    float outerSpotAngle; // Outer SpotAngle.
    vec4 position; // xyz: (Position), w: (Padding).
    vec4 direction; // xyz: (Direction), w: (Padding).
    vec4 color; // rgb: (Color), w: (Intensity).
};

layout(std140, set = 3, binding = 0) uniform LightBuffer {
    int numOfLights; // Number of lights.
    vec4 ambientColor; // rgb: (Color), w: (Intensity).
    Light[MAX_LIGHT_COUNT] lights; // The lights array.
};

layout(std140, set = 4, binding = 0) uniform ShadowBuffer {
    float shadowMapResolution; // Shadow map resolution (in pixels).
    float shadowBias; // Constant depth bias applied when comparing against the shadow map (reduces shadow acne).
    float shadowNormalBias; // Offset along the surface normal when sampling the shadow map (reduces acne at grazing angles).
    float shadowBiasClamp; // Maximum allowed total depth bias (prevents peter-panning).
    vec4 shadowAmbientColor; // rgb: (Color), w: (Intensity).
    mat4x4 lightVP; // View-projection matrix of the shadow light (world space -> light space).
    vec4 shadowLightDirection; // xyz: (Direction), w: (Padding).
    vec4 shadowLightColor; // rgb: (Color), w: (Intensity).
};

layout (set = 5, binding = 0) uniform texture2D fAlbedo;
layout (set = 5, binding = 1) uniform sampler fAlbedoSampler;

layout (set = 6, binding = 0) uniform texture2D fNormal;
layout (set = 6, binding = 1) uniform sampler fNormalSampler;

layout (set = 7, binding = 0) uniform texture2D fMetallic;
layout (set = 7, binding = 1) uniform sampler fMetallicSampler;

layout (set = 8, binding = 0) uniform texture2D fEmission;
layout (set = 8, binding = 1) uniform sampler fEmissionSampler;

layout (set = 9, binding = 0) uniform texture2D fShadowMap;
layout (set = 9, binding = 1) uniform samplerShadow fShadowMapSampler;

layout (location = 0) in vec2 fTexCoords;
layout (location = 1) in vec4 fColor;
layout (location = 2) in vec3 fWorldPosition;
layout (location = 3) in vec3 fWorldNormal;
layout (location = 4) in mat3 fTbn;
layout (location = 7) in vec3 fViewPosition;

layout (location = 0) out vec4 fFragColor;

/*
 * Calculates the Fresnel term using Schlick's approximation.
 *
 * The result controls how much light is reflected at grazing angles.
 */
vec3 getFresnel(float hDotV, vec3 reflectance) {
    return reflectance + (1.0F - reflectance) * pow(1.0F - hDotV, 5.0F);
}

/*
 * Calculates the GGX normal distribution term.
 *
 * This controls how strongly the surface microfacets align with the halfway vector.
 */
float getDistribution(float nDotH, float roughness) {
    float alpha = roughness * roughness;
    float alphaSquared = alpha * alpha;
    float divisor = nDotH * nDotH * (alphaSquared - 1.0F) + 1.0F;

    return alphaSquared / max(PI * divisor * divisor, EPSILON);
}

/*
 * Calculates the Smith geometry term.
 *
 * This approximates how much microfacet shadowing happens between the view and light directions.
 */
float getGeometry(float nDotV, float nDotL, float roughness) {
    float radius = roughness + 1.0F;
    float k = radius * radius / 8.0F;

    float viewGeometry = nDotV / max(nDotV * (1.0F - k) + k, EPSILON);
    float lightGeometry = nDotL / max(nDotL * (1.0F - k) + k, EPSILON);

    return viewGeometry * lightGeometry;
}

/*
 * Reads the material normal.
 *
 * The normal map is transformed from tangent space into world space.
 */
vec3 getNormal() {
    vec3 normal = normalize(fWorldNormal);
    vec3 sampledNormal = texture(sampler2D(fNormal, fNormalSampler), fTexCoords).rgb;

    sampledNormal = normalize(sampledNormal * 2.0F - 1.0F);
    sampledNormal.xy *= maps[1].value;

    return normalize(fTbn * sampledNormal);
}

/*
 * Calculates range attenuation for local lights.
 *
 * The light fades out smoothly near its configured range.
 */
float getRangeAttenuation(float distance, float range) {
    if (range <= 0.0F) {
        return 1.0F;
    }

    float attenuation = clamp(1.0F - distance / range, 0.0F, 1.0F);
    return attenuation * attenuation / max(distance * distance, 1.0F);
}

/*
 * Calculates spot light cone attenuation.
 *
 * Fragments inside the inner cone receive full light and fragments outside
 * the outer cone receive no light.
 */
float getSpotAttenuation(Light light, vec3 lightDirection) {
    vec3 direction = normalize(-light.direction.xyz);
    float angle = dot(lightDirection, direction);

    float innerCone = cos(light.innerSpotAngle);
    float outerCone = cos(light.outerSpotAngle);

    return clamp((angle - outerCone) / max(innerCone - outerCone, EPSILON), 0.0F, 1.0F);
}

/*
 * Calculates the radiance and direction for a light.
 */
vec3 getLightRadiance(Light light, out vec3 lightDirection) {

    // Directional.
    if (light.type == 0) {
        lightDirection = normalize(-light.direction.xyz);
        return light.color.rgb * light.color.w;
    }

    vec3 difference = light.position.xyz - fWorldPosition;
    float distance = length(difference);

    lightDirection = distance > EPSILON ? difference / distance : vec3(0.0F, 1.0F, 0.0F);

    float attenuation = getRangeAttenuation(distance, light.range);

    // Spot.
    if (light.type == 2) {
        attenuation *= getSpotAttenuation(light, lightDirection);
    }

    return light.color.rgb * light.color.w * attenuation;
}

/*
 * Calculates the PBR lighting contribution for one light.
 */
vec3 getLightColor(Light light, vec3 albedo, vec3 normal, vec3 viewDirection, float metallic, float roughness) {
    vec3 lightDirection;
    vec3 radiance = getLightRadiance(light, lightDirection);

    vec3 halfDirection = normalize(viewDirection + lightDirection);

    float nDotV = max(dot(normal, viewDirection), EPSILON);
    float nDotL = max(dot(normal, lightDirection), 0.0F);
    float nDotH = max(dot(normal, halfDirection), 0.0F);
    float hDotV = max(dot(halfDirection, viewDirection), 0.0F);

    vec3 baseReflectance = mix(vec3(0.04F), albedo, metallic);

    float distribution = getDistribution(nDotH, roughness);
    float geometry = getGeometry(nDotV, nDotL, roughness);
    vec3 fresnel = getFresnel(hDotV, baseReflectance);

    vec3 specular = distribution * geometry * fresnel / max(4.0F * nDotV * nDotL, EPSILON);

    vec3 diffuse = vec3(1.0F) - fresnel;
    diffuse *= 1.0F - metallic;

    return (diffuse * albedo / PI + specular) * radiance * nDotL;
}

/*
 * Samples the shadow map using the light-space position passed from the vertex shader.
 *
 * Returns:
 * 1.0 = fully lit
 * 0.0 = fully shadowed
 */
float getShadowFactor(vec3 normal) {
    if (shadowLightColor.w <= 0.0F || shadowMapResolution <= 0.0F) {
        return 1.0F;
    }

    vec4 lightSpacePosition = lightVP * vec4(fWorldPosition, 1.0F);
    vec3 projectionCoords = lightSpacePosition.xyz / lightSpacePosition.w;

    projectionCoords.xy = projectionCoords.xy * 0.5F + 0.5F;
    projectionCoords.y = 1.0F - projectionCoords.y;

    if (projectionCoords.x < 0.0F || projectionCoords.x > 1.0F ||
    projectionCoords.y < 0.0F || projectionCoords.y > 1.0F ||
    projectionCoords.z < 0.0F || projectionCoords.z > 1.0F) {
        return 1.0F;
    }

    float currentDepth = projectionCoords.z;

    vec3 lightDirection = normalize(-shadowLightDirection.xyz);
    float normalDotLight = max(dot(normal, lightDirection), 0.0F);
    
    float normalBias = shadowNormalBias * (1.0F - normalDotLight);
    float bias = clamp(shadowBias + normalBias, 0.0F, shadowBiasClamp);
    
    vec2 texelSize = vec2(1.0F / shadowMapResolution);
    
    float shadow = 0.0F;
    
    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            vec2 offset = vec2(x, y) * texelSize;
            shadow += texture(sampler2DShadow(fShadowMap, fShadowMapSampler), vec3(projectionCoords.xy + offset, currentDepth - bias));
        }
    }
    
    return shadow / 9.0F;
}

/*
 * Calculates the final PBR color.
 *
 * Material map usage:
 * maps[0] = Albedo color/value.
 * maps[1] = Normal strength.
 * maps[2] = Metallic value.
 * maps[3] = Roughness value.
 * maps[4] = Ambient occlusion value.
 * maps[5] = Emission color/power.
 */
vec4 getPbrColor() {
    vec4 albedoTexel = texture(sampler2D(fAlbedo, fAlbedoSampler), fTexCoords);
    vec4 metallicTexel = texture(sampler2D(fMetallic, fMetallicSampler), fTexCoords);
    vec4 emissionTexel = texture(sampler2D(fEmission, fEmissionSampler), fTexCoords);

    vec3 albedo = albedoTexel.rgb * fColor.rgb * maps[0].color.rgb;
    float alpha = albedoTexel.a * fColor.a * maps[0].color.a;

    float metallic = clamp(metallicTexel.r * maps[2].value, 0.0F, 1.0F);
    float roughness = clamp(metallicTexel.g * maps[3].value, 0.04F, 1.0F);
    float ambientOcclusion = clamp(metallicTexel.b * maps[4].value, 0.0F, 1.0F);

    vec3 emission = emissionTexel.rgb * maps[5].color.rgb * maps[5].value;

    vec3 normal = getNormal();
    vec3 viewDirection = normalize(fViewPosition - fWorldPosition);

    float shadowFactor = getShadowFactor(normal);
    vec3 lightColor = vec3(0.0F);

    for (int index = 0; index < numOfLights; index++) {
        vec3 currentLightColor = getLightColor(lights[index], albedo, normal, viewDirection, metallic, roughness);

        // Apply the shadow map only to directional lights.
        if (lights[index].type == 0) {
            currentLightColor *= mix(0.35F, 1.0F, shadowFactor);
        }

        lightColor += currentLightColor;
    }

    vec3 ambient = ambientColor.rgb * ambientColor.w * albedo * ambientOcclusion;
    vec3 color = ambient + lightColor * ambientOcclusion + emission;

    // HDR tonemapping.
    color = color / (color + vec3(1.0F));

    // Gamma correction.
    color = pow(color, vec3(1.0F / 2.2F));

    return vec4(color, alpha);
}

void main() {
    vec4 texelColor = getPbrColor();

    // Set render mode.
    switch (renderMode) {

    // Solid.
        case 0:
            texelColor.a = 1.0F;
            break;

    // Cutout.
        case 1:
            if (texelColor.a < 0.99F) {
                discard;
            }
            break;
    }

    fFragColor = texelColor;
}