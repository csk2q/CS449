using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SOS_Game;
using SOS_Game.Logic;
using static UnitTestProject.TestHelper;

namespace UnitTestProject;

public class Sprint4UnitTests
{
    /*
     * AC 8.1 Computer takes the first turn
     * Given: A new game is started
     * And: The first turn is the computer’s
     * When: The first turn of the game is the computer’s
     * Then: Place a tile of either type at random on the board.
     */
    [AvaloniaTheory]
    [InlineData(GameType.Simple)]
    [InlineData(GameType.General)]
    void StartingRandomPlacementTest(GameType gameType)
    {
        // Set up the window
        var window = new MainWindow();
        window.Show();


        // Set game properties
        SetGameMode(gameType, window);
        SetIsComputerRadioButtons(PlayerType.BlueLeft, true, window);

        // Start the game
        window.StartNewGame(null, new RoutedEventArgs());

        // Get the displayed game board
        var gameBoardGrid = window.FindControl<UniformGrid>("GameBoardGrid");
        Assert.NotNull(gameBoardGrid);
        Assert.NotEmpty(gameBoardGrid.Children);

        // Wait for UI to update
        Dispatcher.UIThread.RunJobs();


        // Count number of tiles with a letter
        int nonEmptyTileCount = 0;
        foreach (Control control in gameBoardGrid.Children)
        {
            if (control is Button button)
            {
                var buttonContent = button.Content;
                if (!string.IsNullOrEmpty((string?)buttonContent))
                    nonEmptyTileCount++;
            }
        }

        // Verify that there is a single tile placed on the board
        Assert.Equal(1, nonEmptyTileCount);
    }


