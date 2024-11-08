using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SOS_Game.Logic;
using Timer = System.Timers.Timer;

namespace SOS_Game;

public partial class MainWindow : Window
{
    // Note: Several functions & variables have been made public to allow for unit testing

    // Variables //

    private int currentBoardSize;
    private GameBoard gameBoard = new SimpleGame(0, false, false);

    
    // Constructor //
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        currentBoardSize = getBoardSizeInput();
    }

    // Getters & Setters //

    private int getBoardSizeInput()
    {
        int value = (int)Math.Clamp(Convert.ToInt32(BoardSizeNumericUpDown.Value), GameBoard.MinBoardSize,
            GameBoard.MaxBoardSize);

        //Override displayed value to show used value.
        BoardSizeNumericUpDown.Value = value;

        return value;
    }

    // Returns a new tile for use on the board
    private Button getNewTile(TileType tileType)
    {
        // tileElement for reuse in the board
        var button = new Button
        {
            Content = "", // Button content
            // FontSize = 35, // Font size of the button text
            Background = Brushes.Gray, // Background color of the button
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            CornerRadius = new CornerRadius(0), // No rounded corners
            Padding = new Thickness(0), // No padding
            Margin = new Thickness(0), // No margin
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Black,
        };

        //Click callback
        button.Click += placeTile;

        // Set letter of tile
        if (tileType != TileType.None)
        {
            button.Content = Enum.GetName(tileType);
        }

        return button;
    }

    private void updateTurnText()
    {
        if (gameBoard.IsGameOver())
        {
            TurnDisplay.IsVisible = false;
            WinnerDisplay.IsVisible = true;

            switch (gameBoard.GetWinner())
            {
                // Draw
                case PlayerType.None:
                    WinnerNameText.Text = "None (Draw)";
                    WinnerNameText.Foreground = Brushes.MediumPurple;
                    break;
                // Blue win
                case PlayerType.BlueLeft:
                    WinnerNameText.Text = "Blue!";
                    WinnerNameText.Foreground = Brushes.Blue;
                    break;
                // Red win
                case PlayerType.RedRight:
                    WinnerNameText.Text = "Red!";
                    WinnerNameText.Foreground = Brushes.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            TurnDisplay.IsVisible = true;
            WinnerDisplay.IsVisible = false;

            if (gameBoard.CurPlayerTurn == PlayerType.BlueLeft)
            {
                //BlueLeft's turn
                TurnTextBlock.Text = "Blue's Turn";
                TurnTextBlock.Foreground = Brushes.Blue;
            }
            else if (gameBoard.CurPlayerTurn == PlayerType.RedRight)
            {
                //RedRight's turn
                TurnTextBlock.Text = "Red's Turn";
                TurnTextBlock.Foreground = Brushes.Red;
            }
        }
    }

    private void updateScoreText()
    {
        BlueScore.Text = gameBoard.Blue.Score.ToString();
        RedScore.Text = gameBoard.Red.Score.ToString();
    }

    private Button? getTile(int row, int column)
    {
        // Try fast path
        // Position on board is based on row(i / boardSize) and column(i % boardSize)
        // Reverse to solve for i from row and column
        int i = row * currentBoardSize + column;
        if (GameBoardGrid.Children[i] is Button tile
            && Grid.GetRow(tile) == row
            && Grid.GetColumn(tile) == column)
            return tile;

        // Fallback to iterating over all elements
        foreach (var control in GameBoardGrid.Children)
        {
            if (control is Button curTile)
            {
                var tileRow = Grid.GetRow(curTile);
                var tileCol = Grid.GetColumn(curTile);

                if (tileRow == row && tileCol == column)
                {
                    Debug.WriteLine($"Slow path taken for tile: {row}, {column}");
                    return control as Button;
                }
            }
        }

        Debug.WriteLine($"Failed to find tile: {row}, {column}");
        return null;
    }

    private bool getIsComputer(PlayerType playerType)
    {
        bool result;
        switch (playerType)
        {
            case PlayerType.BlueLeft:
                result = BlueComputerRadioButton.IsChecked.Value;
                break;
            case PlayerType.RedRight:
                result = RedComputerRadioButton.IsChecked.Value;
                break;

            case PlayerType.None:
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType,
                    "Player type must be either Blue or Red.");
        }

        return result;
    }


    // Event handlers //

    private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var gridSize = Math.Min(GameBoarder.Bounds.Width, GameBoarder.Bounds.Height);
        GameBoardGrid.Width = GameBoardGrid.Height = gridSize;
    }

    private void GameBoardGrid_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var gridSize = Math.Min(GameBoarder.Bounds.Width, GameBoarder.Bounds.Height);

        foreach (var control in GameBoardGrid.Children)
        {
            if (control is Button tile)
            {
                tile.Width = tile.Height = gridSize / currentBoardSize;
            }
        }
    }
    
    private void onBoardUpdateHandler(TurnResult turn)
    {
        
        // Create a timer to delay execution for a few frames.
        // This to a workaround for a bug in Avalonia where attempting to obtain the bounds or location of an element
        // immediately after changing its content will cause all requests to return the default value 0,0.
        // See the note in markSos() for more details about the bug.
        Timer timer = new Timer();
        timer.Interval = 100; // in milliseconds of time
        timer.Elapsed += (sender, args) => Dispatcher.UIThread.Post(() =>
        {
            try
            {
                // Stop ticking during execution to prevent a second call
                timer.Stop();
                
                var completedSosArray = turn.SosMade;
                var button = getTile(turn.Move.Position.row, turn.Move.Position.column);

                Debug.Assert(button is not null, "button is null?");

                // Color letter based on player turn
                if (turn.placingPlayer == PlayerType.BlueLeft)
                    button.Foreground = Brushes.Blue;
                else
                    button.Foreground = Brushes.Red;

                // Set tile letter
                button.Content = Enum.GetName(turn.Move.Tile);

                // For every completed SOS
                foreach (var sos in completedSosArray)
                    markSos(sos);

                updateTurnText();
                updateScoreText();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                timer.Dispose();
            }
        });

        timer.Start();
    }


    // UI Logic //

    private void placeTile(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // Variables
            TileType tileSelection;
            var placingPlayer = gameBoard.CurPlayerTurn;

            // Get player choices
            if (placingPlayer == PlayerType.BlueLeft)
            {
                //BlueLeft's turn
                if (BlueSChoice.IsChecked ?? true)
                    tileSelection = TileType.S;
                else
                    tileSelection = TileType.O;
            }
            else if (placingPlayer == PlayerType.RedRight)
            {
                //RedRight's turn
                if (RedSChoice.IsChecked ?? true)
                    tileSelection = TileType.S;
                else
                    tileSelection = TileType.O;
            }
            else
                throw new ApplicationException($"Unknown Player turn! \"{placingPlayer}\" is not a valid player.");

            // Try place tile
            bool result = gameBoard.PlaceTile(Grid.GetRow(button), Grid.GetColumn(button), tileSelection);
        }
        else
            Debug.Assert(false, "ClickTile called but sender is not a button! Sender: " + sender);
    }

    public void StartNewGame(object? sender, RoutedEventArgs e)
    {
        // Get rid of old board
        gameBoard.Dispose();

        // Get input
        var boardSize = currentBoardSize = getBoardSizeInput();

        //Default game mode is simple
        GameType gameMode = (SimpleGameRadioButton.IsChecked ?? true) ? GameType.Simple : GameType.General;

        //Set up variables
        var newTiles = new List<Button>(boardSize);

        gameBoard = GameBoard.CreateNewGame(gameMode, boardSize, getIsComputer(PlayerType.BlueLeft),
            getIsComputer(PlayerType.RedRight));
        gameBoard.SubscribeToBoardChanges(onBoardUpdateHandler);

        //Generate new tiles
        for (int i = 0; i < Math.Pow(boardSize, 2); i++)
        {
            var tile = getNewTile(TileType.None);

            //Set position on board
            Grid.SetRow(tile, i / boardSize);
            Grid.SetColumn(tile, i % boardSize);

            // Set tile size and maintain aspect ratio
            var gridSize = Math.Min(GameBoarder.Bounds.Width, GameBoarder.Bounds.Height);
            var sideLength = gridSize / boardSize;
            tile.Width = tile.Height = sideLength;

            //Scale font size
            const double percentageOfTile = 0.80;
            tile.FontSize = sideLength * percentageOfTile;

            newTiles.Add(tile);
        }

        //Clear and update board UI
        GameBoardGrid.Children.Clear();
        GameBoardGrid.Children.AddRange(newTiles);

        //Clear lines from last game
        BoardCanvas.Children.Clear();

        updateTurnText();
        updateScoreText();

        // Show Avalonia bug warning if both players are computers
        AvaloniaBugWarning.IsVisible = gameBoard.Blue.IsComputer && gameBoard.Red.IsComputer;

        gameBoard.StartGame();
    }


    // Helper Functions //

    private void markSos(Sos sos)
    {
        var s1 = getTile(sos.S1.row, sos.S1.column);
        var o = getTile(sos.O.row, sos.O.column);
        var s2 = getTile(sos.S2.row, sos.S2.column);
        if (s1 is null || o is null || s2 is null)
        {
            Debug.WriteLine($"Failed get tiles at {sos.S1}, {sos.O}, {sos.S2}. Yet, they were a part of an SOS?");
            return;
        }

        // Note :
        //      The following is an attempt to make the lines render correctly.
        //      Avalonia does not render the lines in the correct place.
        //      It seems Avalonia adds a random offset depending on the order and type of tiles placed.
        //      Possibly also dependent on which player places the tiles or completes the SOS.

        // The values in Bounds does not seem to give correct results.
        // Using Bounds.Center gave even more erroneous values.
        // Control.TranslatePoint(new Point(0,0)) also gives erroneous values.
        // The best results I have obtained from using PointToScreen() into PointToClient()

        // I suspect at least part of the issue is that PointToScreen() and PointToClient() returns default if there is an error.
        // This results in a silent failure and incorrect values beinng returned to my code.
        // Source link https://github.com/AvaloniaUI/Avalonia/blob/9cecb90ba1e681d1783e3a89db4b8d860859550f/src/Avalonia.Native/TopLevelImpl.cs#L278
        // Related issue: https://github.com/AvaloniaUI/Avalonia/issues/16622


        // Get the position of the top-left corner of the tile
        var startPoint = s1.PointToScreen(new Point(0, 0));
        var centerPoint = o.PointToScreen(new Point(0, 0));
        var endPoint = s2.PointToScreen(new Point(0, 0));

        // Debug render: Put squares in the top left of each tile in the SOS
        if (false)
        {
            var size = 10;

            var circle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.LawnGreen),
                Width = size,
                Height = size,
                ZIndex = 999,
            };
            var localPoint = BoardCanvas.PointToClient(startPoint);
            Canvas.SetLeft(circle, localPoint.X);
            Canvas.SetTop(circle, localPoint.Y);
            BoardCanvas.Children.Add(circle);
            circle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.DeepPink),
                Width = size,
                Height = size,
                ZIndex = 999,
            };
            localPoint = BoardCanvas.PointToClient(centerPoint);
            Canvas.SetLeft(circle, localPoint.X);
            Canvas.SetTop(circle, localPoint.Y);
            BoardCanvas.Children.Add(circle);
            circle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.OrangeRed),
                Width = size,
                Height = size,
                ZIndex = 999,
            };
            localPoint = BoardCanvas.PointToClient(endPoint);
            Canvas.SetLeft(circle, localPoint.X);
            Canvas.SetTop(circle, localPoint.Y);
            BoardCanvas.Children.Add(circle);
        }

        // Calculate tile edge length
        var firstTilePoint = GameBoardGrid.Children[0].PointToScreen(new Point(0, 0));
        var secondTilePoint = GameBoardGrid.Children[1].PointToScreen(new Point(0, 0));
        var edgeLength = Math.Abs(firstTilePoint.X - secondTilePoint.X);

        // Move points to center of tile
        PixelPoint offset = new PixelPoint(edgeLength / 2, edgeLength / 2);
        startPoint += offset;
        centerPoint += offset;
        endPoint += offset;

        // Scale line up around center the SOS so lines don't start and end from the center of the S tiles
        // Desmos graph of the scaling math: https://www.desmos.com/calculator/aws7k4yil5
        var scalingFactor = 1.3;
        var startPointDirection = startPoint - centerPoint;
        startPoint = centerPoint + new PixelPoint((int)(startPointDirection.X * scalingFactor),
            (int)(startPointDirection.Y * scalingFactor));
        var endPointDirection = endPoint - centerPoint;
        endPoint = centerPoint + new PixelPoint((int)(endPointDirection.X * scalingFactor),
            (int)(endPointDirection.Y * scalingFactor));

        // Create line object
        var line = new Line
        {
            StartPoint = BoardCanvas.PointToClient(startPoint),
            EndPoint = BoardCanvas.PointToClient(endPoint),
            Stroke = gameBoard.CurPlayerTurn == PlayerType.BlueLeft ? Brushes.DarkBlue : Brushes.DarkRed,
            StrokeThickness = 40 / (double)currentBoardSize,
        };

        // Add to canvas
        BoardCanvas.Children.Add(line);
    }
}