namespace Sparkle.csharp.content.type; 

public class ModelAnimationContent : IContentType {
    
    public string Path { get; set; }

    public ModelAnimationContent(string path) {
        this.Path = path;
    }
}