using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SOS_Game.Logic;

public abstract class GameBoard : IDisposable
{
    // Constants //

    // These *must* be public for ui binding to take place
    public const decimal DefaultBoardSize = 3;
    public const decimal MinBoardSize = 3;
    public const decimal MaxBoardSize = 20;

    // Events //

    public delegate void OnGameBoardUpdate(TurnResult turn);

    private event OnGameBoardUpdate? OnBoardUpdateEvent;

    // Public Variables //
    public bool IsGameStarted { get; private set; }

    // Blue goes first
    // TODO allow setting who plays first?
    public PlayerType CurPlayerTurn { get; private set; } = PlayerType.BlueLeft;
    public Player Blue { get; init; }
    public Player Red { get; init; }

    // Private Variables //

    //(Row, Column) 0,0 is top left.
    private TileType[][] board;
    private readonly int size;

    private Random random = new Random();

    //Turn stuff
    public record Turn(PlayerType Player, Position Position, TileType TileType);

    protected List<Turn> turnRecord = [];


    // Constructor //
    protected GameBoard(int size, bool isBlueComputer, bool isRedComputer)
    {
        //Set variables
        this.size = (int)Math.Clamp(size, MinBoardSize, MaxBoardSize);
        Blue = Player.Create(PlayerType.BlueLeft, isBlueComputer);
        Red = Player.Create(PlayerType.RedRight, isRedComputer);

        // Generate board
        board = new TileType[size][];
        for (int i = 0; i < size; i++)
            board[i] = new TileType[size];
    }

    // Copy constructor
    protected GameBoard(GameBoard other)
    {
        board = other.board.Select(a => (TileType[])a.Clone()).ToArray();
        size = other.size;
        CurPlayerTurn = other.CurPlayerTurn;
        Blue = Player.Clone(other.Blue);
        Red = Player.Clone(other.Red);
        random = other.random;
        turnRecord = new List<Turn>(other.turnRecord);
        OnBoardUpdateEvent = null;
    }

    // Create Method //

    public static GameBoard CreateNewGame(GameType gameType, int boardSize, bool isBlueComputer, bool isRedComputer)
    {
        GameBoard newGame;

        switch (gameType)
        {
            default:
            case GameType.Simple:
                newGame = new SimpleGame(boardSize, isBlueComputer, isRedComputer);
                break;
            case GameType.General:
                newGame = new GeneralGame(boardSize, isBlueComputer, isRedComputer);
                break;
        }

        return newGame;
    }

    // Clone method

    protected static GameBoard CloneGameBoard(GameBoard other)
    {
        GameBoard newGameBoard;

        switch (other.GetGameType())
        {
            default:
            case GameType.Simple:
                newGameBoard = new SimpleGame(other);
                break;
            case GameType.General:
                newGameBoard = new GeneralGame(other);
                break;
        }

        return newGameBoard;
    }

    // Dispose Function //
    public void Dispose()
    {
        OnBoardUpdateEvent = null;
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

    public bool IsCurrentPlayerComputer()
    {
        if (CurPlayerTurn == PlayerType.BlueLeft)
            return Blue.IsComputer;
        if (CurPlayerTurn == PlayerType.RedRight)
            return Red.IsComputer;
        return false;
    }

    private TileType getRandomTileType()
    {
        // Randomly choose the tile either S(1) or O(2)
        return (TileType)random.Next(1, 3);
    }

    private List<Position> getEmptyTiles()
    {
        List<Position> emptyTiles = [];
        for (int i = 0; i < size; i++)
        for (int j = 0; j < size; j++)
            if (board[i][j] == TileType.None)
                emptyTiles.Add(new Position(i, j));

        return emptyTiles;
    }

    public void SubscribeToBoardChanges(OnGameBoardUpdate handler)
    {
        OnBoardUpdateEvent += handler;
    }

    // Business Functions //

    public bool StartGame()
    {
        if (!IsGameStarted)
        {
            IsGameStarted = true;
            tickComputerPlayers();
        }
        else
        {
            throw new InvalidOperationException("Game already started");
        }

        return IsGameStarted;
    }

    public bool PlaceTile(int row, int column, TileType tileType)
    {
        if (IsGameStarted)
        {
            var placingPlayerTurn = CurPlayerTurn;
            
            var didPlace = placeTile(row, column, tileType, out Sos[] completedSosArray);
            if (didPlace)
            {
                OnBoardUpdateEvent?.Invoke(new TurnResult(new Move(tileType, new Position(row, column)), completedSosArray, placingPlayerTurn));
                tickComputerPlayers();
            }
            return didPlace;
        }
        else
            throw new InvalidOperationException("Game not started. Start the game first!");
    }

    private bool placeTile(int row, int column, TileType tileType, out Sos[] completedSosArray)
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
            turnRecord.Add(new Turn(CurPlayerTurn, new Position(row, column), tileType));

            completedSosArray = checkSos(row, column);
            foreach (var (s1, o, s2) in completedSosArray)
                Console.WriteLine($"SOS created for {tileType} at ({s1}, {o}, {s2})");


            // If one or more SOSes were made
            if (completedSosArray.Length > 0)
            {
                // Add score
                switch (CurPlayerTurn)
                {
                    case PlayerType.BlueLeft:
                        Blue.Score += completedSosArray.Length;
                        break;
                    case PlayerType.RedRight:
                        Red.Score += completedSosArray.Length;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unknown player turn: " + CurPlayerTurn);
                }
            }
            else // No SOSes were made
            {
                // Change turn
                CurPlayerTurn = CurPlayerTurn switch
                {
                    PlayerType.BlueLeft => PlayerType.RedRight,
                    PlayerType.RedRight => PlayerType.BlueLeft,
                    _ => throw new ArgumentOutOfRangeException("Unknown player turn: " + CurPlayerTurn)
                };
            }

            // Tile was placed
            return true;
        }

