namespace Sparkle.CSharp.GUI;

public struct BorderInsets {
    
    /// <summary>
    /// Represents a border with all sides set to zero.
    /// </summary>
    public static readonly BorderInsets Zero = new BorderInsets(0);
    
    /// <summary>
    /// The size of the left border inset.
    /// </summary>
    public int Left;
    
    /// <summary>
    /// The size of the right border inset.
    /// </summary>
    public int Right;
    
    /// <summary>
    /// The size of the top border inset.
    /// </summary>
    public int Top;
    
    /// <summary>
    /// The size of the bottom border inset.
    /// </summary>
    public int Bottom;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BorderInsets"/> struct.
    /// </summary>
    /// <param name="uniform">The inset value applied equally to all sides.</param>
    public BorderInsets(int uniform) {
        this.Left = this.Right = this.Top = this.Bottom = uniform;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BorderInsets"/> struct.
    /// </summary>
    /// <param name="left">The inset value for the left side.</param>
    /// <param name="right">The inset value for the right side.</param>
    /// <param name="top">The inset value for the top side.</param>
    /// <param name="bottom">The inset value for the bottom side.</param>
    public BorderInsets(int left, int right, int top, int bottom) {
        this.Left = left;
        this.Right = right;
        this.Top = top;
        this.Bottom = bottom;
    }
}