using Bliss.CSharp.Fonts;
using Sparkle.CSharp;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.Test.CSharp;

public class GameTest : Game {
    
    public GameTest(GameSettings settings) : base(settings) {
        
    }

    protected override void Load(ContentManager content) {
        base.Load(content);

        Font font = content.Load(new FontContent("content/fontoe.ttf"));
    }
}