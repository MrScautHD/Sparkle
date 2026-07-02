using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.GUI.Batching;
using Sparkle.CSharp.GUI.Elements.Data;

namespace Sparkle.CSharp.GUI.Elements;

public class RectangleSlideBarElement : GuiElement {
    
    /// <summary>
    /// The data used to render the slide bar.
    /// </summary>
    public RectangleSlideBarData Data { get; private set; }
    
    /// <summary>
    /// The minimum allowable value for the slider bar, defining the lower limit of the slider's range.
    /// </summary>
    public float MinValue;
    
    /// <summary>
    /// The maximum allowable value for the slider bar, defining the upper limit of the slider's range.
    /// </summary>
    public float MaxValue;
    
    /// <summary>
    /// The current value of the slider bar, clamped between the minimum and maximum values.
    /// </summary>
    public float Value {
        get;
        set {
            float clampedValue = Math.Clamp(value, this.MinValue, this.MaxValue);
            field = this.WholeNumbers ? MathF.Round(clampedValue) : clampedValue;
            this.ValueUpdated?.Invoke(field);
        }
    }
    
    /// <summary>
    /// The slider value should be rounded to the nearest whole number.
    /// </summary>
    public bool WholeNumbers;
    
    /// <summary>
    /// An event that triggers when the slider value is updated.
    /// The new value of the slider is passed as a parameter to the attached event handlers.
    /// </summary>
    public event Action<float>? ValueUpdated;
    
    /// <summary>
    /// The user is currently dragging the slider.
    /// </summary>
    private bool _isDragging;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleSlideBarElement"/> class.
    /// </summary>
    /// <param name="data">The visual and styling data used to render the slide bar.</param>
    /// <param name="renderOrder">The order in which the element is rendered, relative to others.</param>
    /// <param name="anchor">The anchor point used to position the element.</param>
    /// <param name="offset">The offset from the anchor position.</param>
    /// <param name="size">The size of the slide bar.</param>
    /// <param name="minValue">The minimum value of the slide bar.</param>
    /// <param name="maxValue">The maximum value of the slide bar.</param>
    /// <param name="value">The initial value of the slide bar.</param>
    /// <param name="wholeNumbers">Indicates whether the value should be restricted to whole numbers.</param>
    /// <param name="scale">Optional scale applied to the element.</param>
    /// <param name="origin">Optional origin point used for rotation and alignment.</param>
    /// <param name="rotation">The rotation of the element in degrees.</param>
    /// <param name="clickFunc">Optional function invoked when the element is clicked.</param>
    public RectangleSlideBarElement(
        RectangleSlideBarData data,
        int renderOrder,
        Anchor anchor,
        Vector2 offset,
        Vector2 size,
        float minValue,
        float maxValue,
        float value = 0.0F,
        bool wholeNumbers = false,
        Vector2? scale = null,
        Vector2? origin = null,
        float rotation = 0.0F,
        Func<GuiElement, bool>? clickFunc = null) : base(renderOrder, anchor, offset, size, scale, origin, rotation, clickFunc) {
        this.Data = data;
        this.MinValue = minValue;
        this.MaxValue = maxValue;
        this.Value = value;
        this.WholeNumbers = wholeNumbers;
    }
    
