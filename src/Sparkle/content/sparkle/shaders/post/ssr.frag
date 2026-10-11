#version 450

const float EPSILON = 0.000001F;

layout (std140, set = 0, binding = 0) uniform SsrBuffer {
    vec2 screenSize;
    float maxDistance;
    float maxSteps;
    float stepSize;
    float thickness;
    float strength;
    float edgeFade;
    mat4x4 projection;
    mat4x4 view;
    mat4x4 viewProjection;
    mat4x4 inverseProjection;
    mat4x4 inverseView;
    mat4x4 inverseViewProjection;
};

layout (set = 1, binding = 0) uniform texture2D fSceneColor;
layout (set = 1, binding = 1) uniform sampler fSceneColorSampler;

layout (set = 2, binding = 0) uniform texture2D fNormal;
layout (set = 2, binding = 1) uniform sampler fNormalSampler;

layout (set = 3, binding = 0) uniform texture2D fDepth;
layout (set = 3, binding = 1) uniform sampler fDepthSampler;

layout (set = 4, binding = 0) uniform texture2D fSsrMask;
layout (set = 4, binding = 1) uniform sampler fSsrMaskSampler;

layout (location = 0) in vec2 fTexCoords;

layout (location = 0) out vec4 fFragColor;

/*
 * Converts a stored normal from the [0, 1] range back into a unit vector in the [-1, 1] range.
 */
vec3 decodeNormal(vec3 encodedNormal) {
    return normalize(encodedNormal * 2.0F - 1.0F);
}

/*
 * Tests whether a UV coordinate lies inside the visible screen area.
 */
bool isInsideScreen(vec2 uv) {
    return uv.x >= 0.0F && uv.x <= 1.0F && uv.y >= 0.0F && uv.y <= 1.0F;
}

/*
 * Converts a UV coordinate into an integer texel coordinate.
 *
 * The depth texture defines the resolution, so the normal and mask textures
 * are expected to have the same size. The result is clamped to the texture
 * bounds, which emulates clamp-to-edge addressing for exact fetches.
 */
ivec2 getPixelCoord(vec2 uv) {
    ivec2 size = textureSize(sampler2D(fDepth, fDepthSampler), 0);
    
    // Keep the UV inside [0, 1] so the texel lookup can never go out of range.
    vec2 safeUv = clamp(uv, vec2(0.0F), vec2(1.0F));
    vec2 pixel = floor(safeUv * vec2(size));
    
    // A UV of exactly 1.0 would land one texel past the edge, so clamp it back.
    return ivec2(clamp(pixel, vec2(0.0F), vec2(size - ivec2(1))));
}

/*
 * Reads the encoded normal at a UV without any filtering.
 *
 * Filtering would blend normals across object edges, which produces incorrect reflection directions.
 */
vec4 fetchNormalExact(vec2 uv) {
    return texelFetch(sampler2D(fNormal, fNormalSampler), getPixelCoord(uv), 0);
}

/*
 * Reads the depth at a UV without any filtering.
 */
float fetchDepthExact(vec2 uv) {
    return texelFetch(sampler2D(fDepth, fDepthSampler), getPixelCoord(uv), 0).r;
}

/*
 * Reads the material reflectivity at a UV without any filtering.
 */
float fetchSsrMaskExact(vec2 uv) {
    return texelFetch(sampler2D(fSsrMask, fSsrMaskSampler), getPixelCoord(uv), 0).r;
}

/*
 * Builds a clip-space position from a screen UV and its depth.
 *
 * UV space has its origin at the top-left, while clip space has its Y axis pointing up, so the Y coordinate is flipped.
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
 * Projects a world-space position onto the screen and returns its UV.
 *
 * This is the inverse of reconstructWorldPosition. The result can lie outside [0, 1] when the position is off screen.
 */
vec2 projectWorldPosition(vec3 worldPosition) {
    vec4 clipPosition = viewProjection * vec4(worldPosition, 1.0F);
    
    // Perform the perspective divide to get normalized device coordinates.
    clipPosition /= max(abs(clipPosition.w), EPSILON);
    
    // Convert from clip space to UV space and flip Y back to a top-left origin.
    vec2 uv = clipPosition.xy * 0.5F + 0.5F;
    uv.y = 1.0F - uv.y;
    
    return uv;
}

