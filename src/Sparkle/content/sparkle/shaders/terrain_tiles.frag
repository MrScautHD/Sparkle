#version 450

#define MAX_MAPS_COUNT 8

const float PI = 3.14159265359F;
const float TWO_PI = 6.28318530718F;
const float INV_SQRT_TWO = 0.70710678F;
const float EPSILON = 0.000001F;
const uint HASH_X = 374761393u;
const uint HASH_Y = 668265263u;
const uint HASH_MIX = 1274126177u;

struct MaterialMap {
    vec4 color;
    float value;
};

layout(std140, set = 2, binding = 0) uniform MaterialBuffer {
    int renderMode;
    MaterialMap maps[MAX_MAPS_COUNT];
};

layout(std140, set = 3, binding = 0) uniform BrushBuffer {
    int brushType;
    int brushPreviewEnabled;
    float brushRadius;
    float brushPreviewStrength;
    vec4 BrushPreviewColor;
    vec3 brushCenter;
    float brushWireThickness;
};

layout(set = 4, binding = 0) uniform texture2DArray fSources;
layout(set = 4, binding = 1) uniform sampler fSourcesSampler;

layout(set = 5, binding = 0) uniform texture2D fTiles;
layout(set = 5, binding = 1) uniform sampler fTilesSampler;

layout(location = 0) in vec3 fTerrainPosition;
layout(location = 1) in vec4 fColor;
layout(location = 2) in vec3 fSurfacePosition;
layout(location = 3) noperspective in vec3 fBarycentric;

layout(location = 0) out vec4 fFragColor;

/*
 * Returns the terrain material assigned to a logical tile cell.
 *
 * Coordinates outside the tile map use the fallback material stored in
 * material map 1. Tile values are stored as normalized 8-bit values.
 */
int getTile(ivec2 cell) {
    ivec2 size = textureSize(sampler2D(fTiles, fTilesSampler), 0);
    
    // Return fallback material for cells outside the tile texture.
    if (any(lessThan(cell, ivec2(0))) || any(greaterThanEqual(cell, size))) {
        return int(maps[1].value);
    }
    
    // Read the normalized red channel and convert it back to an 8-bit material index.
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
    
    // Prefer horizontal XZ projection when Y is tied with another dominant axis.
    if (direction.y >= largest - 0.001F) {
        return 1;
    }
    
    // Otherwise choose YZ for X-dominant surfaces or XY for Z-dominant surfaces.
    return direction.x >= largest - 0.001F ? 0 : 2;
}

/*
 * Projects a 3D terrain position onto the selected planar axis.
 *
 * Axis 0 uses the YZ plane, axis 1 uses the XZ plane, and axis 2 uses
 * the XY plane.
 */
