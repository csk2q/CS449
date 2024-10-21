namespace SOS_Game.Logic;

public class GeneralGame : GameBoard
{
    public GeneralGame(int size) : base(size)
    {
        
    }

    public override GameType GetGameType() => GameType.General;

    public override bool isGameOver() => IsBoardFilled();
}