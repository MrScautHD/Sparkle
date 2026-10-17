#version 450

const float EPSILON = 0.000001F;

layout (std140, set = 0, binding = 0) uniform FogBuffer {
    vec4 fogColor; // rgb: (Color), w: (Intensity).
    vec4 fogParameters; // x: (Density), y: (Height falloff), z: (Base height), w: (Start distance).
    vec4 fogDistanceParameters; // x: (End distance), yzw: (Padding).
    vec4 cameraPosition; // xyz: (Camera position), w: (Sky fog distance).
    mat4x4 inverseProjection; // Inverse of the projection matrix (clip space -> view space). Not used in this shader.
    mat4x4 inverseView; // Inverse of the view matrix (view space -> world space). Not used in this shader.
    mat4x4 inverseViewProjection; // Inverse of the view projection matrix (clip space -> world space).
};

layout (set = 1, binding = 0) uniform texture2D fSceneColor;
layout (set = 1, binding = 1) uniform sampler fSceneColorSampler;

layout (set = 2, binding = 0) uniform texture2D fDepth;
layout (set = 2, binding = 1) uniform sampler fDepthSampler;

layout (location = 0) in vec2 fTexCoords;

layout (location = 0) out vec4 fFragColor;

/*
 * Builds a clip-space position from a screen UV and a depth value.
 *
 * UV space has its origin at the top-left, while clip space has its Y axis
 * pointing up, so the Y coordinate is flipped.
 */
vec4 getClipPosition(vec2 uv, float depth) {
    vec2 clipXY = uv * 2.0F - 1.0F;
    clipXY.y = -clipXY.y;
    
    return vec4(clipXY, depth, 1.0F);
}

/*
 * Reconstructs the world-space position of a pixel from its UV and depth.
 */
vec3 reconstructWorldPosition(vec2 uv, float depth) {
    vec4 clipPosition = getClipPosition(uv, depth);
    vec4 worldPosition = inverseViewProjection * clipPosition;
    
    // Undo the perspective projection. The w component is guarded against division by zero.
    worldPosition /= max(abs(worldPosition.w), EPSILON);
    
    return worldPosition.xyz;
}

/*
 * Calculates the world-space direction of the view ray through a pixel.
 *
 * It is used for sky pixels, because they have no depth and therefore no world position.
 * The ray is built from the same pixel on the near plane and the far plane.
 */
vec3 getWorldRayDirection(vec2 uv) {
    vec4 nearClip = getClipPosition(uv, 0.0F);
    vec4 farClip = getClipPosition(uv, 1.0F);
    
    vec4 nearWorld = inverseViewProjection * nearClip;
    vec4 farWorld = inverseViewProjection * farClip;
    
    nearWorld /= max(abs(nearWorld.w), EPSILON);
    farWorld /= max(abs(farWorld.w), EPSILON);
    
    return normalize(farWorld.xyz - nearWorld.xyz);
}

/*
 * Clamps a value to [0, 1] and applies the smoothstep curve to it.
 *
 * The curve starts and ends gently, so transitions built from it have no visible edges.
 */
float smooth01(float value) {
    value = clamp(value, 0.0F, 1.0F);
    return value * value * (3.0F - (2.0F * value));
}

/*
 * Natural exponential height fog for geometry.
 *
 * This version avoids forced cutoffs. Fog reaches full opacity naturally through
 * density and distance instead of a visible artificial wall.
 */
