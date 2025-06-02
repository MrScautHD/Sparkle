using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class TextureButtonData {

    public Texture2D Texture;
    public float LayerDepth;
    public Rectangle SourceRect;
    public Vector2 Scale;
    public Color Color;
    public Color HoverColor;
    public SpriteFlip Flip;

    public TextureButtonData(Texture2D texture, float layerDepth = 0.5F, Rectangle? sourceRect = null, Vector2? scale = null, Color? color = null, Color? hoverColor = null, SpriteFlip flip = SpriteFlip.None) {
        this.Texture = texture;
        this.LayerDepth = layerDepth;
        this.SourceRect = sourceRect ?? new Rectangle(0, 0, (int) texture.Width, (int) texture.Height);
        this.Scale = scale ?? Vector2.One;
        this.Color = color ?? Color.White;
        this.HoverColor = hoverColor ?? this.Color;
        this.Flip = flip;
    }
}