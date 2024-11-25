namespace SOS_Game.Logic;

public class HumanPlayer : Player
{
    public HumanPlayer(PlayerType playerType) : base(playerType, false)
    {
    }

    public HumanPlayer(Player otherPlayer) : base(otherPlayer)
    {
    }
}