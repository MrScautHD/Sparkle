using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class TextureTextBoxElement : GuiElement {
    
    public TextureTextBoxData TextBoxData { get; private set; }
    
    public LabelData LabelData { get; private set; }
    
    public LabelData HintLabelData { get; private set; }
    
    public int MaxTextLength { get; private set; }
    
    private bool _textInputActive;
    private bool _isCaretVisible;
    private double _caretTimer;
    
    public TextureTextBoxElement(TextureTextBoxData textBoxData, LabelData labelData, LabelData hintLabelData, Anchor anchor, Vector2 offset, int maxTextLength, Vector2? size = null, Vector2? origin = null, float rotation = 0, Func<bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, origin, rotation, clickFunc) {
        this.TextBoxData = textBoxData;
        this.LabelData = labelData;
        this.HintLabelData = hintLabelData;
        this.MaxTextLength = maxTextLength;
        this.Size = size ?? new Vector2(textBoxData.Texture.Width, textBoxData.Texture.Height);
    }
    
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        if (this.IsSelected) {
            if (!this._textInputActive || !Input.IsTextInputActive()) {
                Input.EnableTextInput();
                this._textInputActive = true;
            }
        }
        else {
            if (this._textInputActive) {
                Input.DisableTextInput();
                this._textInputActive = false;
            }
        }
        
        if (this._textInputActive) {
            if (Input.IsTextInputActive()) {
                
                // Write text.
                if (Input.GetTypedText(out string text)) { // TODO: Add a system that allows to do a longer text then the actual texture is.... And idk but maybe fix also the text of buttons that they use font size instead of text.Y.
                    if (this.LabelData.Text.Length <= this.MaxTextLength - 1) {
                        this.LabelData.Text += text;
                    }
                }
                
                // Remove text with "BackSpace".
                if (Input.IsKeyPressed(KeyboardKey.BackSpace, true)) {
                    if (this.LabelData.Text.Length > 0) {
                        this.LabelData.Text = this.LabelData.Text.Remove(this.LabelData.Text.Length - 1, 1);
                    }
                }
            }
        }
        
        // Caret timer. // TODO: Make it more like normal ones.
        if (this._textInputActive) {
            this._caretTimer += delta;
            
            if (this._caretTimer >= 0.5) {
                this._isCaretVisible = !this._isCaretVisible;
                this._caretTimer = 0;
            }
        }
        else {
            this._isCaretVisible = false;
            this._caretTimer = 0;
        }
    }
    
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw texture.
        Color buttonColor = this.IsHovered ? this.TextBoxData.HoverColor : this.TextBoxData.Color;
        context.SpriteBatch.DrawTexture(this.TextBoxData.Texture, this.Position, 0.5F, this.TextBoxData.SourceRect, this.TextBoxData.Scale * this.Gui.ScaleFactor, this.Origin, this.Rotation, buttonColor, this.TextBoxData.Flip);
        
        // Draw text.
        if (this.LabelData.Text != string.Empty) {
            this.DrawText(context.SpriteBatch, this.LabelData);
        }
        else {
            if (this.HintLabelData.Text != string.Empty) {
                this.DrawText(context.SpriteBatch, this.HintLabelData);
            }
        }
        
        context.SpriteBatch.End();

        if (this._isCaretVisible) {
            context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
            this.DrawCaret(context.PrimitiveBatch, this.LabelData);
            context.PrimitiveBatch.End();
        }
    }

    private void DrawText(SpriteBatch spriteBatch, LabelData labelData) {
        Vector2 textSize = labelData.Font.MeasureText(labelData.Text, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
        Vector2 textOrigin = new Vector2(textSize.X, labelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin);
        Vector2 textPos = this.Position;
        
        Color textColor = this.IsHovered ? labelData.HoverColor : labelData.Color;
        spriteBatch.DrawText(labelData.Font, labelData.Text, textPos, labelData.Size, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Scale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.Rotation, textColor, labelData.Style, labelData.Effect, labelData.EffectAmount);
    }
    
    private void DrawCaret(PrimitiveBatch primitiveBatch, LabelData labelData) {
        Vector2 textSize = labelData.Font.MeasureText(labelData.Text, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
        Vector2 caretOrigin = new Vector2(-textSize.X, labelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin);
        Vector2 caretPos = this.Position;
        
        RectangleF rectangle = new RectangleF(caretPos.X, caretPos.Y, 2.0F * this.Gui.ScaleFactor, labelData.Size * this.Gui.ScaleFactor);
        
        primitiveBatch.DrawFilledRectangle(rectangle, caretOrigin * this.Gui.ScaleFactor, this.Rotation, 0.5F, labelData.Color);
    }
}