        // Unable to place tile
        completedSosArray = [];
        return false;
    }

    private Sos[] checkSos(int row, int column) => checkSos(row, column, board[row][column]);

    private Sos[] checkSos(int row, int column, TileType placedTile)
    {
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

    // Call to have computer players make their moves
    private void tickComputerPlayers()
    {
        while (IsCurrentPlayerComputer() && !IsGameOver())
        {
            var turn = makeComputerMove();
            OnBoardUpdateEvent?.Invoke(turn);
        }
    }

    private TurnResult makeComputerMove()
    {
        var placingPlayerTurn = CurPlayerTurn;
        
        // Read AC 8 for the logic to implement
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
                tileType = getRandomTileType();

                success = placeTile(row, column, tileType, out completedSosArray);

                Debug.Assert(success, "Failed to place tile?");
            } while (!success);

            return new TurnResult(new Move(tileType, new Position(row, column)), completedSosArray, placingPlayerTurn);
        }

        // -- Explore first ply of moves
        var moves = new PriorityQueue<Move, int>(new IntPickMaxCompare());

        // Find empty tiles
        List<Position> emptyTiles = getEmptyTiles();

        // For every empty tile calculate number of SOSes generated by S and O tile placement.
        foreach (Position emptyTile in emptyTiles)
        {
            // Save number of SOSes completed to moves with priority based on number of SOSes made
            moves.Enqueue(new Move(TileType.S, emptyTile),
                checkSos(emptyTile.row, emptyTile.column, TileType.S).Length);
            moves.Enqueue(new Move(TileType.O, emptyTile),
                checkSos(emptyTile.row, emptyTile.column, TileType.O).Length);
        }


        /*
         * AC 8.2 Computer makes an SOS
         * Given: An ongoing game
         * When: It is the computer’s turn
         * And: A valid SOS can be completed
         * Then: The computer should place the tile needed to complete the SOS
         */
        if (moves.TryPeek(out var bestMove, out var sosCount) && sosCount > 0)
        {
            var success = placeTile(bestMove.Position.row, bestMove.Position.column, bestMove.Tile,
                out var completedSosArray);
            Debug.Assert(success, "Failed to place tile?");
            return new TurnResult(bestMove, completedSosArray, placingPlayerTurn);
        }

        /*
         * AC 8.3 Computer makes a blocking move
         * Given: An ongoing game
         * When: It is the computer’s turn
         * And: There are no SOSes to complete
         * Then: The computer should attempt to make a “blocking” move so that the human cannot score a point next turn if possible.
         */
        // Search a second ply for blocking moves
        // If we reach moves that give the other player a score we can move on.
        // For the second ply of moves we want a min-heap since this our opponents score.
        // (Priority queue is a min-heap by default.)
        List<Move> zeroSumMoves = [];
        while (moves.TryDequeue(out bestMove, out sosCount) && sosCount == 0)
        {
            var nextState = CloneGameBoard(this);
            nextState.placeTile(bestMove.Position.row, bestMove.Position.column, bestMove.Tile, out _);

            bool invalidateMove = false;

            // Get empty tiles in next state
            // And search for blocking moves (moves that won't score a point for the other player)
            foreach (Position emptyTile in nextState.getEmptyTiles())
            {
                var opponentSosCount = nextState.checkSos(emptyTile.row, emptyTile.column, TileType.S).Length;
                if (opponentSosCount > 0)
                {
                    invalidateMove = true;
                    break;
                }
                
                opponentSosCount = nextState.checkSos(emptyTile.row, emptyTile.column, TileType.O).Length;
                if (opponentSosCount > 0)
                {
                    invalidateMove = true;
                    break;
                }
            }

            // Save move only if opponent cannot make an SOS for this move
            if (!invalidateMove)
                zeroSumMoves.Add(bestMove);                
        }

        // Randomly pick a move from equivalent zero sum moves
        if (zeroSumMoves.Count > 0)
        {
            bestMove = zeroSumMoves[random.Next(zeroSumMoves.Count)];
            var placeTileResult = placeTile(bestMove.Position.row, bestMove.Position.column, bestMove.Tile,
                out Sos[] resultSosArray);
            if (placeTileResult)
                return new TurnResult(bestMove, resultSosArray, placingPlayerTurn);
            Debug.Assert(placeTileResult, "Failed to place tile?");
        }


        /*
         * AC 8.4 Computer makes a random Move
         * Given: And ongoing game
         * When: The computer cannot make an SOS nor make a blocking move.
         * Then: The computer will make a random valid move.
         */
        Debug.WriteLine("Had to make a random move.");
        var randomTileIndex = random.Next(0, emptyTiles.Count);
        var randPos = emptyTiles[randomTileIndex];
        var randTile = getRandomTileType();
        var didPlaceTile = placeTile(randPos.row, randPos.column, randTile, out Sos[] finalSosArray);
        Debug.Assert(didPlaceTile, "Failed to place tile?");
        return new TurnResult(new Move(randTile, randPos), finalSosArray, placingPlayerTurn);
    }
}