vec2 getProjectedPosition(vec3 position, int axis) {
    
    // Horizontal surfaces use XZ terrain coordinates.
    if (axis == 1) {
        return position.xz;
    }
    
    // X-dominant surfaces use ZY; Z-dominant surfaces use XY.
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
    
    // Horizontal projection already uses terrain XZ directly.
    if (axis == 1) {
        return fTerrainPosition;
    }
    
    // Calculate the offset from the current projected position to the center of its source texel.
    vec2 offset = (floor(projected * float(size)) + vec2(0.5F)) / float(size) - projected;
    
    // For X-dominant surfaces, solve the X movement needed to stay on the surface plane.
    if (axis == 0) {
        return fTerrainPosition + vec3(-(normal.z * offset.x + normal.y * offset.y) / normal.x, offset.y, offset.x);
    }
    
    // For Z-dominant surfaces, solve the Z movement needed to stay on the surface plane.
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
    
    // Pick the two mip levels surrounding the requested LOD.
    int lowerLevel = int(floor(lod));
    int upperLevel = min(lowerLevel + 1, levelCount - 1);
    
    // Convert the source pixel into normalized UV coordinates at the base resolution.
    vec2 uv = (vec2(sourcePixel) + vec2(0.5F)) / float(size);
    
    // Convert the UV into a texel coordinate for the lower mip level.
    ivec2 lowerSize = textureSize(sampler2DArray(fSources, fSourcesSampler), lowerLevel).xy;
    ivec2 lowerPixel = min(ivec2(uv * vec2(lowerSize)), lowerSize - 1);
    
    // Read the lower mip color.
    vec4 lowerColor = texelFetch(sampler2DArray(fSources, fSourcesSampler), ivec3(lowerPixel, layer), lowerLevel);
    
    // If no blend is needed, return the lower mip sample directly.
    if (upperLevel == lowerLevel || fract(lod) == 0.0F) {
        return lowerColor;
    }
    
    // Convert the UV into a texel coordinate for the upper mip level.
    ivec2 upperSize = textureSize(sampler2DArray(fSources, fSourcesSampler), upperLevel).xy;
    ivec2 upperPixel = min(ivec2(uv * vec2(upperSize)), upperSize - 1);
    
    // Read the upper mip color.
    vec4 upperColor = texelFetch(sampler2DArray(fSources, fSourcesSampler), ivec3(upperPixel, layer), upperLevel);
    
    // Blend between neighboring mip levels using the fractional LOD.
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
    
    // Estimate the rendered surface tangent vectors in screen space.
    vec3 positionDx = dFdx(fSurfacePosition);
    vec3 positionDy = dFdy(fSurfacePosition);
    
    // Cross derivatives to get the geometric surface normal.
    vec3 surfaceNormal = cross(positionDx, positionDy);
    float normalLength = length(surfaceNormal);
    
    // Normalize the normal, falling back to up if the derivative normal is degenerate.
    normal = normalLength > 0.0F ? surfaceNormal / normalLength : vec3(0.0F, 1.0F, 0.0F);
    
    // Select which planar projection should be used for this surface.
    axis = getProjection(normal);
    
    // Source texture size is needed to convert projected derivatives into texel footprint.
    int size = textureSize(sampler2DArray(fSources, fSourcesSampler), 0).x;
    
    // Measure how many texture pixels the current screen pixel covers.
    vec2 texelsDx = getProjectedPosition(positionDx, axis) * float(size);
    vec2 texelsDy = getProjectedPosition(positionDy, axis) * float(size);
    
    // Use the largest derivative length as the conservative material texture footprint.
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
    
    // Horizontal projection uses material pixels directly; steep projections use source pixels.
    uvec2 noisePixel = uvec2((axis == 1 ? pixel : sourcePixel) % (size - 1));
    
    // Generate a stable integer hash from the selected noise pixel.
    uint hash = noisePixel.x * HASH_X + noisePixel.y * HASH_Y;
    hash = (hash ^ (hash >> 13)) * HASH_MIX;
    
    // Convert low hash bits into a small centered noise value.
    float noise = (float(hash & 255u) / 255.0F - 0.5F) * 0.12F;
    
    // Fade noise out when the pixel footprint becomes large to reduce distant shimmer.
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

    // Mark each corner as covered if its material priority is at least the current layer.
    vec4 mask = vec4(greaterThanEqual(corners, ivec4(layer)));
    
    // Interpolate coverage across the north and south edges.
    float north = mix(mask.x, mask.y, uv.x);
    float south = mix(mask.z, mask.w, uv.x);
    
    // Interpolate between north and south, centered around zero for easier thresholding.
    float coverage = mix(north, south, uv.y) - 0.5F;
    
    // Approximate the 2D mask gradient in local terrain space.
    vec3 gradient = vec3(mix(mask.y - mask.x, mask.w - mask.z, uv.y), 0.0F, south - north);
    
    float gradientLength = length(gradient);
    
    // Adjust the coverage width so steep slopes do not stretch the transition band.
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
    
    // Sorting network for four material indices.
    sortPair(layers.x, layers.y);
    sortPair(layers.z, layers.w);
    sortPair(layers.x, layers.z);
    sortPair(layers.y, layers.w);
    sortPair(layers.y, layers.z);
    
    vec4 color = vec4(0.0F);
    int below = layers.x;
    
    // Evaluate each unique material layer in sorted order.
    for (int index = 0; index < 4; index++) {
        int layer = layers[index];
        
        // Skip duplicate material indices to avoid redundant work.
        if (index > 0 && layer == layers[index - 1]) {
            continue;
        }
        
        // Coverage decides whether this layer replaces or blends with the layer below.
        float coverage = getMaterialCoverage(layer, corners, uv, normal) - noise;
        
        // Fully covered region: sample this material directly.
        if (coverage >= 0.0F) {
            color = sampleSource(sourcePixel, size, layer, lod);
            below = layer;
        }
        // Narrow transition band: blend from the lower material to this layer.
        else if (coverage > -0.12F) {
            float transition = smoothstep(0.0F, 1.0F, (coverage + 0.12F) / 0.12F);
            
            vec4 belowColor = sampleSource(sourcePixel, size, below, lod);
            vec4 layerColor = sampleSource(sourcePixel, size, layer, lod);
            
            // Slightly darken the transition to create a soft boundary/shadow.
            float shadow = mix(0.75F, 1.0F, transition);
            color = mix(belowColor, layerColor, transition);
            color.rgb *= shadow;
        }
    }
    
    return color;
}

