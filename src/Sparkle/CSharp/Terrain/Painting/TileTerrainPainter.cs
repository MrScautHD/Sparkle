using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Images;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Effects;
using Veldrith;

namespace Sparkle.CSharp.Terrain.Painting;

public class TileTerrainPainter : Disposable, ITerrainPainter {
    
    /// <summary>
    /// The graphics device used for rendering the terrain.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// The material used for rendering the terrain.
    /// </summary>
    public Material Material { get; private set; }
    
    /// <summary>
    /// The terrain tile width in cells.
    /// </summary>
    public int Width { get; private set; }
    
    /// <summary>
    /// The terrain tile depth in cells.
    /// </summary>
    public int Depth { get; private set; }
    
    /// <summary>
    /// The width and height of each source terrain layer texture.
    /// </summary>
    public int SourceTextureSize { get; private set; }
    
    /// <summary>
    /// The maximum number of terrain material layers this painter can store.
    /// </summary>
    public int MaxLayerCount { get; private set; }
    
    /// <summary>
    /// The current number of uploaded terrain material layers.
    /// </summary>
    public int LayerCount { get; private set; }
    
    /// <summary>
    /// GPU-side source texture array.
    /// This should be bound to the terrain tile shader as fSources.
    /// </summary>
    public Texture SourceTextureArray { get; private set; }
    
    /// <summary>
    /// GPU-side tile layer texture.
    /// This should be bound to the terrain tile shader as fTiles.
    /// </summary>
    public Texture TileTexture { get; private set; }
    
    /// <summary>
    /// CPU-side tile layer data.
    /// Each byte stores the selected terrain material layer for one tile cell.
    /// </summary>
    private byte[] _tiles;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TileTerrainPainter"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering the terrain.</param>
    /// <param name="material">The terrain rendering material.</param>
    /// <param name="width">The terrain tile width in cells.</param>
    /// <param name="depth">The terrain tile depth in cells.</param>
    /// <param name="sourceTextureSize">The width and height of every terrain source layer texture.</param>
    /// <param name="maxLayerCount">The maximum number of terrain source layers.</param>
    /// <param name="defaultLayer">The layer used to initialize the tile map.</param>
    public TileTerrainPainter(GraphicsDevice graphicsDevice, Material material, int width, int depth, int sourceTextureSize, int maxLayerCount, byte defaultLayer = 0) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(depth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceTextureSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLayerCount);
        
        if (maxLayerCount > byte.MaxValue + 1) {
            throw new ArgumentOutOfRangeException(nameof(maxLayerCount), $"A tile layer is stored as one byte, so max layer count cannot be bigger than {byte.MaxValue + 1}.");
        }
        
        this.GraphicsDevice = graphicsDevice;
        this.Material = material;
        this.Width = width;
        this.Depth = depth;
        this.SourceTextureSize = sourceTextureSize;
        this.MaxLayerCount = maxLayerCount;
        this.LayerCount = 0;
        this._tiles = new byte[width * depth];
        
