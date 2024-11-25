using System;

namespace SOS_Game.Logic;

public abstract class Player
{
    // Variables //
    public readonly PlayerType PlayerType;
    public int Score;
    public readonly bool IsComputer;

    // Constructor //

    protected Player(PlayerType playerType, bool isComputer = false)
    {
        if (playerType == PlayerType.None)
            throw new ArgumentException("Player type cannot be None");

        this.PlayerType = playerType;
        this.IsComputer = isComputer;
        Score = 0;
    }

    public Player(Player otherPlayer)
    {
        this.PlayerType = otherPlayer.PlayerType;
        this.IsComputer = otherPlayer.IsComputer;
        Score = otherPlayer.Score;
    }

    // Create Method //
    public static Player Create(PlayerType playerType, bool isComputer = false)
    {
        return isComputer ? new ComputerPlayer(playerType) : new HumanPlayer(playerType);
    }

    // Clone Method //
    public static Player Clone(Player otherPlayer)
    {
        return otherPlayer.IsComputer
            ? new ComputerPlayer(otherPlayer) { Score = otherPlayer.Score }
            : new HumanPlayer(otherPlayer) { Score = otherPlayer.Score };
    }
}