/*
 * Calculates how strongly a reflection fades out near the screen borders.
 *
 * Reflections that hit close to the edge are unreliable because the real
 * reflected geometry is likely off screen. The fade width is controlled by
 * the edgeFade setting.
 */
float getEdgeFade(vec2 uv) {
    vec2 fadeIn = smoothstep(vec2(0.0F), vec2(edgeFade), uv);
    vec2 fadeOut = smoothstep(vec2(0.0F), vec2(edgeFade), 1.0F - uv);
    vec2 fade = fadeIn * fadeOut;
    
    return clamp(fade.x * fade.y, 0.0F, 1.0F);
}

/*
 * Calculates how strongly a reflection fades with the distance travelled
 * by the ray.
 *
 * The fade is linear up to maxDistance, but never drops below 0.35 so
 * distant reflections stay visible instead of vanishing completely.
 */
float getDistanceFade(float distanceTravelled) {
    float fade = clamp(1.0F - distanceTravelled / max(maxDistance, EPSILON), 0.0F, 1.0F);
    return clamp(fade, 0.35F, 1.0F);
}

/*
 * Returns the world-space position of the scene geometry visible at a UV.
 *
 * The raw depth is returned through sceneDepth so callers can detect empty
 * pixels (depth >= 1.0) such as the sky.
 */
vec3 getSceneWorldPosition(vec2 uv, out float sceneDepth) {
    vec2 safeUv = clamp(uv, vec2(0.0F), vec2(1.0F));
    
    sceneDepth = fetchDepthExact(safeUv);
    return reconstructWorldPosition(safeUv, sceneDepth);
}

/*
 * Returns how far behind the visible surface a ray position is, measured as the distance from the camera.
 *
 * Positive values mean the ray is behind the surface, negative values mean it is still in front of it.
 * A hit is only valid when the ray is behind the surface, but not further than the surface thickness.
 * Without this one-sided test, rays that are still in front of a surface would count as hits, which
 * shifts the reflected image toward the camera.
 */
float getRayDepthDelta(vec3 rayPosition, vec3 scenePosition) {
    vec3 cameraWorldPosition = inverseView[3].xyz;
    return length(rayPosition - cameraWorldPosition) - length(scenePosition - cameraWorldPosition);
}
/*
 * Refines a surface crossing with a binary search and validates it.
 *
 * The ray was in front of the surface at previousRayPosition and behind it at
 * currentRayPosition. The search narrows this down to the actual crossing point.
 * At a real surface the remaining depth delta becomes very small, while at a
 * depth discontinuity (the ray passing behind the edge of a nearer object) it
 * stays large. Only the first case is accepted as a hit.
 *
 * Returns true if the crossing is a valid hit. The hit position is returned through hitUv.
 */
bool refineHit(vec3 previousRayPosition, vec3 currentRayPosition, float currentDelta, out vec2 hitUv) {
    vec3 startPosition = previousRayPosition;
    vec3 endPosition = currentRayPosition;
    
    // Start with the coarse hit. The delta tracks how far behind the surface the best position so far is.
    hitUv = projectWorldPosition(currentRayPosition);
    float bestDelta = currentDelta;
    
    for (int i = 0; i < 8; i++) {
        vec3 midPosition = (startPosition + endPosition) * 0.5F;
        vec2 midUv = projectWorldPosition(midPosition);
        
        // The midpoint left the screen, so the hit must be closer to the start.
        if (!isInsideScreen(midUv)) {
            endPosition = midPosition;
            continue;
        }
        
        float sceneDepth;
        vec3 scenePosition = getSceneWorldPosition(midUv, sceneDepth);

        // The midpoint sees empty space, so the surface must be further along the ray.
        if (sceneDepth >= 1.0F) {
            startPosition = midPosition;
            continue;
        }
        
        float depthDelta = getRayDepthDelta(midPosition, scenePosition);
        
        // The midpoint is behind the surface: remember it and keep searching closer to the start.
        if (depthDelta > 0.0F) {
            hitUv = midUv;
            bestDelta = depthDelta;
            endPosition = midPosition;
        }
        // The midpoint is still in front of the surface, so search further along the ray.
        else {
            startPosition = midPosition;
        }
    }
    
    // A real surface leaves almost no gap. A large gap means the ray slipped behind a silhouette edge.
    return bestDelta < thickness;
}