float getExponentialHeightFogFactor(vec3 worldPosition) {
    if (fogColor.w <= 0.0F || fogParameters.x <= 0.0F) {
        return 0.0F;
    }
    
    vec3 cameraToFragment = worldPosition - cameraPosition.xyz;
    float distanceToFragment = length(cameraToFragment);
    
    // The end distance is kept above the start distance, so the fade range can never be zero.
    float fogStartDistance = max(fogParameters.w, 0.0F);
    float fogEndDistance = max(fogDistanceParameters.x, fogStartDistance + EPSILON);
    float fadeRange = max(fogEndDistance - fogStartDistance, EPSILON);
    
    vec3 rayDirection = cameraToFragment / max(distanceToFragment, EPSILON);
    float heightFalloff = max(fogParameters.y, EPSILON);
    float cameraHeight = cameraPosition.y - fogParameters.z;
    
    // Fog density at the height of the camera. The exponent is clamped to avoid overflow.
    float startDensity = exp(clamp(-cameraHeight * heightFalloff, -50.0F, 50.0F));
    float heightIntegral;
    
    // How fast the density changes along the ray, which depends on how steeply the ray goes up or down.
    float heightFalloffAlongRay = rayDirection.y * heightFalloff;
    
    // For a (nearly) horizontal ray the density is constant, so the integral is a simple product.
    if (abs(heightFalloffAlongRay) < 0.0001F) {
        heightIntegral = startDensity * distanceToFragment;
    }
    // Otherwise, the density integrated along the ray has a closed form, so no ray marching is needed.
    else {
        float exponent = clamp(-heightFalloffAlongRay * distanceToFragment, -50.0F, 50.0F);
        heightIntegral = startDensity * (1.0F - exp(exponent)) / heightFalloffAlongRay;
    }
    
    // Beer-Lambert law: the more density the ray passes through, the less of the scene shines through.
    float exponentialFog = 1.0F - exp(-fogParameters.x * max(heightIntegral, 0.0F));
    
    // Smoothly fade fog in around the start distance.
    float startFadeRange = max(fadeRange * 0.25F, 10.0F);
    float startFade = smooth01((distanceToFragment - fogStartDistance) / startFadeRange);
    
    // Optional broad distance shaping. This makes endDistance meaningful without creating a wall.
    float distanceShape = smooth01((distanceToFragment - fogStartDistance) / fadeRange);
    
    // Keep the physically-shaped fog, but help it build smoothly over the configured distance range.
    float fogFactor = max(exponentialFog, distanceShape * exponentialFog);
    fogFactor *= startFade;
    
    // The fog intensity scales the final amount.
    return clamp(fogFactor * fogColor.w, 0.0F, 1.0F);
}

/*
 * Sky fog uses view direction because skybox has no scene depth.
 */
float getSkyFogFactor(vec3 viewDirection) {
    if (fogColor.w <= 0.0F || fogParameters.x <= 0.0F) {
        return 0.0F;
    }
    
    // The sky has no distance, so the sky fog distance stands in for it (at least the end distance).
    float fogStartDistance = max(fogParameters.w, 0.0F);
    float fogEndDistance = max(fogDistanceParameters.x, fogStartDistance + EPSILON);
    float skyDistance = max(cameraPosition.w, fogEndDistance);
    
    // 1 at horizon, 0 when looking straight up/down.
    float horizonAmount = 1.0F - clamp(abs(viewDirection.y), 0.0F, 1.0F);
    horizonAmount = smooth01(horizonAmount);
    
    // Wide horizon band. This keeps the fog from being only a thin line.
    float horizonBand = pow(horizonAmount, 0.65F);
    
    // Fog density at the height of the camera, the same as for geometry.
    float heightFalloff = max(fogParameters.y, EPSILON);
    float cameraHeight = cameraPosition.y - fogParameters.z;
    float startDensity = exp(clamp(-cameraHeight * heightFalloff, -50.0F, 50.0F));
    
    // Fog amount for a ray of the sky distance, as if the sky were geometry at that distance.
    float skyFog = 1.0F - exp(-fogParameters.x * startDensity * skyDistance);
    
    // Force the horizon to match dense far fog.
    // Looking upward keeps more of the original sky.
    float horizonFog = mix(skyFog * 0.25F, 1.0F, horizonBand);
    
    // The fog intensity scales the final amount.
    return clamp(horizonFog * fogColor.w, 0.0F, 1.0F);
}

void main() {
    vec2 uv = clamp(fTexCoords, vec2(0.0F), vec2(1.0F));
    
    vec4 sceneColor = texture(sampler2D(fSceneColor, fSceneColorSampler), uv);
    float depth = texture(sampler2D(fDepth, fDepthSampler), uv).r;
    
    float fogFactor;
    
    // Pixels at the far plane have no geometry, so they are the sky and are fogged by view direction.
    if (depth >= 0.9999F) {
        vec3 viewDirection = getWorldRayDirection(uv);
        fogFactor = getSkyFogFactor(viewDirection);
    }
    // Everything else is fogged by its distance and height.
    else {
        vec3 worldPosition = reconstructWorldPosition(uv, depth);
        fogFactor = getExponentialHeightFogFactor(worldPosition);
    }
    
    // Blend from the scene color towards the fog color. The alpha stays untouched.
    vec3 color = mix(sceneColor.rgb, fogColor.rgb, fogFactor);
    
    fFragColor = vec4(color, sceneColor.a);
}