/*
 * Tests the exact integer tile coordinate used by the CPU painter.
 */
bool isEditedBrushCell(ivec2 cell) {
    if (brushPreviewEnabled == 0 || brushRadius <= 0.0F) {
        return false;
    }
    
    // Fetch the tile-map size so out-of-bounds cells can be rejected.
    ivec2 size = textureSize(sampler2D(fTiles, fTilesSampler), 0);
    
    // Ignore cells outside the editable tile texture.
    if (any(lessThan(cell, ivec2(0))) || any(greaterThanEqual(cell, size))) {
        return false;
    }
    
    // Convert the integer cell coordinate into brush-local X/Z offset.
    float offsetX = float(cell.x) - brushCenter.x;
    float offsetZ = float(cell.y) - brushCenter.z;
    
    // Precompute squared distance values to avoid unnecessary square roots.
    float distanceSquared = offsetX * offsetX + offsetZ * offsetZ;
    float radiusSquared = brushRadius * brushRadius;
    
    switch (brushType) {
        
        // Circle, SoftCircle, Smoothing.
        case 0:
        case 1:
        case 2: {
                return distanceSquared <= radiusSquared;
            }
        
        // Route.
        case 3: {
                float rotatedX = (offsetX + offsetZ) * INV_SQRT_TWO;
                float rotatedZ = (offsetZ - offsetX) * INV_SQRT_TWO;
                
                // The route brush is a square in rotated local space.
                return abs(rotatedX) <= brushRadius && abs(rotatedZ) <= brushRadius;
            }
        
        // Quad.
        case 4: {
                return abs(offsetX) <= brushRadius && abs(offsetZ) <= brushRadius;
            }
        
        // Pentagon.
        case 5: {
                float distance = sqrt(distanceSquared);
                
                // The exact center is always inside the polygon.
                if (distance <= EPSILON) {
                    return true;
                }
                
                // Build a regular pentagon radius for the current polar angle.
                const int sideCount = 5;
                float sectorAngle = TWO_PI / float(sideCount);
                
                // Shift by 90 degrees so the pentagon points in the desired direction.
                float angle = atan(offsetZ, offsetX) - PI * 0.5F;
                
                // Normalize angle into the [0, 2PI) range.
                angle -= floor(angle / TWO_PI) * TWO_PI;
                
                // Convert the angle into the local sector around one polygon side.
                float localAngle = mod(angle, sectorAngle);
                localAngle -= sectorAngle * 0.5F;
                
                // Calculate the polygon edge radius for this direction.
                float polygonRadius = brushRadius * cos(PI / float(sideCount)) / cos(localAngle);
                
                // Calculate the polygon edge radius for this direction.
                return distance <= polygonRadius;
            }
        
        // Noisy.
        case 6: {
                
                // Noisy brush still starts from a circular brush boundary.
                if (distanceSquared > radiusSquared) {
                    return false;
                }
                
                // Generate deterministic per-cell noise from the tile coordinate.
                uint hash = uint(cell.x) * HASH_X + uint(cell.y) * HASH_Y;
                hash = (hash ^ (hash >> 13)) * HASH_MIX;
                hash ^= hash >> 16;
                
                // Convert hash to normalized noise and combine it with radial falloff.
                float noise = float(hash) / float(0xffffffffu);
                float falloff = 1.0F - sqrt(distanceSquared) / brushRadius;
                
                // Keep stronger/noisier cells and reject weaker ones.
                return falloff * mix(0.45F, 1.0F, noise) > 0.35F;
            }
    }
    
    return false;
}

/*
 * Applies a full triangle wire overlay for the exact tile cells touched by the brush.
 */
