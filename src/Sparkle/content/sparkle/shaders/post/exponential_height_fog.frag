#version 450

const float EPSILON = 0.000001F;
const float LINE_INTEGRAL_EPSILON = 0.01F;
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
 * Performs a stable perspective divide while preserving the sign of w.
 *
 * Using abs(w) here can mirror reconstructed positions for projections where
 * clip-space reconstruction produces a negative homogeneous w.
 */
vec4 perspectiveDivide(vec4 position) {
    float w = abs(position.w) > EPSILON ? position.w : (position.w < 0.0F ? -EPSILON : EPSILON);
    return position / w;
}

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
    
    // Undo the perspective projection while preserving the sign of w.
    worldPosition = perspectiveDivide(worldPosition);
    
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
    
    nearWorld = perspectiveDivide(nearWorld);
    farWorld = perspectiveDivide(farWorld);
    
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
 *
 * The noise only depends on the pixel position, so it does not flicker between frames.
 * It is an interleaved gradient noise, which has an even distribution at pixel level.
 */
float getDitherNoise(vec2 pixelPosition) {
    return fract(52.9829189F * fract(dot(pixelPosition, vec2(0.06711056F, 0.00583715F))));
}

/*
 * Adds very small dithering in display-space-sized steps.
 *
 * This is intentionally tiny: enough to break fog/sky rings, not enough to look noisy.
 * The noise is centered around zero and spans one 8-bit color step.
 */
vec3 applyFogDither(vec3 color) {
    float noise = getDitherNoise(gl_FragCoord.xy) - 0.5F;
    return color + noise / 255.0F;
}

/*
 * Samples the depth buffer without filtering.
 *
 * Depth values are non-linear and must not be linearly filtered before world-position
 * reconstruction. Filtered depth creates fake positions, which can flicker badly when
 * looking at far terrain or when the camera is high above the scene.
 */
float sampleDepthNearest(vec2 uv) {
    ivec2 size = textureSize(sampler2D(fDepth, fDepthSampler), 0);
    ivec2 pixel = ivec2(clamp(uv, vec2(0.0F), vec2(1.0F)) * vec2(size));
    pixel = clamp(pixel, ivec2(0), size - ivec2(1));
    
    return texelFetch(sampler2D(fDepth, fDepthSampler), pixel, 0).r;
}

/*
 * Returns whether a depth value belongs to the untouched far plane.
 *
 * Only the actual cleared depth should count as sky. Real distant geometry can
 * be extremely close to 1.0 with a large far plane, and treating it as sky makes
 * fog switch/flicker when flying high.
 */
bool isSkyDepth(float depth) {
    return depth >= 1.0F;
}

/*
 * Returns a stable linear world distance for a screen pixel.
 *
 * Sky pixels use the configured sky fog distance because they have no scene depth.
 */
float getSceneDistance(vec2 uv, float depth) {
    
    // The sky has no position, so the configured distance stands in for it.
    if (isSkyDepth(depth)) {
        return cameraPosition.w;
    }
    
    vec3 worldPosition = reconstructWorldPosition(uv, depth);
    return length(worldPosition - cameraPosition.xyz);
}

/*
 * Returns the square of a value.
 */
float pow2(float value) {
    return value * value;
}

/*
 * Calculates the density of exponential height fog, integrated along one ray.
 *
 * The density falls off exponentially with height, so the fog is thicker near the ground. The
 * result is the average density along the ray (for a ray parameter from 0 to 1), multiplied
 * by the density at the start of the ray. The caller multiplies it with the ray length, and
 * exp2(-integral) is then the transmittance. The exact formula divides by the height change, so a
 * Taylor approximation takes over for almost-horizontal rays, where the division would become unstable.
 *
 * heightFalloff: How quickly the density falls off with height.
 * rayDirectionY: The vertical distance that the ray covers, positive when it goes up. It is not normalized.
 * rayOriginDensity: The density at the start of the ray.
 */
float calculateLineIntegralShared(float heightFalloff, float rayDirectionY, float rayOriginDensity) {
    
    // How much the density changes along the ray, limited so exp2 below cannot overflow.
    float falloff = max(-127.0F, heightFalloff * rayDirectionY);
    
    // First two terms of the Taylor series of the integral, used for almost-horizontal rays.
    float log2Value = log(2.0F);
    float lineIntegralTaylor = log2Value - 0.5F * pow2(log2Value) * falloff;
    
    if (abs(falloff) <= LINE_INTEGRAL_EPSILON) {
        return rayOriginDensity * lineIntegralTaylor;
    }
    
    // The exact integral of the exponential density along the ray.
    float lineIntegral = (1.0F - exp2(-falloff)) / falloff;
    return rayOriginDensity * lineIntegral;
}

/*
 * Calculates exponential height fog for a world position.
 *
 * Returns:
 *   rgb: Fog color contribution.
 *   a:   Remaining scene transmittance.
 *
 * Final composition:
 *   finalColor = sceneColor * transmittance + fogContribution
 *
 * The amount of fog depends on the density summed along the ray from the camera to the
 * position, so thick fog near the ground affects a ray that travels through it for a long distance.
 * The end distance is not a cutoff. The fog keeps building up naturally behind it.
 */
