#version 450

#define MAX_MAPS_COUNT 12

const float PI = 3.14159265359F;
const float EPSILON = 0.000001F;

const int MAP_ALBEDO = 0;
const int MAP_NORMAL = 1;
const int MAP_METALLIC = 2;
const int MAP_ROUGHNESS = 3;
const int MAP_OCCLUSION = 4;
const int MAP_EMISSION = 5;
const int MAP_REFLECTION = 6;
const int MAP_SSR_REFLECTION = 7;

struct MaterialMap {
    vec4 color;
    float value;
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

struct LightCluster {
    uint lightOffset; // Index of the first entry of this cluster in the cluster light index buffer.
    uint lightCount; // Number of lights that affect this cluster.
    vec2 lightClusterPadding; // Padding to keep the struct at 16 bytes.
};

layout (std140, set = 3, binding = 0) uniform MaterialBuffer {
    int renderMode; // Render mode: (Solid = 0, Cutout = 1, Translucent = 2).
    MaterialMap maps[MAX_MAPS_COUNT]; // The material parameters.
};

layout (std430, set = 4, binding = 0) readonly buffer LightBuffer {
    int numOfLights; // Number of lights.
    int numOfDirectionalLights; // Number of directional lights.
    vec2 lightPadding; // Padding to keep the struct at 16 bytes.
    vec4 ambientColor; // rgb: (Color), w: (Intensity).
    Light[] lights; // The lights array.
};

layout (std140, set = 5, binding = 0) uniform ClusterDataBuffer {
    vec2 clusterScreenSize; // Size of the render target in pixels.
    uint maxLightsPerCluster; // The max number of lights per cluster.
    uint clusterCountX; // Number of clusters along the screen X axis.
    uint clusterCountY; // Number of clusters along the screen Y axis.
    uint clusterCountZ; // Number of clusters along the depth axis (depth slices).
    float clusterNearPlane; // Near plane of the camera, where the first depth slice starts.
    float clusterFarPlane; // Far plane of the camera, where the last depth slice ends.
    mat4 clusterProjection; // Camera projection matrix.
    mat4 clusterView; // Camera view matrix (world space -> view space).
    mat4 clusterInverseProjection; // Inverse of the projection matrix.
    mat4 clusterInverseView; // Inverse of the view matrix (view space -> world space).
};

layout(std430, set = 6, binding = 0) readonly buffer ClusterBuffer {
    LightCluster clusters[]; // The light range of every cluster.
};

layout(std430, set = 7, binding = 0) readonly buffer ClusterLightIndexBuffer {
    uint clusterLightIndices[]; // Light indices, grouped by cluster.
};

layout (std140, set = 8, binding = 0) uniform ShadowBuffer {
    float shadowMapResolution; // Shadow map resolution (in pixels).
    float shadowBias; // Constant depth bias applied when comparing against the shadow map (reduces shadow acne).
    float shadowNormalBias; // Offset along the surface normal when sampling the shadow map (reduces acne at grazing angles).
    float shadowBiasClamp; // Maximum allowed total depth bias (prevents peter-panning).
    vec4 shadowAmbientColor; // rgb: (Color), w: (Intensity).
    mat4x4 lightVP; // View-projection matrix of the shadow light (world space -> light space).
    vec4 shadowLightDirection; // xyz: (Direction), w: (Padding).
    vec4 shadowLightColor; // rgb: (Color), w: (Intensity).
};

layout (std140, set = 9, binding = 0) uniform EnvironmentBuffer {
    float maxReflectionLod; // Highest mip level of the prefilter map (roughness 1.0 reads this level).
    vec4 environmentColor; // rgb: (Tint), w: (Intensity).
};

layout (set = 10, binding = 0) uniform texture2D fAlbedo;
layout (set = 10, binding = 1) uniform sampler fAlbedoSampler;

layout (set = 11, binding = 0) uniform texture2D fNormal;
layout (set = 11, binding = 1) uniform sampler fNormalSampler;

layout (set = 12, binding = 0) uniform texture2D fMetallic;
layout (set = 12, binding = 1) uniform sampler fMetallicSampler;

layout (set = 13, binding = 0) uniform texture2D fEmission;
layout (set = 13, binding = 1) uniform sampler fEmissionSampler;

layout (set = 14, binding = 0) uniform texture2D fShadowMap;
layout (set = 14, binding = 1) uniform samplerShadow fShadowMapSampler;

layout (set = 15, binding = 0) uniform textureCube fIrradianceMap;
layout (set = 15, binding = 1) uniform sampler fIrradianceMapSampler;

layout (set = 16, binding = 0) uniform textureCube fPrefilterMap;
layout (set = 16, binding = 1) uniform sampler fPrefilterMapSampler;

layout (set = 17, binding = 0) uniform texture2D fBrdfLut;
layout (set = 17, binding = 1) uniform sampler fBrdfLutSampler;

layout (location = 0) in vec2 fTexCoords;
layout (location = 1) in vec4 fColor;
layout (location = 2) in vec3 fWorldPosition;
layout (location = 3) in vec3 fWorldNormal;
layout (location = 4) in mat3 fTbn;
layout (location = 7) in vec3 fViewPosition;

layout (location = 0) out vec4 fFragColor;
layout (location = 1) out vec4 fSsrMaskColor;

/*
 * Calculates the Fresnel reflectance using the Schlick approximation.
 *
 * Describes how much light is reflected instead of refracted, which grows
 * towards 1.0 as the viewing angle becomes more grazing.
 * The reflectance parameter is the base reflectance at normal incidence (F0).
 */
vec3 getFresnel(float hDotV, vec3 reflectance) {
    return reflectance + (1.0F - reflectance) * pow(1.0F - hDotV, 5.0F);
}

/*
 * Calculates the Fresnel reflectance for image-based lighting.
 *
 * Rough surfaces cannot reflect as strongly at grazing angles, so the upper
 * limit is lowered by the roughness.
 * Without this, rough materials would get unrealistically bright rims from the environment.
 */
vec3 getFresnelRoughness(float nDotV, vec3 reflectance, float roughness) {
    return reflectance + (max(vec3(1.0F - roughness), reflectance) - reflectance) * pow(1.0F - nDotV, 5.0F);
}

/*
 * Calculates the normal distribution function using GGX (Trowbridge-Reitz).
 *
 * Describes how many microfacets are aligned with the halfway vector. Low
 * roughness results in a small, bright highlight, and high roughness spreads the highlight over a wide area.
 */
float getDistribution(float nDotH, float roughness) {
    float alpha = roughness * roughness;
    float alphaSquared = alpha * alpha;
    float divisor = nDotH * nDotH * (alphaSquared - 1.0F) + 1.0F;
    
    return alphaSquared / max(PI * divisor * divisor, EPSILON);
}

/*
 * Calculates the Smith geometry term using Schlick-GGX.
 *
 * Describes how much light is lost to microfacet self-shadowing (light
 * direction) and masking (view direction). This is the direct lighting
 * variant of k, as opposed to the IBL variant used in the BRDF LUT shader.
 */
float getGeometry(float nDotV, float nDotL, float roughness) {
    float radius = roughness + 1.0F;
    float k = radius * radius / 8.0F;
    
    float viewGeometry = nDotV / max(nDotV * (1.0F - k) + k, EPSILON);
    float lightGeometry = nDotL / max(nDotL * (1.0F - k) + k, EPSILON);
    
    return viewGeometry * lightGeometry;
}

/*
 * Calculates the world-space surface normal, including the normal map.
 *
 * The normal map is stored in tangent space, so it is decoded and rotated
 * into world space with the TBN matrix. The normal strength is controlled by the value of the normal material map.
 */
vec3 getNormal() {
    vec3 normal = normalize(fWorldNormal);
    vec3 sampledNormal = texture(sampler2D(fNormal, fNormalSampler), fTexCoords).rgb;
    vec3 decoded = sampledNormal * 2.0F - 1.0F;
    
    // Decode the normal from the [0, 1] range into a unit vector in the [-1, 1] range.
    sampledNormal = dot(decoded, decoded) > EPSILON ? normalize(decoded) : vec3(0.0F, 0.0F, 1.0F);
    
    // Scaling only X and Y strengthens or weakens the bumps without flipping the surface.
    sampledNormal.xy *= maps[MAP_NORMAL].value;
    
    return normalize(fTbn * sampledNormal);
}

/*
 * Calculates how much a point or spot light weakens with distance.
 *
 * The light fades linearly to zero at its range and additionally follows the
 * inverse-square law. The distance is clamped to 1.0 so the intensity cannot
 * explode when a surface is very close to the light. A range of zero or less disables the attenuation.
 */
float getRangeAttenuation(float distance, float range) {
    if (range <= 0.0F) {
        return 1.0F;
    }
    
    float attenuation = clamp(1.0F - distance / range, 0.0F, 1.0F);
    return attenuation * attenuation / max(distance * distance, 1.0F);
}

/*
 * Calculates the cone falloff of a spot light.
 *
 * Returns 1.0 inside the inner cone, 0.0 outside the outer cone, and a
 * linear blend in between. The cone angles are expected in radians.
 */
float getSpotAttenuation(Light light, vec3 lightDirection) {
    vec3 direction = normalize(-light.direction.xyz);
    float angle = dot(lightDirection, direction);
    
    float innerCone = cos(light.innerSpotAngle);
    float outerCone = cos(light.outerSpotAngle);
    
    return clamp((angle - outerCone) / max(innerCone - outerCone, EPSILON), 0.0F, 1.0F);
}

/*
 * Calculates the radiance a light delivers to the current fragment.
 *
 * The direction from the fragment towards the light is returned through
 * lightDirection. Directional lights have no position or attenuation, while
 * point and spot lights are weakened by distance, and spot lights also by their cone.
 */
vec3 getLightRadiance(Light light, out vec3 lightDirection) {
    
    // Directional light: constant direction and no attenuation.
    if (light.type == 0) {
        lightDirection = normalize(-light.direction.xyz);
        return light.color.rgb * light.color.w;
    }
    
    vec3 difference = light.position.xyz - fWorldPosition;
    float distanceSquared = dot(difference, difference);
    float range = light.range;
    
    if (range > 0.0F && distanceSquared > range * range) {
        lightDirection = vec3(0.0F, 1.0F, 0.0F);
        return vec3(0.0F);
    }
    
    float distance = sqrt(max(distanceSquared, EPSILON));
    
    // Fall back to straight up if the fragment sits exactly on the light.
    lightDirection = distance > EPSILON ? difference / distance : vec3(0.0F, 1.0F, 0.0F);
    
    float attenuation = getRangeAttenuation(distance, range);
    
    // Spot light: additionally restrict the light to its cone.
    if (light.type == 2) {
        float spotAttenuation = getSpotAttenuation(light, lightDirection);
        
        if (spotAttenuation <= 0.0F) {
            return vec3(0.0F);
        }
        
        attenuation *= spotAttenuation;
    }
    
    return light.color.rgb * light.color.w * attenuation;
}

/*
 * Calculates the direct lighting of a single light using a Cook-Torrance BRDF.
 *
 * The result is the sum of a Lambertian diffuse term and a microfacet
 * specular term, multiplied by the light radiance and NdotL.
 * The reflectionStrength scales the specular part of the material.
 */
vec3 getLightColor(Light light, vec3 albedo, vec3 normal, vec3 viewDirection, float metallic, float roughness, float reflectionStrength) {
    vec3 lightDirection;
    vec3 radiance = getLightRadiance(light, lightDirection);
    
    if (dot(radiance, radiance) <= EPSILON) {
        return vec3(0.0F);
    }
    
    float nDotL = max(dot(normal, lightDirection), 0.0F);
    
    if (nDotL <= 0.0F) {
        return vec3(0.0F);
    }
    
    vec3 halfSum = viewDirection + lightDirection;
    vec3 halfDirection = halfSum / max(length(halfSum), EPSILON);
    
    // NdotV is kept above zero because it is used as a divisor below.
    float nDotV = clamp(dot(normal, viewDirection), EPSILON, 1.0F);
    float nDotH = clamp(dot(normal, halfDirection), 0.0F, 1.0F);
    float hDotV = clamp(dot(halfDirection, viewDirection), 0.0F, 1.0F);
    
    // Dielectrics reflect about 4% of the light, while metals reflect their albedo color.
    vec3 baseReflectance = mix(vec3(0.04F), albedo, metallic);
    
    float distribution = getDistribution(nDotH, roughness);
    float geometry = getGeometry(nDotV, nDotL, roughness);
    vec3 fresnel = getFresnel(hDotV, baseReflectance);
    
    // Cook-Torrance specular term.
    vec3 specular = distribution * geometry * fresnel / max(4.0F * nDotV * nDotL, EPSILON);
    specular *= reflectionStrength;
    
    // Energy conservation: light that is reflected cannot also be diffused, and metals have no diffuse light.
    vec3 diffuse = vec3(1.0F) - fresnel;
    diffuse *= 1.0F - metallic;
    
    return (diffuse * albedo / PI + specular) * radiance * nDotL;
}

/*
 * Calculates the ambient lighting from the environment maps.
 *
 * The diffuse part comes from the irradiance map. The specular part uses the
 * split-sum approximation, where the prefiltered environment color is
 * combined with the scale and bias from the BRDF LUT.
 * The result is multiplied by the ambient occlusion and the environment tint.
 */
vec3 getImageBasedLightColor(vec3 albedo, vec3 normal, vec3 viewDirection, float metallic, float roughness, float ambientOcclusion, float reflectionStrength) {
    float nDotV = clamp(dot(normal, viewDirection), EPSILON, 1.0F);
    
    vec3 baseReflectance = mix(vec3(0.04F), albedo, metallic);
    vec3 fresnel = getFresnelRoughness(nDotV, baseReflectance, roughness);
    
    // Energy conservation: the diffuse part only gets the light that is not reflected.
    vec3 diffuse = vec3(1.0F) - fresnel;
    diffuse *= 1.0F - metallic;
    
    // Diffuse: the irradiance map already holds the cosine-weighted light around the normal.
    vec3 irradiance = texture(samplerCube(fIrradianceMap, fIrradianceMapSampler), normal).rgb;
    vec3 diffuseColor = irradiance * albedo;
    
    // Specular: rougher surfaces read from higher, blurrier mip levels of the prefilter map.
    vec3 reflection = reflect(-viewDirection, normal);
    vec3 prefilteredColor = textureLod(samplerCube(fPrefilterMap, fPrefilterMapSampler), reflection, roughness * maxReflectionLod).rgb;
    
    // The LUT replaces the BRDF integral with a scale and bias applied to the Fresnel term.
    vec2 brdf = texture(sampler2D(fBrdfLut, fBrdfLutSampler), vec2(nDotV, roughness)).rg;
    vec3 specularColor = prefilteredColor * (fresnel * brdf.x + brdf.y) * reflectionStrength;
    
    return (diffuse * diffuseColor + specularColor) * ambientOcclusion * environmentColor.rgb * environmentColor.w;
}

/*
 * Calculates how much of the shadow light reaches the current fragment.
 *
 * Returns 1.0 for fully lit and 0.0 for fully shadowed. The fragment is
 * projected into the light's view, compared against the shadow map with a
 * slope-dependent bias, and filtered with a 3x3 PCF kernel for soft edges.
 */
float getShadowFactor(vec3 normal) {
    
    // Without an active shadow light there is nothing to shadow.
    if (shadowLightColor.w <= 0.0F || shadowMapResolution <= 0.0F) {
        return 1.0F;
    }
    
    // Transform the world position into light space and apply the perspective divide.
    vec4 lightSpacePosition = lightVP * vec4(fWorldPosition, 1.0F);
    vec3 projectionCoords = lightSpacePosition.xyz / lightSpacePosition.w;
    
    // Convert from clip space to UV space and flip Y to a top-left origin.
    projectionCoords.xy = projectionCoords.xy * 0.5F + 0.5F;
    projectionCoords.y = 1.0F - projectionCoords.y;
    
    // Fragments outside the shadow map are treated as lit.
    if (projectionCoords.x < 0.0F || projectionCoords.x > 1.0F ||
        projectionCoords.y < 0.0F || projectionCoords.y > 1.0F ||
        projectionCoords.z < 0.0F || projectionCoords.z > 1.0F) {
        return 1.0F;
    }
    
    float currentDepth = projectionCoords.z;
    
    vec3 lightDirection = normalize(-shadowLightDirection.xyz);
    float normalDotLight = max(dot(normal, lightDirection), 0.0F);
    
    // Surfaces at grazing angles to the light need a larger bias to avoid shadow acne.
    float normalBias = shadowNormalBias * (1.0F - normalDotLight);

    // The total bias is clamped so shadows do not detach from their casters.
    float bias = clamp(shadowBias + normalBias, 0.0F, shadowBiasClamp);
    
    vec2 texelSize = vec2(1.0F / shadowMapResolution);
    
    float shadow = 0.0F;
    
    // Percentage-closer filtering: average the comparison results of the 3x3 neighborhood.
    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            vec2 offset = vec2(x, y) * texelSize;
            shadow += texture(sampler2DShadow(fShadowMap, fShadowMapSampler), vec3(projectionCoords.xy + offset, currentDepth - bias));
        }
    }
    
    return shadow / 9.0F;
}