vec4 applyBrushWirePreview(vec4 color, vec3 terrainPosition) {
    if (brushPreviewEnabled == 0 || brushRadius <= 0.0F) {
        return color;
    }
    
    // Use the current fragment barycentric coordinates and their derivatives to reconstruct the original triangle vertices in terrain space.
    vec2 currentBarycentric = fBarycentric.xy;
    vec2 barycentricDx = dFdx(currentBarycentric);
    vec2 barycentricDy = dFdy(currentBarycentric);
    
    // The determinant tells us whether the barycentric-to-screen transform is usable.
    float determinant = barycentricDx.x * barycentricDy.y - barycentricDx.y * barycentricDy.x;
    
    // Default to the current terrain position if reconstruction fails.
    vec3 vertex0 = terrainPosition;
    vec3 vertex1 = terrainPosition;
    vec3 vertex2 = terrainPosition;
    
    // Reconstruct triangle vertices only when the derivative matrix is valid.
    if (abs(determinant) > EPSILON) {
        mat2 barycentricToScreen = mat2(barycentricDx, barycentricDy);
        
        // Terrain-space derivatives describe how terrain position changes per screen pixel.
        vec3 positionDx = dFdx(terrainPosition);
        vec3 positionDy = dFdy(terrainPosition);
        
        // Calculate screen-space deltas from the current fragment to each triangle vertex.
        vec2 delta0 = inverse(barycentricToScreen) * (vec2(1.0F, 0.0F) - currentBarycentric);
        vec2 delta1 = inverse(barycentricToScreen) * (vec2(0.0F, 1.0F) - currentBarycentric);
        vec2 delta2 = inverse(barycentricToScreen) * (vec2(0.0F, 0.0F) - currentBarycentric);
        
        // Move from the current fragment position to each reconstructed vertex.
        vertex0 = terrainPosition + positionDx * delta0.x + positionDy * delta0.y;
        vertex1 = terrainPosition + positionDx * delta1.x + positionDy * delta1.y;
        vertex2 = terrainPosition + positionDx * delta2.x + positionDy * delta2.y;
    }
    
    // Use the triangle center to decide which tile cell this triangle represents.
    ivec2 triangleCell = ivec2(floor(((vertex0 + vertex1 + vertex2) / 3.0F).xz));
    
    // If the triangle's owner cell is not touched by the brush, do not draw preview wire.
    if (!isEditedBrushCell(triangleCell)) {
        return color;
    }
    
    // fwidth gives stable screen-space thickness for each barycentric edge.
    vec3 derivative = fwidth(fBarycentric);
    
    // Base thickness is clamped so the wire never becomes invisible.
    float baseThickness = max(0.01F, brushWireThickness);

    // Outer edges are thicker because they are drawn by only one selected triangle.
    float outerThickness = baseThickness * 2.0F;
    
    // Each barycentric component represents the edge opposite that vertex.
    vec2 edgeMid0 = (vertex1.xz + vertex2.xz) * 0.5F;
    vec2 edgeMid1 = (vertex0.xz + vertex2.xz) * 0.5F;
    vec2 edgeMid2 = (vertex0.xz + vertex1.xz) * 0.5F;
    
    // Direction from the opposite vertex through the edge midpoint points outside the triangle.
    vec2 outsideDirection0 = edgeMid0 - vertex0.xz;
    vec2 outsideDirection1 = edgeMid1 - vertex1.xz;
    vec2 outsideDirection2 = edgeMid2 - vertex2.xz;
    
    // Assume edges are outer until a selected neighbor cell is found outside them.
    bool edge0Outer = true;
    bool edge1Outer = true;
    bool edge2Outer = true;

    // Length checks avoid normalizing zero-length directions.
    float directionLength0 = length(outsideDirection0);
    float directionLength1 = length(outsideDirection1);
    float directionLength2 = length(outsideDirection2);
    
    // Step just beyond edge 0 and test whether the outside cell is also selected.
    if (directionLength0 > EPSILON) {
        outsideDirection0 /= directionLength0;
        ivec2 outsideCell0 = ivec2(floor(edgeMid0 + outsideDirection0 * 0.05F));
        edge0Outer = !isEditedBrushCell(outsideCell0);
    }
    
    // Step just beyond edge 1 and test whether the outside cell is also selected.
    if (directionLength1 > EPSILON) {
        outsideDirection1 /= directionLength1;
        ivec2 outsideCell1 = ivec2(floor(edgeMid1 + outsideDirection1 * 0.05F));
        edge1Outer = !isEditedBrushCell(outsideCell1);
    }
    
    // Step just beyond edge 2 and test whether the outside cell is also selected.
    if (directionLength2 > EPSILON) {
        outsideDirection2 /= directionLength2;
        ivec2 outsideCell2 = ivec2(floor(edgeMid2 + outsideDirection2 * 0.05F));
        edge2Outer = !isEditedBrushCell(outsideCell2);
    }
    
    // Pick thicker lines for outline edges and thinner lines for internal selected edges.
    float thickness0 = edge0Outer ? outerThickness : baseThickness;
    float thickness1 = edge1Outer ? outerThickness : baseThickness;
    float thickness2 = edge2Outer ? outerThickness : baseThickness;
    
    // Convert barycentric distance into approximate screen-space distance to each edge.
    float distanceToEdge0 = fBarycentric.x / max(derivative.x, EPSILON);
    float distanceToEdge1 = fBarycentric.y / max(derivative.y, EPSILON);
    float distanceToEdge2 = fBarycentric.z / max(derivative.z, EPSILON);
    
    // Fade each edge smoothly so the wire is anti-aliased.
    float wire0 = 1.0F - smoothstep(thickness0, thickness0 + 1.0F, distanceToEdge0);
    float wire1 = 1.0F - smoothstep(thickness1, thickness1 + 1.0F, distanceToEdge1);
    float wire2 = 1.0F - smoothstep(thickness2, thickness2 + 1.0F, distanceToEdge2);
    
    // Combine all three triangle edges into one wire mask.
    float wire = max(max(wire0, wire1), wire2);
    
    // If this fragment is not close enough to any edge, keep the original color.
    if (wire <= 0.001F) {
        return color;
    }
    
    // Apply preview color using brush alpha, wire coverage, and preview strength.
    float alpha = wire * BrushPreviewColor.a * clamp(brushPreviewStrength, 0.0F, 1.0F);
    color.rgb = mix(color.rgb, BrushPreviewColor.rgb, alpha);
    
    return color;
}

