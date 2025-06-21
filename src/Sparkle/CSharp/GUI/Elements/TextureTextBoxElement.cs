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
    
    // TODO: Add marking renderer (Copy/Paste...)
    // TODO: Think about a Caret color.
    // TODO: Fix TextAlignment.
    // TODO: Fix Caret Pos clicking. (Origin, Rotation and TextScrollOffset system)
    
    public TextureTextBoxData TextBoxData { get; private set; }
    
    public LabelData LabelData { get; private set; }
    
    public LabelData HintLabelData { get; private set; }
    
    public int MaxTextLength { get; private set; }
    
    public TextAlignment TextAlignment;
    
    public (float Left, float Right) TextEdgeOffset;
    
    private bool _textInputActive;
    private int _textScrollOffset;
    private int _caretIndex;
    private bool _isCaretVisible;
    private double _caretTimer;
    
    public TextureTextBoxElement(TextureTextBoxData textBoxData, LabelData labelData, LabelData hintLabelData, Anchor anchor, Vector2 offset, int maxTextLength, TextAlignment textAlignment = TextAlignment.Left, (float Left, float Right)? textEdgeOffset = null, Vector2? size = null, Vector2? origin = null, float rotation = 0, Func<bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, origin, rotation, clickFunc) {
        this.TextBoxData = textBoxData;
        this.LabelData = labelData;
        this.HintLabelData = hintLabelData;
        this.MaxTextLength = maxTextLength;
        this.TextAlignment = textAlignment;
        this.TextEdgeOffset = textEdgeOffset ?? (0, 0);
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
                if (Input.GetTypedText(out string text)) {
                    if (this.LabelData.Text.Length + text.Length <= this.MaxTextLength) {
                        this.LabelData.Text = this.LabelData.Text.Insert(this._caretIndex, text);
                        this._caretIndex += text.Length;
                        
                        // Adjust of the scroll offset.
                        this.UpdateTextScroll(this.LabelData);
                        
                        // Show caret.
                        this._isCaretVisible = true;
                        this._caretTimer = 0;
                    }
                }
                
                // Move caret left.
                if (Input.IsKeyPressed(KeyboardKey.Left, true)) {
                    if (this._caretIndex > 0) {
                        this._caretIndex--;
                        
                        // Adjust of the scroll offset.
                        this.UpdateTextScroll(this.LabelData);
                        
                        // Show caret.
                        this._isCaretVisible = true;
                        this._caretTimer = 0;
                    }
                }
                
                // Move caret right.
                if (Input.IsKeyPressed(KeyboardKey.Right, true)) {
                    if (this._caretIndex < this.LabelData.Text.Length) {
                        this._caretIndex++;
                        
                        // Adjust of the scroll offset.
                        this.UpdateTextScroll(this.LabelData);
                        
                        // Show caret.
                        this._isCaretVisible = true;
                        this._caretTimer = 0;
                    }
                }
                
                // Remove text with "BackSpace".
                if (Input.IsKeyPressed(KeyboardKey.BackSpace, true)) {
                    if (this._caretIndex > 0) {
                        this.LabelData.Text = this.LabelData.Text.Remove(this._caretIndex - 1, 1);
                        this._caretIndex--;
                        
                        // Adjust of the scroll offset.
                        this.UpdateTextScroll(this.LabelData);
                        
                        // Show caret.
                        this._isCaretVisible = true;
                        this._caretTimer = 0;
                    }
                }
            }
        }
        
        // Handle caret positioning based on mouse clicks.
        if (this.IsClicked) {
            Vector2 clickPosition = Input.GetMousePosition();
            Vector2 textStartPos = new Vector2(this.Position.X + (this.TextEdgeOffset.Left * this.Gui.ScaleFactor), this.Position.Y);
            
            // Default caret index to the start of the text.
            this._caretIndex = 0;
            
            // Iterate through each character in the text to determine where the caret should be placed.
            if (this.LabelData.Text != string.Empty) {
                float cumulativeWidth = 0f;
                
                for (int i = 0; i < this.LabelData.Text.Length; i++) {
                    string character = this.LabelData.Text.Substring(i, 1);
                    float charWidth = this.LabelData.Font.MeasureText(character, this.LabelData.Size, this.LabelData.Scale * this.Gui.ScaleFactor, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Effect, this.LabelData.EffectAmount).X;
                    
                    float charStartPos = textStartPos.X + cumulativeWidth;
                    float charMidPos = charStartPos + (charWidth / 2);
                    
                    // Set the caret index if the mouse is within character bounds.
                    if (clickPosition.X < charStartPos + charWidth) {
                        this._caretIndex = (clickPosition.X <= charMidPos) ? i : i + 1;
                        
                        // Show caret.
                        this._isCaretVisible = true;
                        this._caretTimer = 0;
                        
                        return;
                    }
                    
                    cumulativeWidth += charWidth;
                }
                
                // If the mouse is beyond the last character, set the caret to end.
                if (clickPosition.X >= textStartPos.X + cumulativeWidth) {
                    this._caretIndex = this.LabelData.Text.Length;
                }
            }
            
            // Show caret.
            this._isCaretVisible = true;
            this._caretTimer = 0;
        }

        // Caret timer.
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
        string text = this.GetVisibleText(labelData);
        Vector2 textPos = this.Position;
        Vector2 textSize = labelData.Font.MeasureText(text, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
        Vector2 textOrigin = this.TextAlignment switch {
            TextAlignment.Left => new Vector2(this.Size.X, labelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin) - new Vector2(this.TextEdgeOffset.Left, 0.0F),
            TextAlignment.Right => new Vector2(-this.Size.X / 2.0F + (textSize.X - 2.0F), labelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin) - new Vector2(this.TextEdgeOffset.Right, 0.0F),
            TextAlignment.Center => new Vector2(textSize.X, labelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin) - new Vector2((this.TextEdgeOffset.Left + this.TextEdgeOffset.Right) / 2.0F, 0.0F),
            _ => throw new NullReferenceException($"TextAlignment '{this.TextAlignment}' is invalid or undefined.")
        };
        
        Color textColor = this.IsHovered ? labelData.HoverColor : labelData.Color;
        spriteBatch.DrawText(labelData.Font, text, textPos, labelData.Size, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Scale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.Rotation, textColor, labelData.Style, labelData.Effect, labelData.EffectAmount);
    }
    
    private void DrawCaret(PrimitiveBatch primitiveBatch, LabelData labelData) {
        Vector2 caretPos = this.Position;
        
        // Calculate horizontal offset from visible text start to caret.
        float caretOffsetX = 0f;
        int caretVisibleIndex = Math.Max(0, this._caretIndex - this._textScrollOffset);
        
        for (int i = 0; i < caretVisibleIndex && (this._textScrollOffset + i) < labelData.Text.Length; i++) {
            string character = labelData.Text.Substring(this._textScrollOffset + i, 1);
            Vector2 charSize = labelData.Font.MeasureText(character, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
            caretOffsetX += charSize.X;
        }
        
        Vector2 caretOrigin = this.TextAlignment switch {
            TextAlignment.Left => new Vector2(this.Size.X / 2.0F - caretOffsetX, labelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin) - new Vector2(this.TextEdgeOffset.Left, 0.0F),
            TextAlignment.Right => new Vector2(-(this.Size.X / 2.0F) + 2.0F, labelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin) - new Vector2(this.TextEdgeOffset.Right, 0.0F),
            TextAlignment.Center => new Vector2(-caretOffsetX, labelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin) - new Vector2((this.TextEdgeOffset.Left + this.TextEdgeOffset.Right) / 2.0F, 0.0F),
            _ => throw new NullReferenceException($"TextAlignment '{this.TextAlignment}' is invalid or undefined.")
        };
        
        RectangleF rectangle = new RectangleF(caretPos.X, caretPos.Y, 2.0F * this.Gui.ScaleFactor, labelData.Size * this.Gui.ScaleFactor);
        primitiveBatch.DrawFilledRectangle(rectangle, caretOrigin * this.Gui.ScaleFactor, this.Rotation, 0.5F, labelData.Color);
    }
    
    private string GetVisibleText(LabelData labelData) {
        string visibleText = string.Empty;
        float totalWidth = 0.0F;
        int startIndex = this._textScrollOffset;
        
        for (int i = startIndex; i < labelData.Text.Length; i++) {
            string character = labelData.Text.Substring(i, 1);
            Vector2 charSize = labelData.Font.MeasureText(character, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
            
            // Only add if it fits.
            if (totalWidth + charSize.X > this.Size.X - (this.TextEdgeOffset.Left + this.TextEdgeOffset.Right)) {
                break;
            }
            
            visibleText += character;
            totalWidth += charSize.X;
        }
        
        return visibleText;
    }
    
    private void UpdateTextScroll(LabelData labelData) {
        float visibleWidth = this.Size.X - (this.TextEdgeOffset.Left + this.TextEdgeOffset.Right);
        float width = 0.0F;
        int visibleCharCount = 0;
        
        // Get visible text based on the current scroll.
        for (int i = this._textScrollOffset; i < labelData.Text.Length; i++) {
            string character = labelData.Text.Substring(i, 1);
            Vector2 charSize = labelData.Font.MeasureText(character, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
            
            if (width + charSize.X > visibleWidth) {
                break;
            }
            
            width += charSize.X;
            visibleCharCount++;
        }
        
        // Caret before visible range => scroll left.
        if (this._caretIndex < this._textScrollOffset) {
            this._textScrollOffset = this._caretIndex;
        }
        else {
            float caretX = 0.0F;
            
            // Pixel-precise check for caret going beyond the visible width.
            for (int i = this._textScrollOffset; i <= this._caretIndex && i < labelData.Text.Length; i++) {
                string character = labelData.Text.Substring(i, 1);
                Vector2 charSize = labelData.Font.MeasureText(character, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
                
                caretX += charSize.X;
            }
            
            // Scroll enough to bring the caret back into visible range.
            if (caretX > visibleWidth) {
                while (caretX > visibleWidth && this._textScrollOffset < this._caretIndex) {
                    string firstChar = labelData.Text.Substring(this._textScrollOffset, 1);
                    Vector2 firstSize = labelData.Font.MeasureText(firstChar, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
                    
                    caretX -= firstSize.X;
                    this._textScrollOffset++;
                }
            }
        }
        
        // Flip back if nothing visible (like after deleting).
        if (visibleCharCount == 0.0F && labelData.Text.Length > 0.0F) {
            float testWidth = 0.0F;
            int reverseCount = 0;
            
            for (int i = labelData.Text.Length - 1; i >= 0; i--) {
                string character = labelData.Text.Substring(i, 1);
                Vector2 charSize = labelData.Font.MeasureText(character, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
                
                if (testWidth + charSize.X > visibleWidth) {
                    break;
                }
                
                testWidth += charSize.X;
                reverseCount++;
            }
            
            this._textScrollOffset = Math.Max(0, labelData.Text.Length - reverseCount);
        }
        
        // Clamp to valid range.
        this._textScrollOffset = Math.Clamp(this._textScrollOffset, 0, labelData.Text.Length);
    }
}