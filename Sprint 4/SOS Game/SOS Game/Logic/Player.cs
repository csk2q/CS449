using System;

namespace SOS_Game.Logic;

public struct Player
{
    // Variables //
    public readonly PlayerType PlayerType;
    public int Score { get; private set; } = 0;
    public readonly bool IsComputer;

    // Constructor //

    public Player(PlayerType playerType, bool isComputer = false)
    {
        if(playerType == PlayerType.None)
            throw new ArgumentException("Player type cannot be None");
        
        this.PlayerType = playerType;
        this.IsComputer = isComputer;
    }
    
    // Getters & Setters //

    // Adds the given points to the player's score
    // Returns the total score.
    public int ScorePoint(int points = 1)
    {
        return Score += points;
    }


}