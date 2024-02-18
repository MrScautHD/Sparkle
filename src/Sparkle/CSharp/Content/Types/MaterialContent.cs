using Raylib_cs;

namespace Sparkle.CSharp.Content.Types;

public class MaterialContent : IContentType<Material[]> {
    
    public string Path { get; }

    public MaterialContent(string path) {
        this.Path = path;
    }
}