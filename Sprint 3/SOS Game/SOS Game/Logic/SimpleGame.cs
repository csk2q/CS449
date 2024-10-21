namespace SOS_Game.Logic;

public class SimpleGame : GameBoard
{
    public SimpleGame(int size) : base(size)
    {
        
    }

    public override GameType GetGameType() => GameType.Simple;

    public override bool isGameOver()
    {
        // Override placement function and count SOSes?
        // Or count scores in the base class?
        
        throw new System.NotImplementedException();
    }
}