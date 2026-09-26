#version 450

#define MAX_MAPS_COUNT 8

struct MaterialMap {
    vec4 color;
    float value;
};

layout(std140, set = 2, binding = 0) uniform MaterialBuffer {
    int renderMode;
    MaterialMap maps[MAX_MAPS_COUNT];
};

layout(set = 3, binding = 0) uniform texture2DArray fSources;
layout(set = 3, binding = 1) uniform sampler fSourcesSampler;

layout(set = 4, binding = 0) uniform texture2D fTiles;
layout(set = 4, binding = 1) uniform sampler fTilesSampler;

layout(location = 0) in vec3 fTerrainPosition;
layout(location = 1) in vec4 fColor;
layout(location = 2) in vec3 fSurfacePosition;

layout(location = 0) out vec4 fFragColor;

/*
 * Returns the terrain material assigned to a logical tile cell.
 *
 * Coordinates outside the tile map use the fallback material stored in
 * material map 1. Tile values are stored as normalized 8-bit values.
 */
int getTile(ivec2 cell) {
    ivec2 size = textureSize(sampler2D(fTiles, fTilesSampler), 0);
    
    if (any(lessThan(cell, ivec2(0))) || any(greaterThanEqual(cell, size))) {
        return int(maps[1].value);
    }
    
    return int(round(texelFetch(sampler2D(fTiles, fTilesSampler), cell, 0).r * 255.0F));
}

/*
 * Sorts two integer values into ascending order.
 *
 * This helper is used by the small sorting network that orders the four
 * materials surrounding the current terrain position.
 */
void sortPair(inout int first, inout int second) {
    int lower = min(first, second);
    
    second = max(first, second);
    first = lower;
}

/*
 * Selects the dominant projection axis for the supplied surface normal.
 *
 * The Y axis is preferred when multiple axes are nearly equal so projection
 * selection remains stable across small derivative and camera changes.
 */
int getProjection(vec3 normal) {
    vec3 direction = abs(normal);
    float largest = max(direction.x, max(direction.y, direction.z));
    
    if (direction.y >= largest - 0.001F) {
        return 1;
    }
    
    return direction.x >= largest - 0.001F ? 0 : 2;
}

/*
 * Projects a 3D terrain position onto the selected planar axis.
 *
 * Axis 0 uses the YZ plane, axis 1 uses the XZ plane, and axis 2 uses
 * the XY plane.
 */
vec2 getProjectedPosition(vec3 position, int axis) {
    if (axis == 1) {
        return position.xz;
    }
    
    return axis == 0 ? position.zy : position.xy;
}

/*
 * Calculates the terrain position used when evaluating the material mask.
 *
 * For non-horizontal projections, the position is shifted along the surface
 * so the mask is evaluated at its actual texel centers instead of stretched
 * horizontal texel positions.
 */
vec3 getMaskPosition(vec3 normal, int axis, vec2 projected, int size) {
    if (axis == 1) {
        return fTerrainPosition;
    }
    
    vec2 offset = (floor(projected * float(size)) + vec2(0.5F)) / float(size) - projected;
    
    if (axis == 0) {
        return fTerrainPosition + vec3(-(normal.z * offset.x + normal.y * offset.y) / normal.x, offset.y, offset.x);
    }
    
    return fTerrainPosition + vec3(offset.x, offset.y, -(normal.x * offset.x + normal.y * offset.y) / normal.z);
}

/*
 * Samples a terrain material from the source texture array.
 *
 * The source coordinates are converted to the requested mip levels manually.
 * Each mip level uses nearest-texel sampling, while the two mip levels are
 * linearly blended to provide smooth distance filtering without mixing
 * different terrain materials.
 */
vec4 sampleSource(ivec2 sourcePixel, int size, int layer, float lod) {
    int levelCount = textureQueryLevels(sampler2DArray(fSources, fSourcesSampler));
    
    int lowerLevel = int(floor(lod));
    int upperLevel = min(lowerLevel + 1, levelCount - 1);
    
    vec2 uv = (vec2(sourcePixel) + vec2(0.5F)) / float(size);
    
    ivec2 lowerSize = textureSize(sampler2DArray(fSources, fSourcesSampler), lowerLevel).xy;
    ivec2 lowerPixel = min(ivec2(uv * vec2(lowerSize)), lowerSize - 1);
    
    vec4 lowerColor = texelFetch(sampler2DArray(fSources, fSourcesSampler), ivec3(lowerPixel, layer), lowerLevel);
    
    if (upperLevel == lowerLevel || fract(lod) == 0.0F) {
        return lowerColor;
    }
    
    ivec2 upperSize = textureSize(sampler2DArray(fSources, fSourcesSampler), upperLevel).xy;
    ivec2 upperPixel = min(ivec2(uv * vec2(upperSize)), upperSize - 1);
    
    vec4 upperColor = texelFetch(sampler2DArray(fSources, fSourcesSampler), ivec3(upperPixel, layer), upperLevel);
    
    return mix(lowerColor, upperColor, fract(lod));
}

/*
 * Calculates the surface normal and projection information required for
 * terrain material mapping.
 *
 * Screen-space derivatives are used so the normal follows the actual
 * rendered surface rather than relying on the original vertex normal.
 */
