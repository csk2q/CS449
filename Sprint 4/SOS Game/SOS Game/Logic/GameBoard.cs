using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    public PlayerType PlayerTypeTurn { get; private set; } = PlayerType.BlueLeft;
    public Player Blue { get; init; }
    public Player Red { get; init; }

    // Private Variables //

    //(Row, Column) 0,0 is top left.
    private TileType[][] board;
    private readonly int size;

    Random random = new Random();

    //Turn stuff
    protected record Turn(PlayerType Player, Position Position, TileType TileType);

    protected List<Turn> turnRecord = [];


    // Constructor //
    public GameBoard(int size, bool isBlueComputer, bool isRedComputer)
    {
        //Set variables
        this.size = (int)Math.Clamp(size, MinBoardSize, MaxBoardSize);

        // Generate board
        board = new TileType[size][];
        for (int i = 0; i < size; i++)
            board[i] = new TileType[size];
    }

    // Create Method //

    public static GameBoard CreateNewGame(GameType gameType, int boardSize, bool isBlueComputer, bool isRedComputer)
    {
        switch (gameType)
        {
            default:
            case GameType.Simple:
                return new SimpleGame(boardSize, isBlueComputer, isRedComputer);
                break;
            case GameType.General:
                return new GeneralGame(boardSize, isBlueComputer, isRedComputer);
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

    public PlayerType GetWinner()
    {
        if (IsGameOver())
            if (Blue.Score > Red.Score)
                return PlayerType.BlueLeft;
            else if (Red.Score > Blue.Score)
                return PlayerType.RedRight;
            else
                return PlayerType.None;
        else
            return PlayerType.None;
    }

    public bool IsCurentPlayerComputer()
    {
        if (PlayerTypeTurn == PlayerType.BlueLeft)
            return Blue.IsComputer;
        if (PlayerTypeTurn == PlayerType.RedRight)
            return Red.IsComputer;
        return false;
    }

    /*
     * TODO implement GetBoardState()
     * Should it call
     * It will return a yeld-able result?
     */
    public TileType[][] GetBoardState()
    {
        return board;
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
            turnRecord.Add(new Turn(PlayerTypeTurn, new Position(row, column), tileType));

            //TODO check for SOS
            completedSosArray = checkSos(row, column);
            foreach (var (s1, o, s2) in completedSosArray)
                Console.WriteLine($"SOS created for {tileType} at ({s1}, {o}, {s2})");


            // If one or more SOSes were made
            if (completedSosArray.Length > 0)
            {
                // Add score
                switch (PlayerTypeTurn)
                {
                    case PlayerType.BlueLeft:
                        Blue.ScorePoint();
                        break;
                    case PlayerType.RedRight:
                        Red.ScorePoint();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unknown player turn: " + PlayerTypeTurn);
                }
            }
            else // No SOSes were made
            {
                // Change turn
                PlayerTypeTurn = PlayerTypeTurn switch
                {
                    PlayerType.BlueLeft => PlayerType.RedRight,
                    PlayerType.RedRight => PlayerType.BlueLeft,
                    _ => throw new ArgumentOutOfRangeException("Unknown player turn: " + PlayerTypeTurn)
                };
            }

            // Tile was placed
            return true;
        }

        // Unable to place tile
        completedSosArray = [];
        return false;
    }

    private Sos[] checkSos(int row, int column)
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

    // Call to have computer players make their moves?
    private List<TurnResult> tickComputerPlayers()
    {
        List<TurnResult> moves = [];

        while (IsCurentPlayerComputer() && !IsGameOver())
        {
            var newMove = makeComputerMove();
            moves.Add(newMove);
        }

        return moves;
    }

    private TurnResult makeComputerMove()
    {
        // TODO read AC 8 for the logic to implement
        /*
         * AC 8.1 Computer takes the first turn
         * Given: A new game is started
         * And: The first turn is the computer’s
         * When: The first turn of the game is the computer’s
         * Then: Place a tile of either type at random on the board.
         */
        if (turnRecord.Count == 0)
        {
            bool success;
            Sos[] completedSosArray;
            int row;
            int column;
            TileType tileType;

            do
            {
                // Randomly choose location
                row = random.Next(0, size);
                column = random.Next(0, size);

                // Randomly choose the tile either S(1) or O(2)
                tileType = (TileType)random.Next(1, 3);

                success = PlaceTile(row, column, tileType, out completedSosArray);

                Debug.Assert(success, "Failed to place tile?");
            } while (!success);

            return new TurnResult(new Move(tileType, new Position(row, column)), completedSosArray);
        }

        /*
         * AC 8.2 Computer makes an SOS
         * Given: An ongoing game
         * When: It is the computer’s turn
         * And: A valid SOS can be completed
         * Then: The computer should place the tile needed to complete the SOS
         */

        /*
         * AC 8.3 Computer makes a blocking move
         * Given: An ongoing game
         * When: It is the computer’s turn
         * And: There are no SOSes to complete
         * Then: The computer should attempt to make a “blocking” move so that the human cannot score a point next turn if possible.
         * */

        /*
         * AC 8.4 Computer makes a random Move
         * Given: And ongoing game
         * When: The computer cannot make an SOS nor make a blocking move.
         * Then: The computer will make a random valid move.
         */
    }
}