void main() {
    vec2 uv = clamp(fTexCoords, vec2(0.0F), vec2(1.0F));
    
    // Skip pixels whose material is not reflective enough to be worth tracing.
    float materialReflection = clamp(fetchSsrMaskExact(uv), 0.0F, 1.0F);
    
    if (materialReflection < 0.02F) {
        fFragColor = vec4(0.0F);
        return;
    }
    
    // Skip empty pixels such as the sky, since there is no surface to reflect from.
    float depth = fetchDepthExact(uv);
    
    if (depth >= 1.0F) {
        fFragColor = vec4(0.0F);
        return;
    }
    
    // Read the surface normal and reconstruct where this pixel is in the world.
    vec4 normalSample = fetchNormalExact(uv);
    vec3 normalWorld = decodeNormal(normalSample.rgb);
    vec3 worldPosition = reconstructWorldPosition(uv, depth);
    
    // The camera position is stored in the translation column of the inverse view matrix.
    vec3 cameraWorldPosition = inverseView[3].xyz;
    
    // Reflect the camera-to-surface direction around the normal to get the ray direction.
    vec3 viewDirection = normalize(worldPosition - cameraWorldPosition);
    vec3 reflectionDirection = normalize(reflect(viewDirection, normalWorld));
    
    // Abort if the reflection direction is degenerate.
    if (length(reflectionDirection) <= EPSILON) {
        fFragColor = vec4(0.0F);
        return;
    }
    
    // Lift the start of the ray off the surface. The reconstructed position is slightly imprecise, and without this the ray hits the surface it starts on.
    float startBias = thickness * 0.25F;
    vec3 rayPosition = worldPosition + normalWorld * startBias;
    vec3 previousRayPosition = rayPosition;
    vec3 rayStep = reflectionDirection * stepSize;
    
    // Offset the start of the ray by a per-pixel fraction of a step. This turns visible contour lines into fine noise.
    float jitter = fract(52.9829189F * fract(dot(gl_FragCoord.xy, vec2(0.06711056F, 0.00583715F))));
    rayPosition += rayStep * jitter;
    
    vec2 hitUv = vec2(0.0F);
    float hitAmount = 0.0F;
    float hitDistance = stepSize * jitter;
    
    // The ray starts in front of the surface. A hit is the moment it goes from in front to behind.
    float previousDelta = -1.0F;
    
    int steps = int(maxSteps);
    
    // March the ray through the scene until it hits geometry or runs out of range.
    for (int i = 0; i < steps; i++) {
        previousRayPosition = rayPosition;
        rayPosition += rayStep;
        
        hitDistance += stepSize;
        
        // The ray travelled further than allowed.
        if (hitDistance > maxDistance) {
            break;
        }
        
        vec2 rayUv = projectWorldPosition(rayPosition);
        
        // The ray left the screen, so there is no information to reflect beyond this point.
        if (!isInsideScreen(rayUv)) {
            break;
        }
        
        float sceneDepth;
        vec3 sceneWorldPosition = getSceneWorldPosition(rayUv, sceneDepth);
        
        // The ray is currently over empty space such as the sky, which counts as being in front of everything.
        if (sceneDepth >= 1.0F) {
            previousDelta = -1.0F;
            continue;
        }
        
        float depthDelta = getRayDepthDelta(rayPosition, sceneWorldPosition);
        
        // The ray just went from in front of the surface to behind it, so it crossed the surface during this step.
        if (previousDelta <= 0.0F && depthDelta > 0.0F) {
            vec2 refinedUv;
            
            if (refineHit(previousRayPosition, rayPosition, depthDelta, refinedUv)) {
                hitUv = refinedUv;
                
                // Weaken reflections that hit near the screen border or came from far away.
                float edge = getEdgeFade(hitUv);
                float distanceFade = getDistanceFade(hitDistance);
                
                hitAmount = edge * distanceFade;
                break;
            }
        }
        
        previousDelta = depthDelta;
    }
    
    // No valid hit was found, so output a fully transparent reflection.
    if (hitAmount <= 0.0F) {
        fFragColor = vec4(0.0F);
        return;
    }
    
    // The reflected color is the scene color at the hit position.
    vec3 reflectionColor = texture(sampler2D(fSceneColor, fSceneColorSampler), clamp(hitUv, vec2(0.0F), vec2(1.0F))).rgb;
    
    // The alpha channel carries the final reflection opacity for the composite pass.
    float reflectionStrength = clamp(hitAmount * strength * materialReflection, 0.0F, 1.0F);
    
    fFragColor = vec4(reflectionColor, reflectionStrength);
}