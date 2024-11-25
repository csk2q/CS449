using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SOS_Game;
using SOS_Game.Logic;
using static UnitTestProject.TestHelper;

namespace UnitTestProject;

public class Sprint3UnitTests
{
    // Helper functions //

    // Moved to TestHelper.cs


    // Unit Tests //

    /*
     * AC 4.3 Mark SOS sequences
       Given: A tile is placed
       When: A SOS sequence is created vertically or horizontally or diagonally
       Then: Mark the SOS sequence with a colored line with the color of the player who placed the tile
       And: Give the player who placed the tile a point

     * AC 6.3 Mark SOS sequences
       Given: A tile is placed
       When: A SOS sequence is created vertically or horizontally or diagonally.
       Then: Mark the SOS sequence with a colored line with the color of the player who placed the tile.
       And: Give the player who placed the tile a point.
     */
    [AvaloniaTheory]
    [InlineData(GameType.Simple)]
    [InlineData(GameType.General)]
    public void MarkCompletedSoSesTest(GameType gameType)
    {
        // Set up the window
        var window = new MainWindow();
        window.Show();


        // Set game mode and start game
        SetGameMode(gameType, window);
        window.StartNewGame(null, new RoutedEventArgs());

        // Get initial state of BoardCanvas
        var boardCanvas = window.FindControl<Canvas>("BoardCanvas");
        Assert.NotNull(boardCanvas);
        Assert.Empty(boardCanvas.Children);

        // Set Red tile choice to O
        SetTileChoice(PlayerType.RedRight, TileType.O, window);

        // Make an SOS
        var gameBoardGrid = window.FindControl<UniformGrid>("GameBoardGrid");
        Assert.NotNull(gameBoardGrid);

        foreach (var i in new[] { 0, 1, 2 })
            ((Button)gameBoardGrid.Children[i]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));


        // Check blue score to verify an SOS was made
        var gameBoardFieldInfo =
            typeof(MainWindow).GetField("gameBoard", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(gameBoardFieldInfo);
        var gameBoard = (GameBoard?)gameBoardFieldInfo.GetValue(window);
        Assert.NotNull(gameBoard);
        Assert.Equal(1, gameBoard.Blue.Score);
        
        // Wait for UI to update
        Dispatcher.UIThread.RunJobs();

        // Check that a line was added to BoardCanvas
        Assert.Single(boardCanvas.Children);
        Assert.IsType<Line>(boardCanvas.Children[0]);

        // Check color is correct
        Assert.Equal(Brushes.DarkBlue, ((Line)boardCanvas.Children[0]).Stroke);
    }

    /*
     * AC 5.2 Win for the Red Player (Simple game)
       Given: It is the Red player’s turn
       When: A SOS sequence is completed
       Then: The game is over
       And: Lock the board from further moves
       And: Declare the Red Player the winner
     */
    [AvaloniaFact]
    public void RedWinSimpleGameTest()
    {
        // Set up the window
        var window = new MainWindow();
        window.Show();

        // Set game mode and start game
        SetGameMode(GameType.Simple, window);
        window.StartNewGame(null, new RoutedEventArgs());

        // Set Red tile choice to O
        SetTileChoice(PlayerType.RedRight, TileType.O, window);

        // Make an SOS
        var gameBoardGrid = window.FindControl<UniformGrid>("GameBoardGrid");
        Assert.NotNull(gameBoardGrid);
        foreach (var i in new[] { 0, 4, 2, 1 })
            ((Button)gameBoardGrid.Children[i]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));


        // Check red score to verify an SOS was made
        var gameBoardFieldInfo =
            typeof(MainWindow).GetField("gameBoard", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(gameBoardFieldInfo);
        var gameBoard = (GameBoard?)gameBoardFieldInfo.GetValue(window);
        Assert.NotNull(gameBoard);
        Assert.Equal(1, gameBoard.Red.Score);

        // Verify game is over
        Assert.True(gameBoard.IsGameOver());

        // Verify winner is Red
        Assert.Equal(PlayerType.RedRight, gameBoard.GetWinner());

        // Test board lock
        var middleLeftButton = gameBoardGrid.Children[3] as Button;
        Assert.NotNull(middleLeftButton);
        var originalContents = middleLeftButton.Content as string;
        Assert.True(string.IsNullOrEmpty(originalContents));
        middleLeftButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(originalContents, middleLeftButton.Content as string);
        
        // Wait for UI to update
        Dispatcher.UIThread.RunJobs();

        // Check winner display is rendered
        var winnerDisplay = window.FindControl<StackPanel>("WinnerDisplay");
        Assert.NotNull(winnerDisplay);
        Assert.True(winnerDisplay.IsVisible);

        // Check that blue is declared the winner
        var winnerNameText = window.FindControl<TextBlock>("WinnerNameText");
        Assert.NotNull(winnerNameText);
        Assert.Contains("Red", winnerNameText.Text);
    }