/*
 * Calculates the index of the light cluster that contains the current fragment.
 *
 * The view frustum is split into a 3D grid. X and Y follow the screen position,
 * and Z follows the view depth on a logarithmic scale, so slices close to the camera
 * are thin and slices far away are thick. The result is an index into the cluster
 * buffer, ordered by x, then y, then z. It has to match the order that the cluster
 * compute shaders used when they built the grid.
 */
uint getClusterIndex() {
    vec2 tileCounts = vec2(clusterCountX, clusterCountY);
    vec2 tile = min(gl_FragCoord.xy * tileCounts / max(clusterScreenSize, vec2(1.0F)), tileCounts - 1.0F);
    
    // Only the Z row of the view matrix is needed for the depth (camera looks down -Z).
    float viewZ = clusterView[0][2] * fWorldPosition.x
        + clusterView[1][2] * fWorldPosition.y
        + clusterView[2][2] * fWorldPosition.z
        + clusterView[3][2];
    
    float viewDepth = max(-viewZ, clusterNearPlane);
    
    float nearPlane = max(clusterNearPlane, EPSILON);
    float farPlane = max(clusterFarPlane, nearPlane + EPSILON);
    float depthRatio = max(farPlane / nearPlane, 1.0F + EPSILON);
    
    float slice = log(viewDepth / nearPlane) / log(depthRatio) * float(clusterCountZ);
    uint clusterZ = uint(clamp(slice, 0.0F, float(clusterCountZ - 1u)));
    
    return uint(tile.x) + uint(tile.y) * clusterCountX + clusterZ * clusterCountX * clusterCountY;
}