    /// <summary>
    /// Updates the visual state and properties of the rectangle slide bar element based on time delta and user interactions.
    /// </summary>
    /// <param name="delta">The elapsed time, in seconds, since the last update, used for time-based calculations or animations.</param>
    /// <param name="interactionHandled">A reference to a boolean tracking whether interaction has already been handled by another element.</param>
    protected internal override void Update(double delta, ref bool interactionHandled) {
        base.Update(delta, ref interactionHandled);
        
        if (this.Interactable) {
            if (this.IsClicked) {
                this._isDragging = true;
            }
            
            if (this._isDragging) {
                if (Input.IsMouseButtonDown(MouseButton.Left)) {
                    
                    // Transform mouse position to local space (considering rotation and scale).
                    Matrix4x4 rotation = Matrix4x4.CreateRotationZ(float.DegreesToRadians(-this.Rotation));
                    Vector2 localClickPos = Vector2.Transform(Input.GetMousePosition() - this.Position, rotation) + this.Origin * this.Scale * this.Gui.ScaleFactor;
                    
                    float percent;
                    
                    // Check if the slider is there because then the graping need to be synced with his size.
                    if (this.Data.SliderSize.HasValue) {
                        float sliderWidth = this.Data.SliderSize.Value.X;
                        float usableWidth = this.Size.X - sliderWidth;
                        
                        percent = Math.Clamp((localClickPos.X / (this.Scale.X * this.Gui.ScaleFactor) - sliderWidth / 2.0F) / usableWidth, 0.0F, 1.0F);
                    }
                    else {
                        percent = Math.Clamp(localClickPos.X / (this.Scale.X * this.Gui.ScaleFactor) / this.Size.X, 0.0F, 1.0F);
                    }
                    
                    // Update the value based on the percentage.
                    this.Value = this.MinValue + (this.MaxValue - this.MinValue) * percent;
                }
                else {
                    this._isDragging = false;
                }
            }
        }
        else {
            this._isDragging = false;
        }
    }
    