void getSurfaceData(out vec3 normal, out int axis, out float footprint) {
    vec3 positionDx = dFdx(fSurfacePosition);
    vec3 positionDy = dFdy(fSurfacePosition);
    
    vec3 surfaceNormal = cross(positionDx, positionDy);
    float normalLength = length(surfaceNormal);
    
    normal = normalLength > 0.0F ? surfaceNormal / normalLength : vec3(0.0F, 1.0F, 0.0F);
    axis = getProjection(normal);
    
    int size = textureSize(sampler2DArray(fSources, fSourcesSampler), 0).x;
    
    vec2 texelsDx = getProjectedPosition(positionDx, axis) * float(size);
    vec2 texelsDy = getProjectedPosition(positionDy, axis) * float(size);
    
    footprint = max(length(texelsDx), length(texelsDy));
}

/*
 * Calculates the mip level required for the current terrain fragment.
 *
 * The projected pixel footprint is converted to a logarithmic mip level and
 * clamped to the available source texture levels.
 */
float getSourceLod(float footprint) {
    int levelCount = textureQueryLevels(sampler2DArray(fSources, fSourcesSampler));
    return clamp(log2(max(1.0F, footprint)), 0.0F, float(levelCount - 1));
}

/*
 * Calculates a deterministic, pixel-aligned noise value.
 *
 * The integer hash ensures neighboring terrain mask edges use identical
 * values, preventing visible discontinuities along material boundaries.
 */
float getMaskNoise(int axis, ivec2 pixel, ivec2 sourcePixel, int size, float footprint) {
    uvec2 noisePixel = uvec2((axis == 1 ? pixel : sourcePixel) % (size - 1));
    
    uint hash = noisePixel.x * 374761393u + noisePixel.y * 668265263u;
    hash = (hash ^ (hash >> 13)) * 1274126177u;
    
    float noise = (float(hash & 255u) / 255.0F - 0.5F) * 0.12F;
    return noise * clamp(2.0F - footprint, 0.0F, 1.0F);
}

/*
 * Calculates the coverage of a material inside the four surrounding tile
 * corners.
 *
 * The resulting value describes how strongly the material occupies the
 * current surface location and is adjusted for steep terrain so the mask
 * maintains a consistent physical width.
 */
float getMaterialCoverage(int layer, ivec4 corners, vec2 uv, vec3 normal) {
    vec4 mask = vec4(greaterThanEqual(corners, ivec4(layer)));
    
    float north = mix(mask.x, mask.y, uv.x);
    float south = mix(mask.z, mask.w, uv.x);
    
    float coverage = mix(north, south, uv.y) - 0.5F;
    
    vec3 gradient = vec3(mix(mask.y - mask.x, mask.w - mask.z, uv.y), 0.0F, south - north);
    
    float gradientLength = length(gradient);
    
    if (gradientLength > 0.0001F) {
        vec3 tangentGradient = gradient - normal * dot(gradient, normal);
        coverage /= max(length(tangentGradient) / gradientLength, 0.0001F);
    }
    
    return coverage;
}

/*
 * Applies the terrain material mask and returns the resulting color.
 *
 * Materials are evaluated from highest to lowest priority. Fully covered
 * regions sample the material texture, while narrow transition regions use
 * the shadow texture to soften the boundary between materials.
 */
vec4 getTerrainColor(ivec4 corners, ivec2 pixel, ivec2 sourcePixel, vec2 uv, vec3 normal, float lod, float noise, int size) {
    ivec4 layers = corners;
    
    sortPair(layers.x, layers.y);
    sortPair(layers.z, layers.w);
    sortPair(layers.x, layers.z);
    sortPair(layers.y, layers.w);
    sortPair(layers.y, layers.z);
    
    vec4 color = vec4(0.0F);
    int below = layers.x;
    
    for (int index = 0; index < 4; index++) {
        int layer = layers[index];
        
        if (index > 0 && layer == layers[index - 1]) {
            continue;
        }
        
        float coverage = getMaterialCoverage(layer, corners, uv, normal) - noise;
        
        if (coverage >= 0.0F) {
            color = sampleSource(sourcePixel, size, layer, lod);
            below = layer;
        }
        else if (coverage > -0.12F) {
            float transition = smoothstep(0.0F, 1.0F, (coverage + 0.12F) / 0.12F);
            
            vec4 belowColor = sampleSource(sourcePixel, size, below, lod);
            vec4 layerColor = sampleSource(sourcePixel, size, layer, lod);
            
            float shadow = mix(0.75F, 1.0F, transition);
            color = mix(belowColor, layerColor, transition);
            color.rgb *= shadow;
        }
    }
    
    return color;
}

void main() {
    vec3 normal;
    int axis;
    float footprint;
    getSurfaceData(normal, axis, footprint);
    
    int size = textureSize(sampler2DArray(fSources, fSourcesSampler), 0).x;
    float lod = getSourceLod(footprint);
    
    vec2 projected = getProjectedPosition(fTerrainPosition, axis);
    vec2 position = getMaskPosition(normal, axis, projected, size).xz + vec2(0.5F);
    
    ivec2 cell = ivec2(floor(position));
    ivec4 corners = ivec4(getTile(cell + ivec2(-1, -1)), getTile(cell + ivec2(0, -1)), getTile(cell + ivec2(-1, 0)), getTile(cell));
    
    ivec2 pixel = min(ivec2(floor(fract(position) * float(size))), ivec2(size - 1));
    ivec2 sourcePixel = axis == 1 ? (pixel + ivec2(size / 2)) % size : ivec2(floor(fract(projected) * float(size)));
    
    vec2 uv = axis == 1 ? vec2(pixel) / float(size - 1) : fract(position);
    
    float noise = getMaskNoise(axis, pixel, sourcePixel, size, footprint);
    
    vec4 color = getTerrainColor(corners, pixel, sourcePixel, uv, normal, lod, noise, size);
    vec4 texelColor = color * fColor * maps[0].color;
    
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