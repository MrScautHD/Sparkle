using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.window;

namespace Sparkle.csharp.entity; 

public class Cam2D : Entity {
    
    private Camera2D _camera2D;
    private float AMPLIT = 0.1f; // don't go above 80 TODO: make a setting bar to change the value between 1(better) and 50(worst)
    
    public Vector2 Target;
    public CameraMode Mode;
    public float MaxZoom;
    
    
    public Cam2D(Vector2 position, Vector2 target, CameraMode mode) : base(new Vector3(position.X, position.Y, 0)) {
        this._camera2D = new Camera2D();
        this.Target = target;
        this.Mode = mode;
    }

    public new Vector2 Position {
        get => this._camera2D.target;
        set => this._camera2D.target = value;
    }
    
    public new float Rotation {
        get => this._camera2D.rotation;
        set => this._camera2D.rotation = value;
    }
    
    public Vector2 Offset {
        get => this._camera2D.offset;
        set => this._camera2D.offset = value;
    }
    
    public float Zoom {
        get => this._camera2D.zoom;
        set {
            if (value < this.MaxZoom) { //TODO CHECK THE ZOOM AGAIN!
                this._camera2D.zoom = value;
            }
        }
    }

    protected internal override void Update() {
        base.Update();
        this.Zoom -= Input.GetMouseWheelMove() * 0.13F;
        
        switch (this.Mode) {
            case CameraMode.Normal:
                this.NormalMovement(Window.GetScreenWidth(), Window.GetScreenHeight());
                break;
            
            case CameraMode.Smooth:
                this.SmoothMovement((int) (Window.GetScreenWidth()), (int) (Window.GetScreenHeight()));
                break;
            
            case CameraMode.Smoother:
                this.SmootherMovement((int) (Window.GetScreenWidth()), (int) (Window.GetScreenHeight()), AMPLIT);
                break;
            
            default: // for people stupid like Lucy
                Logger.Fatal("BRO YOU FORGOT TO ADD THE CAM SETTINGS");
                break;
        }
    }

    protected void NormalMovement(int width, int height) {
        this.Position = this.Target;
        this.Offset = new Vector2(width / 2.0F, height / 2.0F);
    }
    
    protected void SmoothMovement(int width, int height) {
        float minSpeed = 30;
        float minEffectLength = 10;
        float fractionSpeed = 0.8f;

        this.Offset = new Vector2(width / 2.0F, height / 2.0F); // centering
        Vector2 diff = this.Target - this.Position;
        float length = diff.Length(); // the norm 2

        if (length > minEffectLength) {
            float speed = Math.Max(fractionSpeed * length, minSpeed);
            this.Position = Vector2.Add(this.Position, Vector2.Multiply(diff, speed * Time.Delta / length));
        }
    }

    protected void SmootherMovement(int width, int height, float ampl) {
        float minSpeed = 30;
        // float minEffectLength = 0;
        float fractionSpeed = 0.8f;

        this.Offset = new Vector2(width / 2.0F, height / 2.0F); // centering
        Vector2 diff = this.Target - this.Position;
        float length = diff.Length(); // the norm 2

        if (length > Math.Sin(Time.Delta) * ampl) {
            float speed = Math.Max(fractionSpeed * length, minSpeed);
            this.Position = Vector2.Add(this.Position, Vector2.Multiply(diff, speed * Time.Delta / length));
        }
    }
    
    public enum CameraMode {
        Normal,
        Smooth,
        Smoother,
        Custom
    }
    
    public void BeginMode2D() {
        Raylib.BeginMode2D(this._camera2D);
    }

    public void EndMode2D() {
        Raylib.EndMode2D();
    }
}