/*
 * Calculates the direct lighting of all lights that affect the current fragment.
 *
 * Directional lights are stored at the start of the light array and are always
 * evaluated. Local lights are only evaluated if the cluster of the fragment lists
 * them, so the cost depends on the lights near the fragment and not on the total
 * number of lights in the scene.
 */
vec3 getDirectLightColor(vec3 albedo, vec3 normal, vec3 viewDirection, float metallic, float roughness, float reflectionStrength, float shadowFactor) {
    vec3 lightColor = vec3(0.0F);
    
    // Directional lights.
    int directionalCount = clamp(numOfDirectionalLights, 0, numOfLights);
    
    for (int index = 0; index < directionalCount; index++) {
        vec3 currentLightColor = getLightColor(lights[index], albedo, normal, viewDirection, metallic, roughness, reflectionStrength);
        
        if (lights[index].type == 0) {
            currentLightColor *= mix(0.35F, 1.0F, shadowFactor);
        }
        
        lightColor += currentLightColor;
    }
    
    // Local lights.
    uint clusterIndex = getClusterIndex();
    LightCluster cluster = clusters[clusterIndex];
    uint lightCount = min(cluster.lightCount, maxLightsPerCluster);
    
    for (uint i = 0u; i < lightCount; i++) {
        uint lightIndex = clusterLightIndices[cluster.lightOffset + i];
        lightColor += getLightColor(lights[lightIndex], albedo, normal, viewDirection, metallic, roughness, reflectionStrength);
    }
    
    return lightColor;
}

