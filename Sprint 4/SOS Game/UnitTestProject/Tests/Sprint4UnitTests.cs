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
        var gameBoardInfo =  typeof(MainWindow).GetField("gameBoard", BindingFlags.NonPublic | BindingFlags.Instance);
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


    /*
     * AC 8.4 Computer makes a random move
     * Given: And ongoing game
     * When: The computer cannot make an SOS nor make a blocking move.
     * Then: The computer will make a random valid move.
     */
}