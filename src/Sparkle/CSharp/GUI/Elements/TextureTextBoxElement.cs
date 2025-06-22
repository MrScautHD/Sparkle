using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class TextureTextBoxElement : GuiElement {
    
    // TODO: Add marking renderer (Copy/Paste...)
    // TODO: Think about a Caret / Highlight color.
    
    public TextureTextBoxData TextBoxData { get; private set; }
    
    public LabelData LabelData { get; private set; }
    
    public LabelData HintLabelData { get; private set; }
    
    public int MaxTextLength { get; private set; }
    
    public TextAlignment TextAlignment;
    
    public (float Left, float Right) TextEdgeOffset;
    
    private bool _textInputActive;
    private int _textScrollOffset;
    private (int Start, int End) _highlightRange;
    private int _caretIndex;
    private bool _isCaretVisible;
    private double _caretTimer;
    
    public TextureTextBoxElement(TextureTextBoxData textBoxData, LabelData labelData, LabelData hintLabelData, Anchor anchor, Vector2 offset, int maxTextLength, TextAlignment textAlignment = TextAlignment.Left, (float Left, float Right)? textEdgeOffset = null, Vector2? size = null, Vector2? origin = null, float rotation = 0, Func<bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, origin, rotation, clickFunc) {
        this.TextBoxData = textBoxData;
        this.LabelData = labelData;
        this.HintLabelData = hintLabelData;
        this.MaxTextLength = maxTextLength;
        this.TextAlignment = textAlignment;
        this.TextEdgeOffset = textEdgeOffset ?? (0.0F, 0.0F);
        this.Size = size ?? new Vector2(textBoxData.Texture.Width, textBoxData.Texture.Height);
    }
    
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        // Enable text input system.
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
                    int start = Math.Min(this._highlightRange.Start, this._highlightRange.End);
                    int end = Math.Max(this._highlightRange.Start, this._highlightRange.End);
                    
                    if (start != end) {
                        
                        // Replace highlighted text with the new text.
                        if (this.LabelData.Text.Length - (end - start) + text.Length <= this.MaxTextLength) {
                            this.LabelData.Text = this.LabelData.Text.Remove(start, end - start).Insert(start, text);
                            this._caretIndex = start + text.Length;
                            
                            // Adjust of the scroll offset.
                            this.UpdateTextScroll(this.LabelData);
                            
                            // Reset the highlight.
                            this._highlightRange = (0, 0);
                            
                            // Show caret.
                            this._isCaretVisible = true;
                            this._caretTimer = 0.0F;
                        }
                    }
                    else {
                        
                        // Insert text at the caret if no text is highlighted.
                        if (this.LabelData.Text.Length + text.Length <= this.MaxTextLength) {
                            this.LabelData.Text = this.LabelData.Text.Insert(this._caretIndex, text);
                            this._caretIndex += text.Length;
                            
                            // Adjust of the scroll offset.
                            this.UpdateTextScroll(this.LabelData);
                            
                            // Show caret.
                            this._isCaretVisible = true;
                            this._caretTimer = 0.0F;
                        }
                    }
                }
                
                // Remove text with "BackSpace".
                if (Input.IsKeyPressed(KeyboardKey.BackSpace, true)) {
                    int start = Math.Min(this._highlightRange.Start, this._highlightRange.End);
                    int end = Math.Max(this._highlightRange.Start, this._highlightRange.End);
                    
                    if (start != end) {
                        
                        // Remove the highlighted text.
                        this.LabelData.Text = this.LabelData.Text.Remove(start, end - start);
                        this._caretIndex = start;
                        
                        // Adjust of the scroll offset.
                        this.UpdateTextScroll(this.LabelData);
                        
                        // Reset the highlight.
                        this._highlightRange = (0, 0);
                    }
                    else if (this._caretIndex > 0) {
                        
                        // Remove the character to the left of the caret if no text is highlighted.
                        this.LabelData.Text = this.LabelData.Text.Remove(this._caretIndex - 1, 1);
                        this._caretIndex--;
                        
                        // Adjust of the scroll offset.
                        this.UpdateTextScroll(this.LabelData);
                    }
                    
                    // Show caret.
                    this._isCaretVisible = true;
                    this._caretTimer = 0.0F;
                }
                
                // Move caret left.
                if (Input.IsKeyPressed(KeyboardKey.Left, true)) {
                    if (this._highlightRange.Start != this._highlightRange.End && !Input.IsKeyDown(KeyboardKey.ShiftLeft)) {
                        
                        // If text is highlighted and Shift is not pressed, move caret to the start of the highlighted text.
                        this._caretIndex = Math.Min(this._highlightRange.Start, this._highlightRange.End);
                        this._highlightRange = (0, 0); // Reset highlight range.
                    }
                    else if (this._caretIndex > 0) {
                        
                        // Start or update highlighting if Shift is held down.
                        if (Input.IsKeyDown(KeyboardKey.ShiftLeft)) {
                            if (this._highlightRange.Start == this._highlightRange.End) {
                                this._highlightRange = (this._caretIndex, this._caretIndex - 1);
                            }
                            else {
                                this._highlightRange = (this._highlightRange.Start, this._highlightRange.End - 1);
                            }
                        } else {
                            this._highlightRange = (0, 0);
                        }
                        
                        this._caretIndex--;
                    }

                    // Adjust scroll offset.
                    this.UpdateTextScroll(this.LabelData);
                    
                    // Show caret.
                    this._isCaretVisible = true;
                    this._caretTimer = 0.0F;
                }
                
                // Move caret right.
                if (Input.IsKeyPressed(KeyboardKey.Right, true)) {
                    if (this._highlightRange.Start != this._highlightRange.End && !Input.IsKeyDown(KeyboardKey.ShiftLeft)) {
                        
                        // If text is highlighted and Shift is not pressed, move caret to the end of the highlighted text.
                        this._caretIndex = Math.Max(this._highlightRange.Start, this._highlightRange.End);
                        this._highlightRange = (0, 0); // Reset highlight range.
                    }
                    else if (this._caretIndex < this.LabelData.Text.Length) {
                        
                        // Start or update highlighting if Shift is held down.
                        if (Input.IsKeyDown(KeyboardKey.ShiftLeft)) {
                            if (this._highlightRange.Start == this._highlightRange.End) {
                                this._highlightRange = (this._caretIndex, this._caretIndex + 1);
                            }
                            else {
                                this._highlightRange = (this._highlightRange.Start, this._highlightRange.End + 1);
                            }
                        } else {
                            this._highlightRange = (0, 0);
                        }

                        this._caretIndex++;
                    }

                    // Adjust scroll offset.
                    this.UpdateTextScroll(this.LabelData);
                    
                    // Show caret.
                    this._isCaretVisible = true;
                    this._caretTimer = 0.0F;
                }
                
                // Highlight the whole text (CTRL + A).
                if (Input.IsKeyDown(KeyboardKey.ControlLeft) && Input.IsKeyPressed(KeyboardKey.A)) {
                    this._highlightRange = (0, this.LabelData.Text.Length);
                }
                
                // Highlight the text from caret to mouse pos (SHIFT + LEFT MOUSE BUTTON).
                if (Input.IsKeyDown(KeyboardKey.ShiftLeft) && Input.IsMouseButtonPressed(MouseButton.Left)) {
                    int mouseIndex = this.GetCaretIndexFromPosition(Input.GetMousePosition());
                    
                    // Update the highlight range dynamically.
                    this._highlightRange = (this._caretIndex, mouseIndex);
                    
                    // Show the caret and reset the timer for visibility toggle.
                    this._isCaretVisible = true;
                    this._caretTimer = 0.0F;
                    
                    // Cancel caret positioning.
                    return;
                }
                
                // Handle caret positioning based on mouse clicks.
                if (this.IsClicked && !Input.IsMouseButtonDoubleClicked(MouseButton.Left)) {
                    this._caretIndex = this.GetCaretIndexFromPosition(Input.GetMousePosition());
            
                    // Adjust scroll offset.
                    this.UpdateTextScroll(this.LabelData);
                    
                    // Show caret.
                    this._isCaretVisible = true;
                    this._caretTimer = 0.0F;
                }
                
                // Highlight a word (DOUBLE LEFT CLICK).
                if (Input.IsMouseButtonDoubleClicked(MouseButton.Left)) {
                    int startIndex = this._caretIndex;
                    int endIndex = this._caretIndex;
                    
                    // Define characters that act as word delimiters.
                    char[] wordSeparators = [' ', '.', ',', ';', '/', '(', ')', '[', ']', '{', '}', '&', '-', '_', '!', '?', ':', '"', '\'', '\\', '|'];
                    
                    // Find the start of the word by moving backwards until encountering a delimiter or the beginning of the text.
                    while (startIndex > 0 && !wordSeparators.Contains(this.LabelData.Text[startIndex - 1])) {
                        startIndex--;
                    }
                    
                    // Find the end of the word by moving forward until encountering a delimiter or the end of the text.
                    while (endIndex < this.LabelData.Text.Length && !wordSeparators.Contains(this.LabelData.Text[endIndex])) {
                        endIndex++;
                    }
                    
                    // Set the highlight range.
                    this._highlightRange = (startIndex, endIndex);
                    
                    // Move the caret to the end of the highlighted word.
                    this._caretIndex = endIndex;
                    
                    // Adjust scroll offset.
                    this.UpdateTextScroll(this.LabelData);
                    
                    // Show caret.
                    this._isCaretVisible = true;
                    this._caretTimer = 0.0F;
                }
                
                // TODO:
                // Highlight the text from the starting caret position to the current mouse position while holding the left click.
                //if (Input.IsMouseButtonDown(MouseButton.Left) && !Input.IsMouseButtonDoubleClicked(MouseButton.Left)) {
                //    int mouseIndex = this.GetCaretIndexFromPosition(Input.GetMousePosition());
                //    
                //    // Update the highlight range dynamically as the mouse moves.
                //    this._highlightRange = (this._caretIndex, mouseIndex);
                //}
                
                // Copy highlighted text (CTRL + C).
                if (Input.IsKeyDown(KeyboardKey.ControlLeft) && Input.IsKeyPressed(KeyboardKey.C)) {
                    Input.SetClipboardText(this.LabelData.Text.Substring(this._highlightRange.Start, this._highlightRange.End - this._highlightRange.Start));
                }
                
                // Paste text (CTRL + V).
                if (Input.IsKeyDown(KeyboardKey.ControlLeft) && Input.IsKeyPressed(KeyboardKey.V)) {
                    string clipboardText = Input.GetClipboardText();
                    
                    if (clipboardText != string.Empty) {
                        int start = Math.Min(this._highlightRange.Start, this._highlightRange.End);
                        int end = Math.Max(this._highlightRange.Start, this._highlightRange.End);
                        
                        if (start != end) {
                            
                            // Replace the highlighted text with the new text.
                            this.LabelData.Text = this.LabelData.Text.Remove(start, end - start).Insert(start, clipboardText);
                            this._caretIndex = start + clipboardText.Length;
                        }
                        else {
                            
                            // Insert text if no text is highlighted.
                            this.LabelData.Text = this.LabelData.Text.Insert(this._caretIndex, clipboardText);
                            this._caretIndex += clipboardText.Length;
                        }
                        
                        // Adjust of the scroll offset.
                        this.UpdateTextScroll(this.LabelData);
                        
                        // Reset highlight.
                        this._highlightRange = (0, 0);
                    }
                }
            }
        }
        
        // Caret timer.
        if (this._textInputActive) {
            this._caretTimer += delta;
            
            if (this._caretTimer >= 0.5) {
                this._isCaretVisible = !this._isCaretVisible;
                this._caretTimer = 0.0F;
            }
        }
        else {
            this._isCaretVisible = false;
            this._caretTimer = 0.0F;
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
        
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw caret.
        if (this._isCaretVisible) {
            this.DrawCaret(context.PrimitiveBatch, this.LabelData);
        }
        
        // Draw highlight.
        this.DrawHighlight(context.PrimitiveBatch, this.LabelData);
        
        context.PrimitiveBatch.End();
    }
    
    private void DrawText(SpriteBatch spriteBatch, LabelData labelData) {
        string text = this.GetVisibleText(labelData);
        Vector2 textPos = this.Position;
        Vector2 textSize = labelData.Font.MeasureText(text, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
        Vector2 textOrigin = this.TextAlignment switch {
            TextAlignment.Left => new Vector2(this.Size.X, labelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin) - new Vector2(this.TextEdgeOffset.Left, 0.0F),
            TextAlignment.Center => new Vector2(textSize.X, labelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin),
            TextAlignment.Right => new Vector2(-this.Size.X / 2.0F + (textSize.X + 2.0F), labelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin) + new Vector2(this.TextEdgeOffset.Right, 0.0F),
            _ => throw new ArgumentOutOfRangeException($"TextAlignment '{this.TextAlignment}' is invalid or undefined.")
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
        
        // Calculate visible text size, used for text alignment.
        Vector2 visibleTextSize = labelData.Font.MeasureText(this.GetVisibleText(labelData), labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
        
        Vector2 caretOrigin = this.TextAlignment switch {
            TextAlignment.Left => new Vector2(this.Size.X / 2.0F - caretOffsetX, labelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin) - new Vector2(this.TextEdgeOffset.Left, 0.0F),
            TextAlignment.Center => new Vector2((visibleTextSize.X / 2.0F) - caretOffsetX, labelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin),
            TextAlignment.Right => new Vector2(-this.Size.X / 2.0F + (visibleTextSize.X + 2.0F) - caretOffsetX, labelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin) + new Vector2(this.TextEdgeOffset.Right, 0.0F),
            _ => throw new ArgumentOutOfRangeException($"TextAlignment '{this.TextAlignment}' is invalid or undefined.")
        };
        
        // Draw caret rectangle.
        RectangleF rectangle = new RectangleF(caretPos.X, caretPos.Y, 2.0F * this.Gui.ScaleFactor, labelData.Size * this.Gui.ScaleFactor);
        primitiveBatch.DrawFilledRectangle(rectangle, caretOrigin * this.Gui.ScaleFactor, this.Rotation, 0.5F, labelData.Color);
    }
    
    private void DrawHighlight(PrimitiveBatch primitiveBatch, LabelData labelData) {
        if (this._highlightRange.Start == this._highlightRange.End)
            return;
        
        // Visible text.
        string text = this.GetVisibleText(labelData);
        
        // Start/End index.
        int startIndex = Math.Max(Math.Min(this._highlightRange.Start, this._highlightRange.End), this._textScrollOffset);
        int endIndex = Math.Min(Math.Max(this._highlightRange.Start, this._highlightRange.End), this._textScrollOffset + text.Length);
        
        Vector2 highlightPos = this.Position;
        float highlightOffsetX = 0.0F;
        float highlightWidth = 0.0F;
        
        // Calculate the width of the highlighted text.
        for (int i = startIndex; i < endIndex && i < labelData.Text.Length; i++) {
            string character = labelData.Text.Substring(i, 1);
            Vector2 charSize = labelData.Font.MeasureText(character, labelData.Size, labelData.Scale * this.Gui.ScaleFactor, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
    
            if (i >= this._textScrollOffset) {
                highlightWidth += charSize.X;
            }
        }
        
        // Move the highlight start position if the text is scrolled.
        int visibleStartIndex = Math.Max(startIndex, this._textScrollOffset);
        
        for (int i = this._textScrollOffset; i < visibleStartIndex; i++) {
            string character = labelData.Text.Substring(i, 1);
            Vector2 charSize = labelData.Font.MeasureText(character, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
            
            highlightOffsetX += charSize.X;
        }
        
        // Calculate visible text size, used for text alignment.
        Vector2 visibleTextSize = labelData.Font.MeasureText(text, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
        
        Vector2 highlightOrigin = this.TextAlignment switch {
            TextAlignment.Left => new Vector2(this.Size.X / 2.0F - highlightOffsetX, labelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin) - new Vector2(this.TextEdgeOffset.Left, 0.0F),
            TextAlignment.Center => new Vector2((visibleTextSize.X / 2.0F) - highlightOffsetX, labelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin),
            TextAlignment.Right => new Vector2(-this.Size.X / 2.0F + (visibleTextSize.X + 2.0F) - highlightOffsetX, labelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin) + new Vector2(this.TextEdgeOffset.Right, 0.0F),
            _ => throw new ArgumentOutOfRangeException($"TextAlignment '{this.TextAlignment}' is invalid or undefined.")
        };
        
        // Draw the highlight rectangle.
        RectangleF highlightRectangle = new(highlightPos.X, highlightPos.Y, highlightWidth, labelData.Size * this.Gui.ScaleFactor);
        primitiveBatch.DrawFilledRectangle(highlightRectangle, highlightOrigin * this.Gui.ScaleFactor, this.Rotation, 0.5F, new Color(0, 128, 228, 128)); // TODO: Replace this color.
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
        
        // Adjust scroll behavior based on text alignment.
        switch (this.TextAlignment) {
            
            // Flip back if nothing visible (like after deleting).
            case TextAlignment.Left: {
                if (visibleCharCount == 0.0F && labelData.Text.Length > 0.0F) {
                    float totalWidth = 0.0F;
                    int reverseCount = 0;
            
                    for (int i = labelData.Text.Length - 1; i >= 0; i--) {
                        string character = labelData.Text.Substring(i, 1);
                        Vector2 charSize = labelData.Font.MeasureText(character, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
                
                        if (totalWidth + charSize.X > visibleWidth) {
                            break;
                        }
                
                        totalWidth += charSize.X;
                        reverseCount++;
                    }
            
                    this._textScrollOffset = Math.Max(0, labelData.Text.Length - reverseCount);
                }
                break;
            }
            
            // Flip back, instant (like after deleting).
            case TextAlignment.Center: {
                float totalWidth = 0.0F;
                
                for (int i = this._textScrollOffset; i < labelData.Text.Length; i++) {
                    string character = labelData.Text.Substring(i, 1);
                    Vector2 charSize = labelData.Font.MeasureText(character, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
                    
                    if (totalWidth + charSize.X > visibleWidth) {
                        break;
                    }
                    
                    totalWidth += charSize.X;
                }
                
                // Check if there is enough space to reduce the scroll offset.
                if (totalWidth < visibleWidth && this._textScrollOffset > 0) {
                    string previousCharacter = labelData.Text.Substring(this._textScrollOffset - 1, 1);
                    Vector2 previousCharSize = labelData.Font.MeasureText(previousCharacter, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
    
                    // Reduce the offset only if the previous character can fully fit.
                    if (previousCharSize.X <= visibleWidth - totalWidth) {
                        this._textScrollOffset--;
                    }
                }
                break;
            }
            
            // Flip back, instant (like after deleting).
            case TextAlignment.Right:
                if (visibleCharCount == 0.0F && labelData.Text.Length > 0.0F) {
                    float totalWidth = 0.0F;
                    int reverseCount = 0;

                    // Berechne Zeichen von rechts, bis sie sichtbar sind.
                    for (int i = labelData.Text.Length - 1; i >= 0; i--) {
                        string character = labelData.Text.Substring(i, 1);
                        Vector2 charSize = labelData.Font.MeasureText(character, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
            
                        if (totalWidth + charSize.X > visibleWidth) {
                            break;
                        }
            
                        totalWidth += charSize.X;
                        reverseCount++;
                    }
                    this._textScrollOffset = Math.Max(0, labelData.Text.Length - reverseCount);
                }
                break;
        }
        
        // Adjust scroll when caret is at the fully visible edge (left or right).
        if (this._caretIndex == this._textScrollOffset && visibleCharCount > 0.0F) {
            this._textScrollOffset = Math.Max(0, this._caretIndex - 1);
        }
        
        // Clamp to valid range.
        this._textScrollOffset = Math.Clamp(this._textScrollOffset, 0, labelData.Text.Length);
    }
    
    private int GetCaretIndexFromPosition(Vector2 clickPosition) {
        Matrix4x4 rotationZ = Matrix4x4.CreateRotationZ(float.DegreesToRadians(-this.Rotation));
        Vector2 localClickPosition = Vector2.Transform(clickPosition - this.Position, rotationZ) + this.Origin * this.Gui.ScaleFactor;
        
        // Calculate visible text size, used for text alignment.
        Vector2 visibleTextSize = this.LabelData.Font.MeasureText(this.GetVisibleText(this.LabelData), this.LabelData.Size, this.LabelData.Scale * this.Gui.ScaleFactor, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Effect, this.LabelData.EffectAmount);
        
        // Determine the starting position of the text in the textbox based on the text alignment.
        Vector2 textStartPos = this.TextAlignment switch {
            TextAlignment.Left => new Vector2(this.TextEdgeOffset.Left * this.Gui.ScaleFactor, 0),
            TextAlignment.Center => new Vector2((this.ScaledSize.X - visibleTextSize.X) / 2.0F, 0),
            TextAlignment.Right => new Vector2(this.ScaledSize.X - visibleTextSize.X - (this.TextEdgeOffset.Right * this.Gui.ScaleFactor), 0),
            _ => throw new ArgumentOutOfRangeException($"TextAlignment '{this.TextAlignment}' is invalid or undefined.")
        };
        
        // By default, start the caret index at the visible text's start.
        int caretIndex = this._textScrollOffset;
        
        if (this.LabelData.Text != string.Empty) {
            float cumulativeWidth = 0.0F;
            
            for (int i = this._textScrollOffset; i < this.LabelData.Text.Length; i++) {
                string character = this.LabelData.Text.Substring(i, 1);
                float charWidth = this.LabelData.Font.MeasureText(character, this.LabelData.Size, this.LabelData.Scale * this.Gui.ScaleFactor, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing).X;
                
                float charStartPos = textStartPos.X + cumulativeWidth;
                float charMidPos = charStartPos + (charWidth / 2.0F);
                
                // Check if the mouse falls within this character's bounds.
                if (localClickPosition.X < charStartPos + charWidth) {
                    caretIndex = (localClickPosition.X <= charMidPos) ? i : i + 1;
                    return caretIndex;
                }
                
                cumulativeWidth += charWidth;
            }
            
            // If the click is beyond the last visible character, move the caret to the end.
            if (localClickPosition.X >= textStartPos.X + cumulativeWidth) {
                caretIndex = this.LabelData.Text.Length;
                this.UpdateTextScroll(this.LabelData);
            }
        }
        
        return caretIndex;
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
}