    /// <summary>
    /// Submits the draw commands required to render the GUI element using the appropriate visual state and rendering mode.
    /// </summary>
    /// <param name="renderQueue">The render queue that collects and batches draw commands for later execution.</param>
    protected internal override void Draw(GuiRenderQueue renderQueue) {
        base.Draw(renderQueue);
        
        PrimitiveGuiRenderState renderState = new PrimitiveGuiRenderState(this.Data.Effect, this.Data.BlendState);
        
        Vector2 scaledOrigin = this.Origin * this.Scale * this.Gui.ScaleFactor;
        
        // Draw bar.
        Color barColor = this.IsHovered ? this.Data.BarHoverColor : this.Data.BarColor;
        
        if (!this.Interactable) {
            barColor = this.Data.DisabledBarColor;
        }
        
        // Draw bar outline.
        if (this.Data.BarOutlineThickness > 0.0F) {
            Color outlineColor = this.IsHovered ? this.Data.BarOutlineHoverColor : this.Data.BarOutlineColor;
            
            if (!this.Interactable) {
                outlineColor = this.Data.DisabledBarOutlineColor;
            }
            
            float outlineThickness = this.Data.BarOutlineThickness * this.Gui.ScaleFactor;
            
            renderQueue.SubmitPrimitive(0, static (batch, state) => {
                
                // Draw bar.
                batch.DrawFilledRectangle(state.Rectangle, state.Origin, state.Rotation, 0.5F, state.Color);
                
                // Draw bar outline.
                batch.DrawEmptyRectangle(state.Rectangle, state.Thickness, state.Origin, state.Rotation, 0.5F, state.OutlineColor);
            }, (Rectangle: new RectangleF(this.Position.X, this.Position.Y, this.ScaledSize.X, this.ScaledSize.Y), Thickness: outlineThickness, Origin: scaledOrigin, Rotation: this.Rotation, Color: barColor, OutlineColor: outlineColor), renderState);
        }
        else {
            renderQueue.SubmitPrimitive(0, static (batch, state) => {
                batch.DrawFilledRectangle(state.Rectangle, state.Origin, state.Rotation, 0.5F, state.Color);
            }, (Rectangle: new RectangleF(this.Position.X, this.Position.Y, this.ScaledSize.X, this.ScaledSize.Y), Origin: scaledOrigin, Rotation: this.Rotation, Color: barColor), renderState);
        }
        
        // Calculate progress.
        float value = Math.Clamp(this.Value, this.MinValue, this.MaxValue);
        float percent = (value - this.MinValue) / (this.MaxValue - this.MinValue);
        
        if (percent > 0) {
            float fillWidth;
            
            // Check if the slider is there because then the progress bar needs to be synced with his size.
            if (this.Data.SliderSize.HasValue) {
                float sliderWidth = this.Data.SliderSize.Value.X;
                float usableWidth = this.Size.X - sliderWidth;
                
                fillWidth = (sliderWidth / 2.0F) + (percent * usableWidth);
            }
            else {
                fillWidth = this.Size.X * percent;
            }
            
            Color progressBarColor = this.IsHovered ? this.Data.FilledBarHoverColor : this.Data.FilledBarColor;
            
            if (!this.Interactable) {
                progressBarColor = this.Data.DisabledFilledBarColor;
            }
            
            float thickness = this.Data.FilledBarOutlineThickness * this.Gui.ScaleFactor;
            float scaledFillWidth = fillWidth * this.Scale.X * this.Gui.ScaleFactor;
            float totalWidth = this.ScaledSize.X;
            
            // Draw the outline of the progress bar.
            if (thickness > 0.0F) {
                Color outlineColor = this.IsHovered ? this.Data.FilledBarOutlineHoverColor : this.Data.FilledBarOutlineColor;
                
                if (!this.Interactable) {
                    outlineColor = this.Data.DisabledFilledBarOutlineColor;
                }
                
                float leftWidth = Math.Min(scaledFillWidth, thickness);
                
                // Right Line
                float rightWidth = 0.0F;
                if (scaledFillWidth > totalWidth - thickness) {
                    rightWidth = scaledFillWidth - (totalWidth - thickness);
                }
                
                // Draw the inner of the progress bar.
                float leftOffset = thickness;
                float rightOffset = Math.Max(0, scaledFillWidth - (totalWidth - thickness));
                float innerFillWidth = Math.Max(0, scaledFillWidth - leftOffset - rightOffset);
                
                renderQueue.SubmitPrimitive(1, static (batch, state) => {
                    
                    // Draw the top line.
                    batch.DrawFilledRectangle(state.TopRectangle, state.TopOrigin, state.Rotation, 0.5F, state.OutlineColor);
                    
                    // Draw the bottom line.
                    batch.DrawFilledRectangle(state.BottomRectangle, state.BottomOrigin, state.Rotation, 0.5F, state.OutlineColor);
                    
                    // Draw the left line.
                    batch.DrawFilledRectangle(state.LeftRectangle, state.LeftOrigin, state.Rotation, 0.5F, state.OutlineColor);
                    
                    // Right Line
                    if (state.RightRectangle.Width > 0.0F) {
                        batch.DrawFilledRectangle(state.RightRectangle, state.RightOrigin, state.Rotation, 0.5F, state.OutlineColor);
                    }
                    
                    // Draw the inner of the progress bar.
                    batch.DrawFilledRectangle(state.InnerRectangle, state.InnerOrigin, state.Rotation, 0.5F, state.Color);
                }, (TopRectangle: new RectangleF(this.Position.X, this.Position.Y, scaledFillWidth, thickness), TopOrigin: scaledOrigin, BottomRectangle: new RectangleF(this.Position.X, this.Position.Y, scaledFillWidth, thickness), BottomOrigin: scaledOrigin - new Vector2(0, this.ScaledSize.Y - thickness), LeftRectangle: new RectangleF(this.Position.X, this.Position.Y, leftWidth, this.ScaledSize.Y - thickness * 2.0F), LeftOrigin: scaledOrigin - new Vector2(0, thickness), RightRectangle: new RectangleF(this.Position.X, this.Position.Y, rightWidth, this.ScaledSize.Y - thickness * 2.0F), RightOrigin: scaledOrigin - new Vector2(totalWidth - thickness, thickness), InnerRectangle: new RectangleF(this.Position.X, this.Position.Y, innerFillWidth, this.ScaledSize.Y - thickness * 2.0F), InnerOrigin: scaledOrigin - new Vector2(leftOffset, thickness), Rotation: this.Rotation, Color: progressBarColor, OutlineColor: outlineColor), renderState);
            }
            else {
                
                // Draw the inner of the progress bar.
                renderQueue.SubmitPrimitive(1, static (batch, state) => {
                    batch.DrawFilledRectangle(state.Rectangle, state.Origin, state.Rotation, 0.5F, state.Color);
                }, (Rectangle: new RectangleF(this.Position.X, this.Position.Y, scaledFillWidth, this.ScaledSize.Y), Origin: scaledOrigin, Rotation: this.Rotation, Color: progressBarColor), renderState);
            }
        }
        
        // Draw slider.
        this.DrawSlider(renderQueue, renderState);
    }
    
