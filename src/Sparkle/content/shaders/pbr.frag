#version 330

#define MAX_LIGHTS 815
#define PI 3.14159265358979323846

// Light Types
#define LIGHT_DIRECTIONAL 0
#define LIGHT_POINT 1
#define LIGHT_SPOT 2

struct Light {
    int enabled;
    int type;
    vec3 position;
    vec3 target;
    vec4 color;
    float intensity;
};

layout(std140) uniform LightBuffer {
    Light lights[MAX_LIGHTS];
};

uniform int numOfLights;

// Input vertex attributes (from vertex shader)
in vec3 fragPosition;
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;
in vec4 shadowPos;
in mat3 TBN;

// Output fragment color
out vec4 finalColor;

uniform sampler2D albedoMap;
uniform sampler2D mraMap;
uniform sampler2D normalMap;
uniform sampler2D emissiveMap; // r: Hight g:emissive

uniform vec2 tiling;
uniform vec2 offset;

uniform int useTexAlbedo;
uniform int useTexNormal;
uniform int useTexMRA;
uniform int useTexEmissive;

uniform vec4 albedoColor;
uniform vec4 emissiveColor;
uniform float normalValue;
uniform float metallicValue;
uniform float roughnessValue;
uniform float aoValue;
uniform float emissivePower;

uniform vec3 viewPos;

uniform vec3 ambientColor;
uniform float ambient;

// refl in range 0 to 1
// returns base reflectivity to 1
// incrase reflectivity when surface view at larger angle
vec3 SchlickFresnel(float hDotV, vec3 refl) {
    return refl + (1.0 - refl) * pow(1.0 - hDotV, 5.0);
}

float GgxDistribution(float nDotH, float roughness) {
    float a = roughness * roughness * roughness * roughness;
    float d = nDotH * nDotH * (a - 1.0) + 1.0;
    d = PI * d * d;
    return a / max(d, 0.0000001);
}

float GeomSmith(float nDotV, float nDotL, float roughness) {
    float r = roughness + 1.0;
    float k = r * r / 8.0;
    float ik = 1.0 - k;
    float ggx1 = nDotV / (nDotV * ik + k);
    float ggx2 = nDotL / (nDotL * ik + k);
    return ggx1 * ggx2;
}

vec4 ComputePBR() {
    vec4 albedo_tex = texture(albedoMap, vec2(fragTexCoord.x * tiling.x + offset.x, fragTexCoord.y * tiling.y + offset.y));
    vec3 albedo = vec3(albedoColor.x * albedo_tex.x, albedoColor.y * albedo_tex.y, albedoColor.z * albedo_tex.z);

    float metallic = clamp(metallicValue, 0.0, 1.0);
    float roughness = clamp(roughnessValue, 0.0, 1.0);
    float ao = clamp(aoValue, 0.0, 1.0);

    if (useTexMRA == 1) {
        vec4 mra = texture(mraMap, vec2(fragTexCoord.x * tiling.x + offset.x, fragTexCoord.y * tiling.y + offset.y)) * useTexMRA;
        metallic = clamp(mra.r + metallicValue, 0.04, 1.0);
        roughness = clamp(mra.g + roughnessValue, 0.04, 1.0);
        ao = (mra.b + aoValue) * 0.5;
    }

    vec3 N = normalize(fragNormal);

    if (useTexNormal == 1) {
        N = texture(normalMap, vec2(fragTexCoord.x * tiling.x + offset.y, fragTexCoord.y * tiling.y + offset.y)).rgb;
        N = normalize(N * 2.0 - 1.0);
        N = normalize(N * TBN);
    }

    vec3 V = normalize(viewPos - fragPosition);
    vec3 emissive = (texture(emissiveMap, vec2(fragTexCoord.x * tiling.x + offset.x, fragTexCoord.y * tiling.y + offset.y)).rgb).g * emissiveColor.rgb * emissivePower * useTexEmissive;

    // return N; //vec3(metallic, metallic, metallic);
    // if dia-electric use base reflectivity of 0.04 otherwise ut is a metal use albedo as base reflectivity
    vec3 baseRefl = mix(vec3(0.04), albedo.rgb, metallic);
    vec3 lightAccum = vec3(0.0); // acumulate lighting lum

    for (int i = 0; i < numOfLights; ++i) {
        Light light = lights[i];
        
        vec3 L;
        vec3 H;
        float dist;
        float attenuation;
        vec3 radiance;
        
        switch (light.type) {
            case LIGHT_DIRECTIONAL:
                L = -normalize(light.target - light.position);
                H = normalize(V + L);
                radiance = light.color.rgb * light.intensity; // calc input radiance,light energy comming in
                break;
            
            case LIGHT_SPOT:
                L = normalize(light.position - fragPosition);
                H = normalize(V + L);
                dist = length(light.position - fragPosition);
                attenuation = 1.0 / (dist * dist * 0.23);

                // Check if the fragment is within the spot cone
                float spotCosine = dot(normalize(light.target - light.position), -L);
                float spotFactor = smoothstep(light.target.y, light.target.y + light.color.a, spotCosine);
                radiance = light.color.rgb * light.intensity * attenuation * spotFactor; // calc input radiance,light energy comming in
                break;
            
            case LIGHT_POINT:
                L = normalize(light.position - fragPosition); // calc light vector
                H = normalize(V + L); // calc halfway bisecting vector
                dist = length(light.position - fragPosition); // calc distance to light
                attenuation = 1.0 / (dist * dist * 0.23); // calc attenuation
                radiance = light.color.rgb * light.intensity * attenuation; // calc input radiance,light energy comming in
                break;
        }
        
        // Cook-Torrance BRDF distribution function
        float nDotV = max(dot(N, V), 0.0000001);
        float nDotL = max(dot(N, L), 0.0000001);
        float hDotV = max(dot(H, V), 0.0);
        float nDotH = max(dot(N, H), 0.0);
        float D = GgxDistribution(nDotH, roughness); // larger the more micro-facets aligned to H
        float G = GeomSmith(nDotV, nDotL, roughness); // smaller the more micro-facets shadow
        vec3 F = SchlickFresnel(hDotV, baseRefl); // fresnel proportion of specular reflectance

        vec3 spec = (D * G * F) / (4.0 * nDotV * nDotL);
        // difuse and spec light can't be above 1.0
        // kD = 1.0 - kS  diffuse component is equal 1.0 - spec comonent
        vec3 kD = vec3(1.0) - F;
        // mult kD by the inverse of metallnes , only non-metals should have diffuse light
        kD *= 1.0 - metallic;
        lightAccum += ((kD * albedo.rgb / PI + spec) * radiance * nDotL) * light.enabled; // angle of light has impact on result
    }
    
    vec3 ambient_final = (ambientColor + albedo) * ambient * 0.5;
    return vec4(ambient_final + lightAccum * ao + emissive, albedo_tex.w);
}

void main() {
    vec4 color = ComputePBR();

    if (color.a <= 0.0) {
        discard;
    }

    // HDR tonemapping
    color = vec4(pow(color.rgb, color.rgb + vec3(1.0)), color.w);

    // Gamma correction
    color = vec4(pow(color.rgb, vec3(1.0 / 2.2)), color.w);
    
    // Fog calculation
    float dist = length(viewPos - fragPosition);
    
    // Fog parameters
    const vec4 fogColor = vec4(0.5, 0.5, 0.5, 1);
    const float fogDensity = 0.06;
    
    float fogFactor = 1.0 / exp((dist * fogDensity) * (dist * fogDensity));
    fogFactor = clamp(fogFactor, 0.0, 1.0);

    finalColor = mix(fogColor, color, fogFactor);
}