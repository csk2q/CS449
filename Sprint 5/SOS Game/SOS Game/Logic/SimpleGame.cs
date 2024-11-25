using System;

namespace SOS_Game.Logic;

public sealed class SimpleGame : GameBoard
{
    public SimpleGame(int size, bool isBlueComputer, bool isRedComputer) : base(size, isBlueComputer, isRedComputer)
    {
        
    }

    public SimpleGame(GameBoard game) : base(game)
    {
        
    }

    public override GameType GetGameType() => GameType.Simple;

    public override bool IsGameOver()
    {
        return Blue.Score > 0 || Red.Score > 0 || IsBoardFilled();
    }
}