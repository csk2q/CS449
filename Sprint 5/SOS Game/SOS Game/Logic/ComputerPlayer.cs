namespace SOS_Game.Logic;

public class ComputerPlayer : Player
{
    public ComputerPlayer(PlayerType playerType) : base(playerType, true)
    {
    }

    public ComputerPlayer(Player otherPlayer) : base(otherPlayer)
    {
    }
}