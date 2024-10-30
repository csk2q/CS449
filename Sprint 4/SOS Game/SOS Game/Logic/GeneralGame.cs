namespace SOS_Game.Logic;

public sealed class GeneralGame : GameBoard
{
    public GeneralGame(int size) : base(size)
    {
        
    }

    public override GameType GetGameType() => GameType.General;

    public override bool IsGameOver() => IsBoardFilled();
}