namespace SOS_Game.Logic;

public sealed class GeneralGame : GameBoard
{
    public GeneralGame(int size, bool isBlueComputer, bool isRedComputer) : base(size, isBlueComputer, isRedComputer)
    {
        
    }

    public GeneralGame(GameBoard other) : base(other)
    {
        
    }

    public override GameType GetGameType() => GameType.General;

    public override bool IsGameOver() => IsBoardFilled();
}