    /*
     * AC 7.1 Win for Blue player (General Game)
       Given: The board is full
       When: The S player has completed more SOS sequences than the Red Player
       Then: The game is over
       And: Lock the board from further moves
       And: Declare the Blue player the winner
     */
    [AvaloniaFact]
    public void BlueWinGeneralGameTest()
    {
        // Set up the window
        var window = new MainWindow();
        window.Show();

        // Set game mode and start game
        SetGameMode(GameType.General, window);
        window.StartNewGame(null, new RoutedEventArgs());

        // Set Red tile choice to O
        SetTileChoice(PlayerType.RedRight, TileType.O, window);

        // Make an SOS
        var gameBoardGrid = window.FindControl<UniformGrid>("GameBoardGrid");
        Assert.NotNull(gameBoardGrid);
        foreach (var i in new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 })
            ((Button)gameBoardGrid.Children[i]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var gameBoardFieldInfo =
            typeof(MainWindow).GetField("gameBoard", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(gameBoardFieldInfo);
        var gameBoard = (GameBoard?)gameBoardFieldInfo.GetValue(window);
        Assert.NotNull(gameBoard);

        // Verify game is over and blue won
        Assert.True(gameBoard.IsGameOver());
        Assert.Equal(PlayerType.BlueLeft, gameBoard.GetWinner());

        // Check blue score to verify an SOS was made
        Assert.Equal(3, gameBoard.Blue.Score);

        // Wait for UI to update
        Dispatcher.UIThread.RunJobs();

        // Test board lock
        var middleLeftButton = gameBoardGrid.Children[3] as Button;
        Assert.NotNull(middleLeftButton);
        var originalTileContents = middleLeftButton.Content as string;
        Assert.False(string.IsNullOrEmpty(originalTileContents));
        middleLeftButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(originalTileContents, middleLeftButton.Content as string);

        // Check winner display is rendered
        var winnerDisplay = window.FindControl<StackPanel>("WinnerDisplay");
        Assert.NotNull(winnerDisplay);
        Assert.True(winnerDisplay.IsVisible);

        // Check that blue is declared the winner
        var winnerNameText = window.FindControl<TextBlock>("WinnerNameText");
        Assert.NotNull(winnerNameText);
        Assert.Contains("blue", winnerNameText.Text?.ToLower() ?? "");
    }

    /*
     * AC 5.3 Draw (Simple game)
       Given: It is a new turn
       When: The board is full
       Then: The game is over
       And: Lock the board from further moves
       And: Declare a Draw
     * AC 7.3 Draw (General Game)
       Given: The board is full
       When: Player scores are equal
       Then: The game is over
       And: Lock the board from further moves
       And: Declare a Draw
     */
    [AvaloniaTheory]
    [InlineData(GameType.Simple)]
    [InlineData(GameType.General)]
    public void DrawNoWinnerTest(GameType gameType)
    {
        // Set up the window
        var window = new MainWindow();
        window.Show();

        // Set game mode and start game
        SetGameMode(gameType, window);
        window.StartNewGame(null, new RoutedEventArgs());

        // Fill the board with S tiles
        var gameBoardGrid = window.FindControl<UniformGrid>("GameBoardGrid");
        Assert.NotNull(gameBoardGrid);
        foreach (var tile in gameBoardGrid.Children)
            ((Button)tile).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        // Get gameBoard
        var gameBoardFieldInfo =
            typeof(MainWindow).GetField("gameBoard", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(gameBoardFieldInfo);
        var gameBoard = (GameBoard?)gameBoardFieldInfo.GetValue(window);
        Assert.NotNull(gameBoard);

        // Verify game is over
        Assert.True(gameBoard.IsGameOver());

        // Verify the winner is no one
        Assert.Equal(PlayerType.None, gameBoard.GetWinner());
        
        // Wait for UI to update
        Dispatcher.UIThread.RunJobs();

        // Test board lock
        var middleLeftButton = gameBoardGrid.Children[3] as Button;
        Assert.NotNull(middleLeftButton);
        var originalTileContents = middleLeftButton.Content as string;
        Assert.False(string.IsNullOrEmpty(originalTileContents));
        middleLeftButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(originalTileContents, middleLeftButton.Content as string);

        // Check winner display is rendered
        var winnerDisplay = window.FindControl<StackPanel>("WinnerDisplay");
        Assert.NotNull(winnerDisplay);
        Assert.True(winnerDisplay.IsVisible);

        // Check that blue is declared the winner
        var winnerNameText = window.FindControl<TextBlock>("WinnerNameText");
        Assert.NotNull(winnerNameText);
        Assert.DoesNotContain("blue", winnerNameText.Text?.ToLower() ?? "");
        Assert.DoesNotContain("red", winnerNameText.Text?.ToLower() ?? "");
    }


    // ChatGPT assisted unit tests //

    // Initially generated by ChatGPT for AC 5.1 , edited by me
    // Formerly called WinForSPlayer_SimpleGame_EndsGameAndLocksBoard by ChatGPT
    [AvaloniaFact]
    public void BlueWinSimpleGameTest()
    {
        // Step 1: Instantiate and set up the game window
        var window = new MainWindow();
        window.Show();

        // Start a new game
        window.StartNewGame(null, new RoutedEventArgs());

        // Step 2: Set up player choice using reflection
        var redSChoice =
            typeof(MainWindow).GetField("RedSChoice",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(window) as RadioButton;
        var redOChoice =
            typeof(MainWindow).GetField("RedOChoice",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(window) as RadioButton;

        Assert.NotNull(redSChoice);
        Assert.NotNull(redOChoice);

        // Set RedSChoice to false and RedOChoice to true
        redSChoice.IsChecked = false;
        redOChoice.IsChecked = true;

        // Step 3: Access the game board grid and get the buttons using reflection
        var gameBoardGrid = window.FindControl<UniformGrid>("GameBoardGrid");
        Assert.NotNull(gameBoardGrid);

        // Step 4: Simulate button clicks to form "SOS"
        var buttons = gameBoardGrid.Children.OfType<Button>().ToArray();
        Assert.True(buttons.Length > 3); // Ensure we have enough buttons to click

        // Click the buttons in the order 0, 3, 2, 1
        // Trigger click events for each button to simulate moves (assume these events set the Content)
        foreach (var button in new[] { buttons[0], buttons[3], buttons[2], buttons[1] })
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        // Step 5: Access gameBoard via reflection and check game state
        var gameBoardField = typeof(MainWindow).GetField("gameBoard",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(gameBoardField);
        var gameBoard = gameBoardField.GetValue(window);
        Assert.NotNull(gameBoard);

        // Check if the game is over
        var isGameOverMethod = gameBoard.GetType().GetMethod("IsGameOver",
            BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(isGameOverMethod);
        var isGameOver = (bool?)isGameOverMethod.Invoke(gameBoard, null);
        Assert.NotNull(isGameOver);
        Assert.True(isGameOver);

        // Check the winner
        var getWinnerMethod = gameBoard.GetType().GetMethod("GetWinner",
            BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(getWinnerMethod);
        var winner = getWinnerMethod.Invoke(gameBoard, null);
        Assert.Equal(PlayerType.RedRight, winner);

        // Check the game type
        var getGameTypeMethod = gameBoard.GetType().GetMethod("GetGameType",
            BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(getGameTypeMethod);
        var gameType = getGameTypeMethod.Invoke(gameBoard, null);
        Assert.Equal(GameType.Simple, gameType);

        // Additional Check: Ensure that all further moves are locked (i.e., clicking the button won't change the content)
        var initialContent = buttons[4].Content;
        buttons[4].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(initialContent, buttons[4].Content);
        
        // Wait for UI to update
        Dispatcher.UIThread.RunJobs();

        // Check that the winner is shown
        var winnerDisplayInfo = window.GetType().GetField("WinnerDisplay",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(winnerDisplayInfo);
        var winnerDisplay = (StackPanel?)winnerDisplayInfo.GetValue(window);
        Assert.NotNull(winnerDisplay);
        Assert.True(winnerDisplay.IsVisible);
    }

    // Initially generated by ChatGPT for AC 7.2 , edited by me
    // Formerly called WinForRedPlayer_GeneralGame by ChatGPT
    [AvaloniaFact]
    public void RedWinGeneralGame()
    {
        // Step 1: Instantiate and set up the game window
        var window = new MainWindow();
        window.Show();

        // Step 2: Set the game mode to GeneralGame using reflection
        var simpleGameRadioButton = typeof(MainWindow).GetField("SimpleGameRadioButton",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(window) as RadioButton;
        var generalGameRadioButton = typeof(MainWindow).GetField("GeneralGameRadioButton",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(window) as RadioButton;

        Assert.NotNull(simpleGameRadioButton);
        Assert.NotNull(generalGameRadioButton);

        // Set the General game mode
        simpleGameRadioButton.IsChecked = false;
        generalGameRadioButton.IsChecked = true;

        // Start a new game
        window.StartNewGame(null, new RoutedEventArgs());

        // Step 3: Set up player choices using reflection
        var redSChoice =
            typeof(MainWindow).GetField("RedSChoice",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(window) as RadioButton;
        var redOChoice =
            typeof(MainWindow).GetField("RedOChoice",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(window) as RadioButton;
        var blueSChoice =
            typeof(MainWindow).GetField("BlueSChoice",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(window) as RadioButton;
        var blueOChoice =
            typeof(MainWindow).GetField("BlueOChoice",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(window) as RadioButton;

        Assert.NotNull(redSChoice);
        Assert.NotNull(redOChoice);
        Assert.NotNull(blueSChoice);
        Assert.NotNull(blueOChoice);

        // Set choices for Red and Blue players
        redSChoice.IsChecked = true;
        redOChoice.IsChecked = false;
        blueSChoice.IsChecked = false;
        blueOChoice.IsChecked = true;

        // Step 4: Access the game board grid and get the buttons using reflection
        var gameBoardGrid = window.FindControl<UniformGrid>("GameBoardGrid");
        Assert.NotNull(gameBoardGrid);

        // Step 5: Simulate button clicks to form a full board with Red having more sequences
        var buttons = gameBoardGrid.Children.OfType<Button>().ToArray();
        Assert.True(buttons.Length >= 9, "Not enough buttons on the grid for the test.");

        // Click the buttons in the order 1, 0, 4, 2, 3, 7, 5, 6, 8
        foreach (var button in new[]
                 {
                     buttons[1], buttons[0], buttons[4], buttons[2], buttons[3], buttons[7], buttons[5], buttons[6],
                     buttons[8]
                 })
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        // Step 6: Access gameBoard via reflection and check game state
        var gameBoardField = typeof(MainWindow).GetField("gameBoard",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(gameBoardField);
        var gameBoard = gameBoardField.GetValue(window);
        Assert.NotNull(gameBoard);

        // Check if the game is over
        var isGameOverMethod = gameBoard.GetType().GetMethod("IsGameOver",
            BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(isGameOverMethod);
        var isGameOver = (bool?)isGameOverMethod.Invoke(gameBoard, null);
        Assert.True(isGameOver);

        // Check the winner
        var getWinnerMethod = gameBoard.GetType().GetMethod("GetWinner",
            BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(getWinnerMethod);
        var winner = getWinnerMethod.Invoke(gameBoard, null);
        Assert.Equal(PlayerType.RedRight,
            winner); // Assuming "RedRight" is the correct representation of the Red player

        // Check the game type
        var getGameTypeMethod = gameBoard.GetType().GetMethod("GetGameType",
            BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(getGameTypeMethod);
        var gameType = getGameTypeMethod.Invoke(gameBoard, null);
        Assert.Equal(GameType.General, gameType);
        
        Dispatcher.UIThread.RunJobs(); // Wait for UI to update

        // Additional Check: Ensure that all further moves are locked
        var initialContent = buttons[0].Content;
        buttons[0].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs(); // Wait for UI to update
        Assert.Equal(initialContent, buttons[0].Content);

        // Check that the winner is shown
        var winnerDisplayInfo = window.GetType().GetField("WinnerDisplay",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(winnerDisplayInfo);
        var winnerDisplay = (StackPanel?)winnerDisplayInfo.GetValue(window);
        Assert.NotNull(winnerDisplay);
        Assert.True(winnerDisplay.IsVisible);
    }
}