    /// <summary>
    /// Draws the slider component of the rectangle slide bar within the specified primitive batch.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the primitive shapes.</param>
    /// <param name="renderState">The primitive render state applied to the slider.</param>
    private void DrawSlider(GuiRenderQueue renderQueue, PrimitiveGuiRenderState renderState) {
        if (!this.Data.SliderSize.HasValue) {
            return;
        }
        
        Color color = this.IsHovered ? this.Data.SliderHoverColor : this.Data.SliderColor;
        
        if (!this.Interactable) {
            color = this.Data.DisabledSliderColor;
        }
        
        // Clamp just for safety, the Value should also be clamped already.
        float value = Math.Clamp(this.Value, this.MinValue, this.MaxValue);
        float percent = (value - this.MinValue) / (this.MaxValue - this.MinValue);
        
        // Calculate the slider position within the usable width.
        float sliderWidth = this.Data.SliderSize.Value.X;
        float sliderHeight = this.Data.SliderSize.Value.Y;
        float usableWidth = this.ScaledSize.X - (sliderWidth * this.Scale.X * this.Gui.ScaleFactor);
        float xPos = (percent * usableWidth);
        
        // Calculate the slider origin to align it correctly with the bar.
        Vector2 origin = (this.Origin * this.Scale * this.Gui.ScaleFactor) - new Vector2(xPos, (this.ScaledSize.Y - sliderHeight * this.Scale.Y * this.Gui.ScaleFactor) / 2.0F);
        
        // Draw slider outline.
        if (this.Data.SliderOutlineThickness > 0.0F) {
            Color outlineColor = this.IsHovered ? this.Data.SliderOutlineHoverColor : this.Data.SliderOutlineColor;
            
            if (!this.Interactable) {
                outlineColor = this.Data.DisabledSliderOutlineColor;
            }
            
            float outlineThickness = this.Data.SliderOutlineThickness * this.Gui.ScaleFactor;
            
            renderQueue.SubmitPrimitive(2, static (batch, state) => {
                
                // Draw slider.
                batch.DrawFilledRectangle(state.Rectangle, state.Origin, state.Rotation, 0.5F, state.Color);
                
                // Draw slider outline.
                batch.DrawEmptyRectangle(state.Rectangle, state.Thickness, state.Origin, state.Rotation, 0.5F, state.OutlineColor);
            }, (Rectangle: new RectangleF(this.Position.X, this.Position.Y, sliderWidth * this.Scale.X * this.Gui.ScaleFactor, sliderHeight * this.Scale.Y * this.Gui.ScaleFactor), Thickness: outlineThickness, Origin: origin, Rotation: this.Rotation, Color: color, OutlineColor: outlineColor), renderState);
        }
        else {
            
            // Draw slider.
            renderQueue.SubmitPrimitive(2, static (batch, state) => {
                batch.DrawFilledRectangle(state.Rectangle, state.Origin, state.Rotation, 0.5F, state.Color);
            }, (Rectangle: new RectangleF(this.Position.X, this.Position.Y, sliderWidth * this.Scale.X * this.Gui.ScaleFactor, sliderHeight * this.Scale.Y * this.Gui.ScaleFactor), Origin: origin, Rotation: this.Rotation, Color: color), renderState);
        }
    }
    
    protected override void Dispose(bool disposing) { }
}