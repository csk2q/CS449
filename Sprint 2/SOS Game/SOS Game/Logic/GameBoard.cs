using System;
using System.Collections.Generic;

namespace SOS_Game.Logic;

public class GameBoard
{
    // Constants //
    
    // These *must* be public for ui binding to take place
    public const decimal MinBoardSize = 3;
    public const decimal MaxBoardSize = 20;
    
    
    // Public Variables //
    
    // Blue goes first
    // TODO allow setting who plays first?
    public Player PlayerTurn { get; private set; } = Player.BlueLeft;
    
    // Private Variables //
    
    private GameType gameType;
    private readonly int size;
    private TileType[][] board;
    
    record Turn(Player Player, Position Position, TileType TileType);
    private List<Turn> turnRecord = [];
    
    
    // Constructor //
    public GameBoard(GameType gameType, int size)
    {
        //Set variables
        this.size = (int)Math.Clamp(size, MinBoardSize, MaxBoardSize);;
        this.gameType = gameType;
        
        // Generate board
        board = new TileType[size][];
        for (int i = 0; i < size; i++)
            board[i] = new TileType[size];
    }

    public bool PlaceTile(int row, int column, TileType tileType)
    {
        // If position is valid AND tile is empty
        if (row < size && column < size && board[row][column] == TileType.None)
        {
            // Place tile
            board[row][column] = tileType;
            
            // Update game record
            turnRecord.Add(new Turn(PlayerTurn, new Position(row, column), tileType));
            
            //TODO check for SOS
            
            // Change turn
            //Todo if an SOS was made do NOT change the turn
            PlayerTurn = PlayerTurn == Player.BlueLeft ? Player.RedRight : Player.BlueLeft;
            
            // Check if board is filled
            if (turnRecord.Count >= size * size)
            {
                Console.Beep();
                Console.WriteLine("Game Over!");
            }

            // Tile was placed
            return true;
        }

        // Unable to place tile
        return false;
    }
}