using System;
using System.Collections.Generic;

namespace SOS_Game.Logic;

public abstract class GameBoard
{
    // Constants //

    // These *must* be public for ui binding to take place
    public const decimal DefaultBoardSize = 3;
    public const decimal MinBoardSize = 3;
    public const decimal MaxBoardSize = 20;


    // Public Variables //

    // Blue goes first
    // TODO allow setting who plays first?
    public Player PlayerTurn { get; private set; } = Player.BlueLeft;

    // Private Variables //

    protected readonly int size;

    // (Row, Column) 0,0 is top left.
    private TileType[][] board;

    protected record Turn(Player Player, Position Position, TileType TileType);

    protected List<Turn> turnRecord = [];


    // Constructor //
    public GameBoard(int size)
    {
        //Set variables
        this.size = (int)Math.Clamp(size, MinBoardSize, MaxBoardSize);

        // Generate board
        board = new TileType[size][];
        for (int i = 0; i < size; i++)
            board[i] = new TileType[size];
    }
    
    // Abstract functions //

    public abstract GameType GetGameType();
    public abstract bool isGameOver();
    
    
    // Getters/Setters & Helpers //

    private TileType getTile(int row, int column)
    {
        if (row >= 0 && row < size && column >= 0 && column < size)
            return board[row][column];
        else
            return TileType.None;
    }

    public bool IsBoardFilled()
    {
        // Check if board is filled
        return turnRecord.Count >= size * size;
    }
    
    // Business Functions //

    public bool PlaceTile(int row, int column, TileType tileType, out Sos[] completedSosArray)
    {
        if (tileType is not TileType.None // If the placing tile is S or O
            && row < size && column < size // AND position is valid 
            && board[row][column] == TileType.None) // AND Clicked tile is empty
        {
            // Place tile
            board[row][column] = tileType;

            // Update game record
            turnRecord.Add(new Turn(PlayerTurn, new Position(row, column), tileType));

            //TODO check for SOS
            completedSosArray = checkSOS(row, column);
            foreach (var (s1, o, s2) in completedSosArray)
            {
                Console.WriteLine($"SOS created for {tileType} at ({s1}, {o}, {s2})");
            }

            // Change turn
            //Todo if an SOS was made do NOT change the turn
            PlayerTurn = PlayerTurn == Player.BlueLeft ? Player.RedRight : Player.BlueLeft;

            // Tile was placed
            return true;
        }

        // Unable to place tile
        completedSosArray = [];
        return false;
    }

    Sos[] checkSOS(int row, int column)
    {
        var placedTile = board[row][column];
        List<Sos> newSOSes = [];

        switch (placedTile)
        {
            default:
            case TileType.None:
                // Pointed at an empty tile
                break;
            case TileType.S:
            {
                // TODO add check for when S is placed
                break;
            }

            case TileType.O:
            {
                // Check vertical
                if (getTile(row - 1, column) == TileType.S
                    && getTile(row + 1, column) == TileType.S)
                {
                    newSOSes.Add(new Sos(new Position(row - 1, column), new Position(row, column),
                        new Position(row + 1, column)));
                }

                // Check horizontal 
                if (getTile(row, column - 1) == TileType.S
                    && getTile(row, column + 1) == TileType.S)
                {
                    newSOSes.Add(new Sos(new Position(row, column - 1), new Position(row, column),
                        new Position(row, column + 1)));
                }

                // Check \ diagonal
                if (getTile(row - 1, column - 1) == TileType.S
                    && getTile(row + 1, column + 1) == TileType.S)
                {
                    newSOSes.Add(new Sos(new Position(row - 1, column - 1), new Position(row, column),
                        new Position(row + 1, column + 1)));
                }

                // Check / diagonal
                if (getTile(row - 1, column + 1) == TileType.S
                    && getTile(row + 1, column - 1) == TileType.S)
                {
                    newSOSes.Add(new Sos(new Position(row - 1, column + 1), new Position(row, column),
                        new Position(row + 1, column - 1 )));
                }

                break;
            }
        }

        return newSOSes.ToArray();
    }
    
}