vec4 getExponentialHeightFog(vec3 worldPosition) {
    
    // Without intensity or density there is no fog, so the whole scene shines through.
    if (fogColor.w <= 0.0F || fogParameters.x <= 0.0F) {
        return vec4(0.0F, 0.0F, 0.0F, 1.0F);
    }
    
    vec3 cameraToFragment = worldPosition - cameraPosition.xyz;
    float distanceSquared = dot(cameraToFragment, cameraToFragment);
    
    // A position at the camera has no ray, which also avoids dividing by zero below.
    if (distanceSquared <= EPSILON) {
        return vec4(0.0F, 0.0F, 0.0F, 1.0F);
    }
    
    // One inverse square root gives both the inverse distance and the distance.
    float inverseDistance = inversesqrt(distanceSquared);
    float distanceToFragment = distanceSquared * inverseDistance;
    
    // The part of the ray that is fogged. It starts as the whole ray and is shortened below if the fog starts further away.
    float rayLength = distanceToFragment;
    
    // The vertical distance that the fogged part of the ray covers, positive when it goes up.
    float rayDirectionY = cameraToFragment.y;
    
    // The intensity scales the density, so a lower intensity gives thinner fog.
    float fogDensity = fogParameters.x * fogColor.w;
    float heightFalloff = max(fogParameters.y, EPSILON);
    float baseHeight = fogParameters.z;
    float startDistance = max(fogParameters.w, 0.0F);
    
    // The density at the height of the camera: the density falls off by a factor of 2 for every 1 / falloff units of height above the base.
    float cameraHeight = cameraPosition.y - baseHeight;
    float rayOriginDensity = fogDensity * exp2(-max(-127.0F, heightFalloff * cameraHeight));
    
    // The fog starts at the start distance and not directly at the camera, so the ray is cut at that point.
    if (startDistance > 0.0F && distanceToFragment > startDistance) {
        
        // How far along the ray (from 0 to 1) the start distance lies.
        float excludeIntersectionTime = startDistance * inverseDistance;
        
        // The height of the point where the fog starts, and the vertical distance that remains behind it.
        float cameraToExclusionY = excludeIntersectionTime * cameraToFragment.y;
        float exclusionY = cameraPosition.y + cameraToExclusionY;
        float exclusionToFragmentY = cameraToFragment.y - cameraToExclusionY;
        
        // Only the part of the ray behind that point is fogged, and its density starts at the height of that point.
        rayLength = (1.0F - excludeIntersectionTime) * distanceToFragment;
        rayDirectionY = exclusionToFragmentY;
        
        float exclusionHeight = exclusionY - baseHeight;
        rayOriginDensity = fogDensity * exp2(-max(-127.0F, heightFalloff * exclusionHeight));
    }
    // The position is closer than the start distance, so it has no fog.
    else if (startDistance > 0.0F) {
        return vec4(0.0F, 0.0F, 0.0F, 1.0F);
    }
    
    // The total amount of fog along the fogged part of the ray.
    float sharedLineIntegral = calculateLineIntegralShared(heightFalloff, rayDirectionY, rayOriginDensity);
    float heightLineIntegral = sharedLineIntegral * rayLength;
    
    // Transmittance: the part of the scene that shines through the fog. Lower values mean that more fog color replaces the scene color.
    float transmittance = clamp(exp2(-heightLineIntegral), 0.0F, 1.0F);
    
    // A lower limit for the transmittance. It is zero, so the fog can become fully opaque, because the buffer has no setting for a minimum.
    float minTransmittance = 0.0F;
    transmittance = max(transmittance, minTransmittance);
    
    // The fog fades in softly after the start distance. The end distance is not a cutoff: it only sets
    // the length of this fade-in (a quarter of the range between start and end, at least 10 units).
    float fogStartDistance = max(fogParameters.w, 0.0F);
    float fogEndDistance = max(fogDistanceParameters.x, fogStartDistance + EPSILON);
    float fadeRange = max(fogEndDistance - fogStartDistance, EPSILON);
    float startFadeRange = max(min(fadeRange * 0.25F, fadeRange), 10.0F);
    float startFade = smooth01((distanceToFragment - fogStartDistance) / startFadeRange);
    
    // Apply the fade to the opacity, and convert it back to the transmittance.
    float fogOpacity = (1.0F - transmittance) * startFade;
    transmittance = 1.0F - fogOpacity;
    
    vec3 fogContribution = fogColor.rgb * fogOpacity;
    
    return vec4(fogContribution, transmittance);
}

/*
 * Samples scene transmittance along the current pixel towards the light direction.
 *
 * This is a cheap screen-space approximation:
 * - march along the projected sun direction on screen,
 * - use depth discontinuities as occluders,
 * - fade with distance/fog so rays sit naturally inside the fog.
 *
 * Returns the amount of god ray light for the pixel in the range [0, 1].
 */
