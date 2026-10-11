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
 * Refines a coarse ray hit with a binary search.
 *
 * The ray march steps in fixed increments, so the first detected hit can be
 * up to one step past the real surface. This searches between the last
 * position before the hit and the hit position to find the intersection
 * more precisely, which reduces visible stepping artifacts.
 */
vec2 refineHit(vec3 previousRayPosition, vec3 currentRayPosition) {
    vec3 startPosition = previousRayPosition;
    vec3 endPosition = currentRayPosition;
    
    // Fall back to the coarse hit if no refined position passes the thickness test.
    vec2 bestUv = projectWorldPosition(currentRayPosition);
    
    for (int i = 0; i < 5; i++) {
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
        
        float depthDifference = abs(length(midPosition - scenePosition));
        
        // Inside the surface thickness: remember this hit and keep searching closer to the start.
        if (depthDifference < thickness) {
            bestUv = midUv;
            endPosition = midPosition;
        }
        // Not touching the surface yet: continue searching further along the ray.
        else {
            startPosition = midPosition;
        }
    }
    
    return bestUv;
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
    
    // The ray starts at the surface and advances by a fixed world-space step.
    vec3 rayPosition = worldPosition;
    vec3 previousRayPosition = rayPosition;
    vec3 rayStep = reflectionDirection * stepSize;
    
    vec2 hitUv = vec2(0.0F);
    float hitAmount = 0.0F;
    float hitDistance = 0.0F;
    
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
        
        // The ray is currently over empty space such as the sky, so keep marching.
        if (sceneDepth >= 1.0F) {
            continue;
        }
        
        float depthDifference = abs(length(rayPosition - sceneWorldPosition));
        
        // The ray is within the surface thickness, which counts as a hit.
        if (depthDifference < thickness) {
            hitUv = refineHit(previousRayPosition, rayPosition);
            
            // Weaken reflections that hit near the screen border or came from far away.
            float edge = getEdgeFade(hitUv);
            float distanceFade = getDistanceFade(hitDistance);
            
            hitAmount = edge * distanceFade;
            break;
        }
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