        // Create source texture array.
        this.SourceTextureArray = graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            (uint) sourceTextureSize,
            (uint) sourceTextureSize,
            1,
            (uint) Math.Max(2, maxLayerCount),
            PixelFormat.R8G8B8A8UNorm,
            TextureUsage.Sampled
        ));
        
        // Create tile texture.
        this.TileTexture = graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            (uint) width,
            (uint) depth,
            1,
            1,
            PixelFormat.R8UNorm,
            TextureUsage.Sampled
        ));
        
        // Assign textures to the effect.
        if (material.Effect is TileTerrainEffect tileTerrainEffect) {
            tileTerrainEffect.SetSourcesTexture(material, this.SourceTextureArray);
            tileTerrainEffect.SetTilesTexture(material, this.TileTexture);
        }
        
        // Setup the tile map with the default layer.
        this.Clear(defaultLayer);
    }
    
    /// <summary>
    /// Adds a new terrain material layer using an existing texture.
    /// </summary>
    /// <param name="texture">The texture to copy into the source texture array.</param>
    /// <returns>The layer index that can be used for painting.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="LayerCount"/> already reached <see cref="MaxLayerCount"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="texture"/> does not match <see cref="SourceTextureSize"/>.</exception>
    public int AddLayer(Texture2D texture) {
        if (this.LayerCount >= this.MaxLayerCount) {
            throw new InvalidOperationException($"Cannot add more than {this.MaxLayerCount} terrain layers.");
        }
        
        if (texture.Width != this.SourceTextureSize || texture.Height != this.SourceTextureSize) {
            throw new ArgumentException($"Layer texture must be {this.SourceTextureSize}x{this.SourceTextureSize}.", nameof(texture));
        }
        
        int layer = this.LayerCount;
        this.SetLayer(layer, texture);
        this.LayerCount++;
        
        return layer;
    }
    
    /// <summary>
    /// Adds a new terrain material layer using an image.
    /// </summary>
    /// <param name="image">The image to upload into the source texture array.</param>
    /// <returns>The layer index that can be used for painting.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="LayerCount"/> already reached <see cref="MaxLayerCount"/>.</exception>
    public int AddLayer(Image image) {
        if (this.LayerCount >= this.MaxLayerCount) {
            throw new InvalidOperationException($"Cannot add more than {this.MaxLayerCount} terrain layers.");
        }
        
        int layer = this.LayerCount;
        this.SetLayer(layer, image);
        this.LayerCount++;
        
        return layer;
    }
    
    /// <summary>
    /// Replaces an existing terrain material layer with an existing texture.
    /// </summary>
    /// <param name="layer">The layer index to replace.</param>
    /// <param name="texture">The texture to copy into the source texture array.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="layer"/> is outside the valid layer range.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="texture"/> does not match <see cref="SourceTextureSize"/>.</exception>
    public void SetLayer(int layer, Texture2D texture) {
        if (layer < 0 || layer >= this.MaxLayerCount) {
            throw new ArgumentOutOfRangeException(nameof(layer));
        }
        
        if (texture.Width != this.SourceTextureSize || texture.Height != this.SourceTextureSize) {
            throw new ArgumentException($"Layer texture must be {this.SourceTextureSize}x{this.SourceTextureSize}.", nameof(texture));
        }
        
        // Copy the texture directly on the GPU into the target array slice.
        using CommandList commandList = this.GraphicsDevice.ResourceFactory.CreateCommandList();
        
        commandList.Begin();
        commandList.CopyTexture(texture.DeviceTexture, 0, 0, 0, 0, 0, this.SourceTextureArray, 0, 0, 0, 0, (uint) layer, (uint) this.SourceTextureSize, (uint) this.SourceTextureSize, 1, 1);
        commandList.End();
        
        this.GraphicsDevice.SubmitCommands(commandList);
    }
    
    /// <summary>
    /// Replaces an existing terrain material layer with an image. The image data must be tightly packed RGBA8 pixel data.
    /// </summary>
    /// <param name="layer">The layer index to replace.</param>
    /// <param name="image">The source image.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="layer"/> is outside the valid layer range.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="image"/> does not match <see cref="SourceTextureSize"/>, or its data length does not match the expected RGBA8 byte count.</exception>
    public unsafe void SetLayer(int layer, Image image) {
        if (layer < 0 || layer >= this.MaxLayerCount) {
            throw new ArgumentOutOfRangeException(nameof(layer));
        }
        
        if (image.Width != this.SourceTextureSize || image.Height != this.SourceTextureSize) {
            throw new ArgumentException($"Layer image must be {this.SourceTextureSize}x{this.SourceTextureSize}.", nameof(image));
        }
        
        int expectedLength = this.SourceTextureSize * this.SourceTextureSize * 4;
        
        if (image.Data.Length != expectedLength) {
            throw new ArgumentException($"Layer image data must be exactly {expectedLength} bytes.", nameof(image));
        }
        
        // Upload the pixel data directly into the target array slice.
        fixed (byte* pixelPointer = image.Data) {
            this.GraphicsDevice.UpdateTexture(this.SourceTextureArray, (nint) pixelPointer, (uint) image.Data.Length, 0, 0, 0, (uint) this.SourceTextureSize, (uint) this.SourceTextureSize, 1, 0, (uint) layer);
        }
    }
    
    /// <summary>
    /// Gets the terrain material layer stored at the given tile coordinate.
    /// </summary>
    /// <param name="x">The tile X coordinate.</param>
    /// <param name="z">The tile Z coordinate.</param>
    /// <returns>The stored tile layer, or 0 when outside the tile map.</returns>
    public byte GetTile(int x, int z) {
        if (x < 0 || x >= this.Width || z < 0 || z >= this.Depth) {
            return 0;
        }
        
        return this._tiles[this.GetTileIndex(x, z)];
    }
    
    /// <summary>
    /// Sets the terrain material layer at the given tile coordinate.
    /// </summary>
    /// <param name="x">The tile X coordinate.</param>
    /// <param name="z">The tile Z coordinate.</param>
    /// <param name="layer">The new material layer.</param>
    public void SetTile(int x, int z, byte layer) {
        if (x < 0 || x >= this.Width || z < 0 || z >= this.Depth) {
            return;
        }
        
        if (layer >= this.LayerCount) {
            return;
        }
        
        int index = this.GetTileIndex(x, z);
        
        // Skip the upload if nothing changed.
        if (this._tiles[index] == layer) {
            return;
        }
        
        this._tiles[index] = layer;
        this.UploadTileRegion(x, z, 1, 1);
    }
    
    /// <summary>
    /// Clears all tiles to the given material layer.
    /// </summary>
    /// <param name="layer">The material layer to fill the tile map with.</param>
    public void Clear(byte layer = 0) {
        Array.Fill(this._tiles, layer);
        this.UploadTileRegion(0, 0, this.Width, this.Depth);
    }
    
    /// <summary>
    /// Applies a brush to the terrain texture/layer data.
    /// </summary>
    /// <param name="center">The terrain-space center of the brush.</param>
    /// <param name="radius">The radius of the brush in terrain units.</param>
    /// <param name="strength">The paint strength. Positive values paint, negative values may erase depending on implementation.</param>
    /// <param name="layer">The texture/material layer index.</param>
    /// <param name="brushType">The brush shape/falloff.</param>
    /// <returns><c>true</c> if any paint data was modified; otherwise, <c>false</c>.</returns>
    public bool ApplyTextureLayerBrush(Vector3 center, float radius, float strength, int layer, TerrainBrushType brushType) {
        if (radius <= 0.0F || layer < 0 || layer > byte.MaxValue || layer >= this.LayerCount) {
            return false;
        }
        
        int minimumX = Math.Max(0, (int) MathF.Floor(center.X - radius));
        int maximumX = Math.Min(this.Width - 1, (int) MathF.Ceiling(center.X + radius));
        int minimumZ = Math.Max(0, (int) MathF.Floor(center.Z - radius));
        int maximumZ = Math.Min(this.Depth - 1, (int) MathF.Ceiling(center.Z + radius));
        
        float radiusSquared = radius * radius;
        byte newLayer = strength >= 0.0F ? (byte) layer : (byte) 0;
        
        bool changed = false;
        int dirtyMinimumX = this.Width;
        int dirtyMinimumZ = this.Depth;
        int dirtyMaximumX = -1;
        int dirtyMaximumZ = -1;
        
        for (int x = minimumX; x <= maximumX; x++) {
            for (int z = minimumZ; z <= maximumZ; z++) {
                float offsetX = x - center.X;
                float offsetZ = z - center.Z;
                float distanceSquared = offsetX * offsetX + offsetZ * offsetZ;
                
                bool isInsideBrush = false;
                
                switch (brushType) {
                    case TerrainBrushType.Circle:
                    case TerrainBrushType.SoftCircle: {
                        isInsideBrush = distanceSquared <= radiusSquared;
                        break;
                    }
                    
                    case TerrainBrushType.Quad: {
                        isInsideBrush = MathF.Abs(offsetX) <= radius && MathF.Abs(offsetZ) <= radius;
                        break;
                    }
                    
                    case TerrainBrushType.Route: {
                        float rotatedX = (offsetX + offsetZ) * 0.70710678F;
                        float rotatedZ = (offsetZ - offsetX) * 0.70710678F;
                        
                        isInsideBrush = MathF.Abs(rotatedX) <= radius && MathF.Abs(rotatedZ) <= radius;
                        break;
                    }
                    
                    case TerrainBrushType.Pentagon: {
                        float distance = MathF.Sqrt(offsetX * offsetX + offsetZ * offsetZ);
                        
                        if (distance <= float.Epsilon) {
                            isInsideBrush = true;
                        }
                        else {
                            const int sideCount = 5;
                            float sectorAngle = MathF.Tau / sideCount;
                            float angle = MathF.Atan2(offsetZ, offsetX) - MathF.PI * 0.5F;
                            
                            angle -= MathF.Floor(angle / MathF.Tau) * MathF.Tau;
                            
                            float localAngle = angle % sectorAngle;
                            localAngle -= sectorAngle * 0.5F;
                            
                            float polygonRadius = radius * MathF.Cos(MathF.PI / sideCount) / MathF.Cos(localAngle);
                            
                            isInsideBrush = distance <= polygonRadius;
                        }
                        
                        break;
                    }
                    
                    case TerrainBrushType.Noisy: {
                        if (distanceSquared <= radiusSquared) {
                            uint hash = (uint)x * 374761393U + (uint)z * 668265263U;
                            hash = (hash ^ (hash >> 13)) * 1274126177U;
                            hash ^= hash >> 16;
                            
                            float noise = hash / (float)uint.MaxValue;
                            float falloff = 1.0F - MathF.Sqrt(distanceSquared) / radius;
                            
                            isInsideBrush = falloff * float.Lerp(0.45F, 1.0F, noise) > 0.35F;
                        }
                        
                        break;
                    }
                }
                
                if (!isInsideBrush) {
                    continue;
                }
                
                int index = this.GetTileIndex(x, z);
                
                if (this._tiles[index] == newLayer) {
                    continue;
                }
                
                this._tiles[index] = newLayer;
                changed = true;
                
                dirtyMinimumX = Math.Min(dirtyMinimumX, x);
                dirtyMinimumZ = Math.Min(dirtyMinimumZ, z);
                dirtyMaximumX = Math.Max(dirtyMaximumX, x);
                dirtyMaximumZ = Math.Max(dirtyMaximumZ, z);
            }
        }
        
        if (changed) {
            this.UploadTileRegion(dirtyMinimumX, dirtyMinimumZ, dirtyMaximumX - dirtyMinimumX + 1, dirtyMaximumZ - dirtyMinimumZ + 1);
        }
        
        return changed;
    }
    
    /// <summary>
    /// Creates and returns a copy of the CPU-side tile layer data.
    /// </summary>
    /// <returns>A copy of the CPU tile layer data.</returns>
    public byte[] CopyTileData() {
        byte[] copy = new byte[this._tiles.Length];
        Array.Copy(this._tiles, copy, this._tiles.Length);
        return copy;
    }
    
    /// <summary>
    /// Converts a tile coordinate to a flat row-major array index.
    /// </summary>
    /// <param name="x">The tile X coordinate.</param>
    /// <param name="z">The tile Z coordinate.</param>
    /// <returns>The flat array index for the given tile coordinate.</returns>
    private int GetTileIndex(int x, int z) {
        return z * this.Width + x;
    }
    
    /// <summary>
    /// Uploads a rectangular region of the CPU tile map into the GPU tile texture.
    /// </summary>
    /// <param name="x">The X coordinate of the region's origin.</param>
    /// <param name="z">The Z coordinate of the region's origin.</param>
    /// <param name="width">The width of the region in tiles.</param>
    /// <param name="depth">The depth of the region in tiles.</param>
    private unsafe void UploadTileRegion(int x, int z, int width, int depth) {
        if (width <= 0 || depth <= 0) {
            return;
        }
        
        // The whole map is dirty, upload the tile array directly.
        if (x == 0 && z == 0 && width == this.Width && depth == this.Depth) {
            fixed (byte* tilePointer = this._tiles) {
                this.GraphicsDevice.UpdateTexture(this.TileTexture, (nint) tilePointer, (uint) this._tiles.Length, 0, 0, 0, (uint) this.Width, (uint) this.Depth, 1, 0, 0);
            }
            
            return;
        }
        
        // Otherwise, pack just the dirty rectangle row by row into a temporary buffer.
        byte[] uploadData = new byte[width * depth];
        
        for (int row = 0; row < depth; row++) {
            int sourceOffset = this.GetTileIndex(x, z + row);
            int destinationOffset = row * width;
            
            Array.Copy(this._tiles, sourceOffset, uploadData, destinationOffset, width);
        }
        
        // Upload the packed region at its correct offset.
        fixed (byte* uploadPointer = uploadData) {
            this.GraphicsDevice.UpdateTexture(this.TileTexture, (nint) uploadPointer, (uint) uploadData.Length, (uint) x, (uint) z, 0, (uint) width, (uint) depth, 1, 0, 0);
        }
    }
    
    /// <summary>
    /// Uploads the complete CPU tile map to the GPU tile texture.
    /// </summary>
    public void UploadAllTiles() {
        this.UploadTileRegion(0, 0, this.Width, this.Depth);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.SourceTextureArray.Dispose();
            this.TileTexture.Dispose();
            this._tiles = [];
        }
    }
}