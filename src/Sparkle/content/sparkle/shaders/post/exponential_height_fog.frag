#version 450

const float EPSILON = 0.000001F;
const int MAX_GOD_RAY_SAMPLES = 96;

layout (std140, set = 0, binding = 0) uniform FogBuffer {
    vec4 fogColor; // rgb: (Color), w: (Intensity).
    vec4 fogParameters; // x: (Density), y: (Height falloff), z: (Base height), w: (Start distance).
    vec4 fogDistanceParameters; // x: (End distance), yzw: (Padding).
    vec4 cameraPosition; // xyz: (Camera position), w: (Sky fog distance).
    vec4 godRayDirectionIntensity; // xyz: (World-space light direction), w: (God ray intensity).
    vec4 godRayColorLength; // rgb: (God ray color), w: (Max ray length).
    vec4 godRayParameters; // x: (Sample count), y: (Density), z: (Weight), w: (Decay).
    vec4 godRayPostParameters; // x: (Exposure), y: (Depth fade distance), z: (Horizon fade), w: (Padding).
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
 * Small stable screen-space noise used to hide visible gradient banding in foggy sky.
 */
float getDitherNoise(vec2 pixelPosition) {
    return fract(52.9829189F * fract(dot(pixelPosition, vec2(0.06711056F, 0.00583715F))));
}

/*
 * Adds very small dithering in display-space-sized steps.
 *
 * This is intentionally tiny: enough to break fog/sky rings, not enough to look noisy.
 */
vec3 applyFogDither(vec3 color) {
    float noise = getDitherNoise(gl_FragCoord.xy) - 0.5F;
    return color + noise / 255.0F;
}

/*
 * Returns a stable linear depth-like world distance for a screen pixel.
 * Sky pixels use the configured sky fog distance.
 */
float getSceneDistance(vec2 uv, float depth) {
    if (depth >= 0.9999F) {
        return cameraPosition.w;
    }
    
    vec3 worldPosition = reconstructWorldPosition(uv, depth);
    return length(worldPosition - cameraPosition.xyz);
}

/*
 * Samples scene transmittance along the current pixel towards the light direction.
 *
 * This is a cheap screen-space approximation:
 * - march along the projected sun direction on screen,
 * - use depth discontinuities as occluders,
 * - fade with distance/fog so rays sit naturally inside the fog.
 */
float getGodRayFactor(vec2 uv, float currentDepth, vec3 viewDirection, float fogFactor) {
    if (godRayDirectionIntensity.w <= 0.0F) {
        return 0.0F;
    }
    
    vec3 lightDirection = normalize(godRayDirectionIntensity.xyz);
    
    // God rays are strongest when looking toward the incoming light.
    float viewLightAmount = clamp(dot(viewDirection, -lightDirection), 0.0F, 1.0F);
    viewLightAmount = pow(viewLightAmount, 1.25F);
    
    // Early out before the expensive sample loop.
    if (viewLightAmount <= 0.015F) {
        return 0.0F;
    }
    
    // Reduce shafts when looking too far away from the horizon if desired.
    float horizonAmount = 1.0F - clamp(abs(viewDirection.y), 0.0F, 1.0F);
    float horizonFade = mix(1.0F, smooth01(horizonAmount), clamp(godRayPostParameters.z, 0.0F, 1.0F));
    
    // Early out before the expensive sample loop.
    if (horizonFade <= 0.015F) {
        return 0.0F;
    }
    
    vec3 worldPosition = currentDepth >= 0.9999F ? cameraPosition.xyz + viewDirection * cameraPosition.w : reconstructWorldPosition(uv, currentDepth);
    
    float currentDistance = length(worldPosition - cameraPosition.xyz);
    
    // Nearby surfaces barely need fullscreen shaft accumulation.
    if (currentDepth < 0.9999F && currentDistance < 8.0F) {
        return 0.0F;
    }
    
    float maxRayLength = max(godRayColorLength.w, EPSILON);
    
    // Project a point in the opposite light direction into screen space.
    // Moving from the current pixel toward that projected position approximates tracing toward the sun.
    vec3 lightProbeWorld = worldPosition - lightDirection * maxRayLength;
    vec4 lightProbeClip = inverse(inverseViewProjection) * vec4(lightProbeWorld, 1.0F);
    lightProbeClip.xyz /= max(abs(lightProbeClip.w), EPSILON);
    
    vec2 lightProbeUv = lightProbeClip.xy * 0.5F + 0.5F;
    lightProbeUv.y = 1.0F - lightProbeUv.y;
    
    float sampleCountFloat = clamp(godRayParameters.x, 1.0F, float(MAX_GOD_RAY_SAMPLES));
    
    // Weak rays do not need full quality.
    sampleCountFloat *= mix(0.35F, 1.0F, viewLightAmount);
    sampleCountFloat = clamp(sampleCountFloat, 6.0F, float(MAX_GOD_RAY_SAMPLES));
    
    int sampleCount = int(sampleCountFloat);
    
    vec2 rayVector = (lightProbeUv - uv) * godRayParameters.y / sampleCountFloat;
    
    float illuminationDecay = 1.0F;
    float rayFactor = 0.0F;
    vec2 sampleUv = uv;
    
    for (int index = 0; index < MAX_GOD_RAY_SAMPLES; index++) {
        if (index >= sampleCount) {
            break;
        }
        
        sampleUv += rayVector;
        
        if (sampleUv.x < 0.0F || sampleUv.x > 1.0F || sampleUv.y < 0.0F || sampleUv.y > 1.0F) {
            break;
        }
        
        float sampleDepth = texture(sampler2D(fDepth, fDepthSampler), sampleUv).r;
        float sampleDistance = getSceneDistance(sampleUv, sampleDepth);
        
        // Nearby geometry blocks the ray. Sky/far pixels transmit it.
        float depthFadeDistance = max(godRayPostParameters.y, EPSILON);
        float distanceDelta = sampleDistance - currentDistance;
        float transmittance = smooth01(distanceDelta / depthFadeDistance);
        
        if (sampleDepth >= 0.9999F) {
            transmittance = 1.0F;
        }
        
        rayFactor += transmittance * illuminationDecay * godRayParameters.z;
        illuminationDecay *= godRayParameters.w;
    }
    
    rayFactor *= godRayPostParameters.x;
    rayFactor *= viewLightAmount;
    rayFactor *= horizonFade;
    
    // God rays need participating media, but keep a baseline so light fog/dust still shows shafts.
    float fogAtmosphere = fogFactor + fogParameters.x * 18.0F;
    float dustAtmosphere = 0.35F;
    float atmosphericAmount = clamp(max(fogAtmosphere, dustAtmosphere), 0.0F, 1.0F);
    rayFactor *= smooth01(atmosphericAmount);
    
    return clamp(rayFactor * godRayDirectionIntensity.w, 0.0F, 1.0F);
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
    float horizonBand = pow(horizonAmount, 1.15F);
    
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
    vec3 viewDirection;
    
    // Pixels at the far plane have no geometry, so they are the sky and are fogged by view direction.
    if (depth >= 0.9999F) {
        viewDirection = getWorldRayDirection(uv);
        fogFactor = getSkyFogFactor(viewDirection);
    }
    // Everything else is fogged by its distance and height.
    else {
        vec3 worldPosition = reconstructWorldPosition(uv, depth);
        viewDirection = normalize(worldPosition - cameraPosition.xyz);
        fogFactor = getExponentialHeightFogFactor(worldPosition);
    }
    
    // Blend from the scene color towards the fog color. The alpha stays untouched.
    vec3 color = mix(sceneColor.rgb, fogColor.rgb, fogFactor);
    
    // Add screen-space god rays after fog, so shafts sit inside the atmospheric layer.
    float godRayFactor = getGodRayFactor(uv, depth, viewDirection, fogFactor);
    color += godRayColorLength.rgb * godRayFactor;
    
    color = applyFogDither(color);
    
    fFragColor = vec4(color, sceneColor.a);
}