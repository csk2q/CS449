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
    public int BlueScore { get; private set; } = 0;
    public int RedScore { get; private set; } = 0;


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
    
    // Create Method //

    public static GameBoard CreateNewGame(GameType gameType, int boardSize)
    {
        switch (gameType)
        {
            default:
            case GameType.Simple:
                return new SimpleGame(boardSize);
                break;
            case GameType.General:
                return new GeneralGame(boardSize);
                break;
        }
    }

    // Abstract functions //

    public abstract GameType GetGameType();
    public abstract bool IsGameOver();


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

    public Player GetWinner()
    {
        if (IsGameOver())
            if (BlueScore > RedScore)
                return Player.BlueLeft;
            else if (RedScore > BlueScore)
                return Player.RedRight;
            else
                return Player.None;
        else
            return Player.None;
    }

    // Business Functions //

    public bool PlaceTile(int row, int column, TileType tileType, out Sos[] completedSosArray)
    {
        // Don't place if the game is over or the board is filled
        if (IsGameOver() || IsBoardFilled())
        {
            completedSosArray = [];
            return false;
        }
        
        if (tileType is not TileType.None // If the placing tile is S or O
            && row < size && column < size // AND position is valid 
            && board[row][column] == TileType.None) // AND Clicked tile is empty
        {
            // Place tile
            board[row][column] = tileType;

            // Update game record
            turnRecord.Add(new Turn(PlayerTurn, new Position(row, column), tileType));

            //TODO check for SOS
            completedSosArray = checkSos(row, column);
            foreach (var (s1, o, s2) in completedSosArray)
                Console.WriteLine($"SOS created for {tileType} at ({s1}, {o}, {s2})");


            // If one or more SOSes were made
            if (completedSosArray.Length > 0)
            {
                // Add score
                switch (PlayerTurn)
                {
                    case Player.BlueLeft:
                        BlueScore += 1;
                        break;
                    case Player.RedRight:
                        RedScore += 1;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unknown player turn: " + PlayerTurn);
                }
            }
            else // No SOSes were made
            {
                // Change turn
                PlayerTurn = PlayerTurn switch
                {
                    Player.BlueLeft => Player.RedRight,
                    Player.RedRight => Player.BlueLeft,
                    _ => throw new ArgumentOutOfRangeException("Unknown player turn: " + PlayerTurn)
                };
            }

            // Tile was placed
            return true;
        }

        // Unable to place tile
        completedSosArray = [];
        return false;
    }

    Sos[] checkSos(int row, int column)
    {
        var placedTile = board[row][column];
        List<Sos> newSoSes = [];

        // Switch on tile type, either S or O
        switch (placedTile)
        {
            // Empty or invalid tile
            case TileType.None:
            default:
                // Do nothing
                break;

            // S tile was placed
            case TileType.S:
            {
                // Check vertical up
                if (getTile(row, column - 1) == TileType.O
                    && getTile(row, column - 2) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row, column), new Position(row, column - 1),
                        new Position(row, column - 2)));
                }

                // Check vertical down
                if (getTile(row, column + 1) == TileType.O
                    && getTile(row, column + 2) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row, column), new Position(row, column + 1),
                        new Position(row, column + 2)));
                }

                // Check horizontal left
                if (getTile(row - 1, column) == TileType.O
                    && getTile(row - 2, column) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row, column), new Position(row - 1, column),
                        new Position(row - 2, column)));
                }

                // Check horizontal right
                if (getTile(row + 1, column) == TileType.O
                    && getTile(row + 2, column) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row, column), new Position(row + 1, column),
                        new Position(row + 2, column)));
                }

                // Check \ diagonal top-left
                if (getTile(row - 1, column - 1) == TileType.O
                    && getTile(row - 2, column - 2) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row, column), new Position(row - 1, column - 1),
                        new Position(row - 2, column - 2)));
                }

                // Check \ diagonal bottom-right
                if (getTile(row + 1, column + 1) == TileType.O
                    && getTile(row + 2, column + 2) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row, column), new Position(row + 1, column + 1),
                        new Position(row + 2, column + 2)));
                }

                // Check / diagonal top-right
                if (getTile(row + 1, column - 1) == TileType.O
                    && getTile(row + 2, column - 2) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row, column), new Position(row + 1, column - 1),
                        new Position(row + 2, column - 2)));
                }


                // Check / diagonal bottom-left
                if (getTile(row - 1, column + 1) == TileType.O
                    && getTile(row - 2, column + 2) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row, column), new Position(row - 1, column + 1),
                        new Position(row - 2, column + 2)));
                }


                break;
            }

            // O tile was placed
            case TileType.O:
            {
                // Check vertical
                if (getTile(row - 1, column) == TileType.S
                    && getTile(row + 1, column) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row - 1, column), new Position(row, column),
                        new Position(row + 1, column)));
                }

                // Check horizontal 
                if (getTile(row, column - 1) == TileType.S
                    && getTile(row, column + 1) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row, column - 1), new Position(row, column),
                        new Position(row, column + 1)));
                }

                // Check \ diagonal
                if (getTile(row - 1, column - 1) == TileType.S
                    && getTile(row + 1, column + 1) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row - 1, column - 1), new Position(row, column),
                        new Position(row + 1, column + 1)));
                }

                // Check / diagonal
                if (getTile(row - 1, column + 1) == TileType.S
                    && getTile(row + 1, column - 1) == TileType.S)
                {
                    newSoSes.Add(new Sos(new Position(row - 1, column + 1), new Position(row, column),
                        new Position(row + 1, column - 1)));
                }

                break;
            }
        }

        return newSoSes.ToArray();
    }
}