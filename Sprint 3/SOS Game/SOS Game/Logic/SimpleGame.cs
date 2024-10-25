using System;

namespace SOS_Game.Logic;

public sealed class SimpleGame : GameBoard
{
    public SimpleGame(int size) : base(size)
    {
        
    }

    public override GameType GetGameType() => GameType.Simple;

    public override bool IsGameOver()
    {
        Console.WriteLine("Simple Game Over");
        return BlueScore > 0 || RedScore > 0 || IsBoardFilled();
    }
}