float getGodRayFactor(vec2 uv, float currentDepth, vec3 viewDirection, float fogFactor) {
    
    // Without intensity there are no god rays.
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
    
    // The position of the pixel in the world. Sky pixels use the sky fog distance along their view ray.
    vec3 worldPosition = isSkyDepth(currentDepth) ? cameraPosition.xyz + viewDirection * cameraPosition.w : reconstructWorldPosition(uv, currentDepth);
    float currentDistance = length(worldPosition - cameraPosition.xyz);
    
    // Nearby surfaces barely need fullscreen shaft accumulation.
    if (!isSkyDepth(currentDepth) && currentDistance < 8.0F) {
        return 0.0F;
    }
    
    float maxRayLength = max(godRayColorLength.w, EPSILON);
    
    // Project a point in the opposite light direction into screen space.
    // Moving from the current pixel toward that projected position approximates tracing toward the sun.
    vec3 lightProbeWorld = worldPosition - lightDirection * maxRayLength;
    vec4 lightProbeClip = inverse(inverseViewProjection) * vec4(lightProbeWorld, 1.0F);
    lightProbeClip = perspectiveDivide(lightProbeClip);
    
    vec2 lightProbeUv = lightProbeClip.xy * 0.5F + 0.5F;
    lightProbeUv.y = 1.0F - lightProbeUv.y;
    
    float sampleCountFloat = clamp(godRayParameters.x, 1.0F, float(MAX_GOD_RAY_SAMPLES));
    
    // Weak rays do not need full quality.
    sampleCountFloat *= mix(0.35F, 1.0F, viewLightAmount);
    sampleCountFloat = clamp(sampleCountFloat, 6.0F, float(MAX_GOD_RAY_SAMPLES));
    
    int sampleCount = int(sampleCountFloat);
    
    // One step of the march on the screen. The density sets which fraction of the way to the light is covered.
    vec2 rayVector = (lightProbeUv - uv) * godRayParameters.y / sampleCountFloat;
    
    float illuminationDecay = 1.0F;
    float rayFactor = 0.0F;
    vec2 sampleUv = uv;
    
    // March towards the light and collect the light that passes through open space.
    for (int index = 0; index < MAX_GOD_RAY_SAMPLES; index++) {
        
        // The loop bound is constant, so the actual sample count ends the loop early.
        if (index >= sampleCount) {
            break;
        }
        
        sampleUv += rayVector;
        
        // The march left the screen, so there is no information beyond this point.
        if (sampleUv.x < 0.0F || sampleUv.x > 1.0F || sampleUv.y < 0.0F || sampleUv.y > 1.0F) {
            break;
        }
        
        float sampleDepth = sampleDepthNearest(sampleUv);
        float sampleDistance = getSceneDistance(sampleUv, sampleDepth);
        
        // Nearby geometry blocks the ray. Sky/far pixels transmit it.
        float depthFadeDistance = max(godRayPostParameters.y, EPSILON);
        float distanceDelta = sampleDistance - currentDistance;
        float transmittance = smooth01(distanceDelta / depthFadeDistance);
        
        // The sky never blocks the light.
        if (isSkyDepth(sampleDepth)) {
            transmittance = 1.0F;
        }
        
        // Every sample adds its light, and the following samples count less.
        rayFactor += transmittance * illuminationDecay * godRayParameters.z;
        illuminationDecay *= godRayParameters.w;
    }
    
    // Exposure, the angle to the light and the horizon fade scale the collected light.
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

void main() {
    vec2 uv = clamp(fTexCoords, vec2(0.0F), vec2(1.0F));
    
    vec4 sceneColor = texture(sampler2D(fSceneColor, fSceneColorSampler), uv);
    float depth = sampleDepthNearest(uv);
    
    vec3 viewDirection;
    vec3 fogWorldPosition;
    
    // Sky pixels have no position, so a point far along their view ray stands in for it.
    if (isSkyDepth(depth)) {
        viewDirection = getWorldRayDirection(uv);
        fogWorldPosition = cameraPosition.xyz + viewDirection * max(cameraPosition.w, fogDistanceParameters.x);
    }
    // Everything else is fogged at its real position in the world.
    else {
        fogWorldPosition = reconstructWorldPosition(uv, depth);
        viewDirection = normalize(fogWorldPosition - cameraPosition.xyz);
    }
    
    vec4 fogResult = getExponentialHeightFog(fogWorldPosition);
    
    // Composition: the scene shines through by the transmittance, and the fog color is added on top.
    vec3 color = sceneColor.rgb * fogResult.a + fogResult.rgb;
    
    // God rays still need a fog amount, so convert transmittance into fog opacity.
    float fogFactor = 1.0F - fogResult.a;
    float godRayFactor = getGodRayFactor(uv, depth, viewDirection, fogFactor);
    color += godRayColorLength.rgb * godRayFactor;
    
    // Break up banding rings in smooth fog gradients.
    color = applyFogDither(color);
    
    fFragColor = vec4(color, sceneColor.a);
}