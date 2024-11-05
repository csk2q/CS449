﻿using System;

namespace SOS_Game.Logic;

public class Player
{
    // Variables //
    public readonly PlayerType PlayerType;
    public int Score;
    public readonly bool IsComputer;

    // Constructor //

    public Player(PlayerType playerType, bool isComputer = false)
    {
        if(playerType == PlayerType.None)
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
    
    // Getters & Setters //


}