    /*
     * AC 8.2 Computer makes an SOS
     * Given: An ongoing game
     * When: It is the computer’s turn
     * And: A valid SOS can be completed
     * Then: The computer should place the tile needed to complete the SOS
     */
    [AvaloniaTheory]
    [InlineData(GameType.Simple)]
    [InlineData(GameType.General)]
    void ComputerMakeSosTest(GameType gameType)
    {
        // Set up the window
        var window = new MainWindow();
        window.Show();

        // Set game properties
        SetGameMode(gameType, window);
        SetTileChoice(PlayerType.BlueLeft, TileType.S, window);
        SetTileChoice(PlayerType.RedRight, TileType.O, window);

        // Start the game
        window.StartNewGame(null, new RoutedEventArgs());

        // Get the displayed game board
        var gameBoardGrid = window.FindControl<UniformGrid>("GameBoardGrid");
        Assert.NotNull(gameBoardGrid);
        Assert.NotEmpty(gameBoardGrid.Children);

        // Get the game board for this game
        var gameBoardInfo = typeof(MainWindow).GetField("gameBoard", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(gameBoardInfo);
        var gameBoard = (GameBoard?)gameBoardInfo.GetValue(window);
        Assert.NotNull(gameBoard);

        // Blue player place an S
        gameBoardGrid.Children[0].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        // Set blue player to be a computer player
        SetIsComputerGameBoard(PlayerType.BlueLeft, true, gameBoard);

        // Red player place an O
        gameBoardGrid.Children[1].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        // Check that an SOS was made by the blue computer player
        Assert.Equal(1, gameBoard.Blue.Score);
    }


    /*
     * AC 8.3 Computer avoids giving opponent SOS opportunities
     * Given: An ongoing game
     * When: It is the computer’s turn
     * And: There are no SOSes to complete
     * Then: The computer should avoid moves that allow its opponent to score a point on their next turn.
     */
    // Make a human placement of an O in the center of a 3x3 and check if any other tiles have an S in them
    [AvaloniaTheory]
    [InlineData(GameType.Simple)]
    [InlineData(GameType.General)]
    void AvoidComputerGivingSoses(GameType gameType)
    {
        // Set up the window
        var window = new MainWindow();
        window.Show();

        // Run multiple times to account for randomness
        for (int runIndex = 100; runIndex >= 0; runIndex--)
        {
            // Set up the game
            SetGameMode(gameType, window);
            SetIsComputerRadioButtons(PlayerType.RedRight, true, window);
            SetTileChoice(PlayerType.BlueLeft, TileType.O, window);
            window.StartNewGame(null, new RoutedEventArgs());

            // Place O on center tile by blue human
            var gameBoardGrid = window.FindControl<UniformGrid>("GameBoardGrid");
            Assert.NotNull(gameBoardGrid);
            int centerTileIndex = 4;
            var centerTile = gameBoardGrid.Children[centerTileIndex] as Button;
            Assert.NotNull(centerTile);
            centerTile.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));


            // Verify that the computer player did not place an S 
            for (int i = 0; i < gameBoardGrid.Children.Count; i++)
            {
                if (i != centerTileIndex)
                {
                    var buttonContent = (string?)((Button)gameBoardGrid.Children[i]).Content;
                    if (!string.IsNullOrEmpty(buttonContent))
                        Assert.NotEqual("S", buttonContent);
                }
            }
        }
    }


    /*
     * AC 8.4 Computer makes a random move
     * Given: And ongoing game
     * When: The computer cannot make an SOS nor make a blocking move.
     * Then: The computer will make a random valid move.
     */
    // Note: A random move only happens when all moves allow the opponent to create an SOS 
    //      This can only happen on boards with a size greater than three.
    [AvaloniaTheory]
    [InlineData(GameType.Simple)]
    [InlineData(GameType.General)]
    void ComputerRandomMove(GameType gameType)
    {
        // Set up the window
        var window = new MainWindow();
        window.Show();

        // Set up the game
        SetGameMode(gameType, window);
        SetBoardSize(4, window);
        window.StartNewGame(null, new RoutedEventArgs());

        // Get the gameBoard
        var gameBoardInfo = typeof(MainWindow).GetField("gameBoard", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(gameBoardInfo);
        var gameBoard = (GameBoard?)gameBoardInfo.GetValue(window);
        Assert.NotNull(gameBoard);

        // Get the GameBoardGrid
        var gameBoardGrid = window.FindControl<UniformGrid>("GameBoardGrid");
        Assert.NotNull(gameBoardGrid);

        // Get the getTile method
        var getTileInfo = typeof(MainWindow).GetMethod("getTile", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(getTileInfo);

        Turn[] turns =
        [
            new Turn(PlayerType.BlueLeft, new Position(0, 2), TileType.O),
            new Turn(PlayerType.RedRight, new Position(3, 3), TileType.S),
            new Turn(PlayerType.BlueLeft, new Position(0, 0), TileType.S),
            new Turn(PlayerType.RedRight, new Position(2, 1), TileType.O),
            new Turn(PlayerType.BlueLeft, new Position(3, 0), TileType.O),
            new Turn(PlayerType.RedRight, new Position(2, 0), TileType.O),
            new Turn(PlayerType.BlueLeft, new Position(0, 3), TileType.O),
            new Turn(PlayerType.RedRight, new Position(1, 0), TileType.O),
            new Turn(PlayerType.BlueLeft, new Position(1, 2), TileType.S),
            new Turn(PlayerType.RedRight, new Position(1, 3), TileType.O),
            new Turn(PlayerType.BlueLeft, new Position(3, 1), TileType.O),
            new Turn(PlayerType.RedRight, new Position(0, 1), TileType.S),
            new Turn(PlayerType.BlueLeft, new Position(3, 2), TileType.O),
        ];
        var finalTurn = new Turn(PlayerType.RedRight, new Position(2, 3), TileType.S);

        // Place tiles
        foreach (var turn in turns)
        {
            SetTileChoice(turn.Player, turn.TileType, window);
            var curButton = (Button?)getTileInfo.Invoke(window, [turn.Position.row, turn.Position.column]);
            Assert.NotNull(curButton);
            curButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        SetIsComputerGameBoard(PlayerType.BlueLeft, true, gameBoard);
        SetIsComputerGameBoard(PlayerType.RedRight, true, gameBoard);

        Assert.Equal(0, gameBoard.Blue.Score);
        Assert.Equal(0, gameBoard.Red.Score);

        // Place final tile as Red to force a random move by the Blue computer
        SetTileChoice(finalTurn.Player, finalTurn.TileType, window);
        var button = (Button?)getTileInfo.Invoke(window, [finalTurn.Position.row, finalTurn.Position.column]);
        Assert.NotNull(button);
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(0, gameBoard.Blue.Score);
        Assert.Equal(1, gameBoard.Red.Score);
        Assert.Equal(PlayerType.RedRight, gameBoard.GetWinner());
    }
}