using Sparkle.CSharp;
using Sparkle.ImGUI.Scenes;

namespace Sparkle.ImGUI;

internal static class Program
{
    private static void Main(string[] args)
    {
        var gameSettings = new GameSettings
        {
            Title = "Sparkle - [ImGUI]"
        };

        using var simpleGame = new SimpleGame(gameSettings);
        simpleGame.Run(new MainScene());
    }
}