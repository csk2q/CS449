using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using SOS_Game;
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
        var gameBoardGrid = window.FindControl<Canvas>("GameBoardGrid");
        Assert.NotNull(gameBoardGrid);
        Assert.NotEmpty(gameBoardGrid.Children);


        // Count number of tiles with a letter
        int nonEmptyTileCount = 0;
        foreach (Control control in gameBoardGrid.Children)
        {
            if (control is Button button)
            {
                var buttonContent = button.Content;
                if (string.IsNullOrEmpty((string?)buttonContent))
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


    /*
     * AC 8.3 Computer makes a blocking move
     * Given: An ongoing game
     * When: It is the computer’s turn
     * And: There are no SOSes to complete
     * Then: The computer should attempt to make a “blocking” move so that the human cannot score a point next turn if possible.
     */


    /*
     * AC 8.4 Computer makes a random Move
     * Given: And ongoing game
     * When: The computer cannot make an SOS nor make a blocking move.
     * Then: The computer will make a random valid move.
     */
}