void main() {
    
    // Calculate derivative-based surface data for material projection and LOD selection.
    vec3 normal;
    int axis;
    float footprint;
    getSurfaceData(normal, axis, footprint);
    
    // Source material textures are square, so the X dimension is used as the base size.
    int size = textureSize(sampler2DArray(fSources, fSourcesSampler), 0).x;
    
    // Convert screen-space texture footprint into source texture LOD.
    float lod = getSourceLod(footprint);
    
    // Project the current terrain position onto the best planar axis.
    vec2 projected = getProjectedPosition(fTerrainPosition, axis);
    
    // Calculate the adjusted mask position and shift by half a tile so four-corner sampling lines up with tile centers.
    vec2 position = getMaskPosition(normal, axis, projected, size).xz + vec2(0.5F);
    
    // The base tile cell is the integer part of the adjusted mask position.
    ivec2 cell = ivec2(floor(position));
    
    // Fetch the four material corners surrounding the current mask position.
    ivec4 corners = ivec4(getTile(cell + ivec2(-1, -1)), getTile(cell + ivec2(0, -1)), getTile(cell + ivec2(-1, 0)), getTile(cell));
    
    // Convert fractional tile position into a source pixel inside the material texture.
    ivec2 pixel = min(ivec2(floor(fract(position) * float(size))), ivec2(size - 1));
    
    // Horizontal projection offsets pixels by half the texture size to align material sampling.
    ivec2 sourcePixel = axis == 1 ? (pixel + ivec2(size / 2)) % size : ivec2(floor(fract(projected) * float(size)));
    
    // Horizontal projection uses pixel-aligned UVs; steep projections use the fractional adjusted mask position.
    vec2 uv = axis == 1 ? vec2(pixel) / float(size - 1) : fract(position);
    
    // Add small deterministic noise to soften material transitions.
    float noise = getMaskNoise(axis, pixel, sourcePixel, size, footprint);
    
    // Resolve the final terrain material color from the surrounding tile corners.
    vec4 color = getTerrainColor(corners, pixel, sourcePixel, uv, normal, lod, noise, size);
    
    // Apply vertex color and global material tint.
    vec4 texelColor = color * fColor * maps[0].color;
    
    // Overlay the brush wire preview if enabled.
    texelColor = applyBrushWirePreview(texelColor, fTerrainPosition);
    
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