/*
 * Calculates the final lit color and opacity of the fragment.
 *
 * Reads the material textures, accumulates the direct and ambient lighting,
 * adds the emission, and finally applies tone mapping and gamma correction.
 */
vec4 getPbrColor() {
    vec4 albedoTexel = texture(sampler2D(fAlbedo, fAlbedoSampler), fTexCoords);
    vec4 metallicTexel = texture(sampler2D(fMetallic, fMetallicSampler), fTexCoords);
    vec4 emissionTexel = texture(sampler2D(fEmission, fEmissionSampler), fTexCoords);
    
    // Combine the texture with the vertex color and the material tint.
    vec3 albedo = albedoTexel.rgb * fColor.rgb * maps[MAP_ALBEDO].color.rgb;
    float alpha = albedoTexel.a * fColor.a * maps[MAP_ALBEDO].color.a;
    
    // Unpack metallic, roughness and occlusion from the material texture.
    float metallic = clamp(metallicTexel.r * maps[MAP_METALLIC].value, 0.0F, 1.0F);
    
    // Roughness has a lower limit, because perfectly smooth surfaces break the GGX distribution.
    float roughness = clamp(metallicTexel.g * maps[MAP_ROUGHNESS].value, 0.04F, 1.0F);
    float ambientOcclusion = clamp(metallicTexel.b * maps[MAP_OCCLUSION].value, 0.0F, 1.0F);
    
    float reflectionStrength = clamp(maps[MAP_REFLECTION].value, 0.0F, 1.0F);
    
    vec3 emission = emissionTexel.rgb * maps[MAP_EMISSION].color.rgb * maps[MAP_EMISSION].value;
    
    vec3 normal = getNormal();
    vec3 viewDirection = normalize(fViewPosition - fWorldPosition);
    
    float shadowFactor = getShadowFactor(normal);
    vec3 lightColor = getDirectLightColor(albedo, normal, viewDirection, metallic, roughness, reflectionStrength, shadowFactor);
    
    // Ambient lighting from the environment maps plus the flat ambient color.
    vec3 ambient = getImageBasedLightColor(albedo, normal, viewDirection, metallic, roughness, ambientOcclusion, reflectionStrength);
    ambient += ambientColor.rgb * ambientColor.w * albedo * (1.0F - metallic) * ambientOcclusion;
    
    vec3 color = ambient + lightColor * ambientOcclusion + emission;
    
    // Reinhard tone mapping compresses the HDR color into [0, 1).
    color = color / (color + vec3(1.0F));
    
    // Gamma correction converts the linear color to display space.
    color = pow(color, vec3(1.0F / 2.2F));
    
    return vec4(color, alpha);
}

void main() {
    vec4 texelColor = getPbrColor();
    
    // The SSR pass reads this mask to decide how reflective this pixel is.
    float ssrReflectionMask = clamp(maps[MAP_SSR_REFLECTION].value, 0.0F, 1.0F);
    
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
    fSsrMaskColor = vec4(ssrReflectionMask, 0.0F, 0.0F, 1.0F);
}