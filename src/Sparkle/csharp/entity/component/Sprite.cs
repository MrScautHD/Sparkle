using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.scene;

namespace Sparkle.csharp.entity.component; 

public class Sprite : Component {

    public Texture2D Texture;
    public Vector2 Size;
    public Color Color;
    public float Rotation;
    public bool FlipSprite;
    
    public Sprite(Texture2D texture, Vector2? size, Color? color, float rotation = 0, bool flipSprite = false) {
        this.Texture = texture;
        this.Size = size ?? new Vector2(texture.width, texture.height);
        this.Color = color ?? Color.WHITE;
        this.Rotation = rotation;
        this.FlipSprite = flipSprite;
    }

    protected internal override void Draw() {
        base.Draw();
        
        SceneManager.MainCam2D!.BeginMode2D();
        
        Rectangle source = new Rectangle(0, 0, this.Texture.width, this.Texture.height);
        Rectangle dest = new Rectangle(this.Entity.Position.X + (this.Size.X / 2), this.Entity.Position.Y + (this.Size.Y / 2), this.Size.X, this.Size.Y);
        Vector2 origin = new Vector2(dest.width / 2, dest.height / 2);
        TextureHelper.DrawPro(this.Texture, source, dest, origin, this.Rotation, this.Color);
        
        SceneManager.MainCam2D.EndMode2D();
    }

    protected override void Dispose(bool disposing) { }
}