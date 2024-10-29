using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using SOS_Game.Logic;

namespace SOS_Game;

public partial class MainWindow : Window
{
    // Note: Several functions & variables have been made public to allow for unit testing

    // Variables //

    private int currentBoardSize;
    private GameBoard gameBoard = new SimpleGame(0);


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
            TurnDisplay.Opacity = 0;
            WinnerDisplay.Opacity = 100;

            switch (gameBoard.GetWinner())
            {
                // Draw
                case Player.None:
                    WinnerNameText.Text = "None (Draw)";
                    WinnerNameText.Foreground = Brushes.MediumPurple;
                    break;
                // Blue win
                case Player.BlueLeft:
                    WinnerNameText.Text = "Blue!";
                    WinnerNameText.Foreground = Brushes.Blue;
                    break;
                // Red win
                case Player.RedRight:
                    WinnerNameText.Text = "Red!";
                    WinnerNameText.Foreground = Brushes.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
        else
        {
            TurnDisplay.Opacity = 100;
            WinnerDisplay.Opacity = 0;
            
            if (gameBoard.PlayerTurn == Player.BlueLeft)
            {
                //BlueLeft's turn
                TurnTextBlock.Text = "Blue's Turn";
                TurnTextBlock.Foreground = Brushes.Blue;
            }
            else if (gameBoard.PlayerTurn == Player.RedRight)
            {
                //RedRight's turn
                TurnTextBlock.Text = "Red's Turn";
                TurnTextBlock.Foreground = Brushes.Red;
            }
        }
    }

    private void updateScoreText()
    {
        BlueScore.Text = gameBoard.BlueScore.ToString();
        RedScore.Text = gameBoard.RedScore.ToString();
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


    // UI Logic //

    private void placeTile(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // Variables
            TileType tileSelection;
            var placingPlayer = gameBoard.PlayerTurn;

            // Get player choices
            if (placingPlayer == Player.BlueLeft)
            {
                //BlueLeft's turn
                if (BlueSChoice.IsChecked ?? true)
                    tileSelection = TileType.S;
                else
                    tileSelection = TileType.O;
            }
            else if (placingPlayer == Player.RedRight)
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
            bool result = gameBoard.PlaceTile(Grid.GetRow(button), Grid.GetColumn(button), tileSelection,
                out Sos[] completedSosArray);

            // For every completed SOS
            foreach (var sos in completedSosArray)
            {
                markSos(sos);

                // TODO REMOVE DEPRECATED
                // Color background of buttons in completed sos
                /*Position[] tiles = [sos.S1, sos.O, sos.S2];
                foreach (var position in tiles)
                {
                    Button? curTile = getTile(position.row, position.column);
                    Debug.Assert(curTile is not null);
                    if (curTile is not null)
                    {
                        // Color letter based on player turn
                        if (placingPlayer == Player.BlueLeft)
                            curTile.Background = Brushes.DarkBlue;
                        else
                            curTile.Background = Brushes.DarkRed;
                    }
                    else
                        throw new ApplicationException(
                            $"Could not find tile {position.row}, {position.column}! Yet, it was part of an SOS?");
                }*/
            }

            if (result)
            {
                //Tile was placed successfully
                // Color letter based on player turn
                if (placingPlayer == Player.BlueLeft)
                    button.Foreground = Brushes.Blue;
                else
                    button.Foreground = Brushes.Red;

                // Set tile letter
                button.Content = Enum.GetName(tileSelection);
            }
            else
            {
                //Failed to place tile
                Debug.WriteLine(
                    $"Failed to place tile {tileSelection} at Row:{Grid.GetRow(button)}, Column:{Grid.GetColumn(button)}. (From top left corner.)");
            }

            updateTurnText();
            updateScoreText();
        }
        else
            Debug.Assert(false, "ClickTile called but sender is not a button! Sender: " + sender);
    }

    public void StartNewGame(object? sender, RoutedEventArgs e)
    {
        // Get input
        var boardSize = currentBoardSize = getBoardSizeInput();

        //Default game mode is simple
        GameType gameMode = (SimpleGameRadioButton.IsChecked ?? true) ? GameType.Simple : GameType.General;

        //Set up variables
        var newTiles = new List<Button>(boardSize);


        // TODO move to GameBoard.CreateNewGame(gameType)
        if (gameMode == GameType.Simple)
            gameBoard = new SimpleGame(boardSize);
        else
            gameBoard = new GeneralGame(boardSize);

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
        // Manual calculation is necessary to receive relatively ok results.

        // Get the position of the top-left corner of the tile
        Point startPoint = s1.TranslatePoint(new Point(-7.5, -2.5), BoardCanvas)!.Value;
        Point centerPoint = o.TranslatePoint(new Point(0, 0), BoardCanvas)!.Value;
        Point endPoint = s2.TranslatePoint(new Point(0, 0), BoardCanvas)!.Value;

        // Debug rendering of top left of tile
        if (false)
        {
            var circle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.LawnGreen),
                Width = 5,
                Height = 5,
                ZIndex = 999,
            };
            Canvas.SetLeft(circle, startPoint.X);
            Canvas.SetTop(circle, startPoint.Y);
            BoardCanvas.Children.Add(circle);
            circle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.DeepPink),
                Width = 5,
                Height = 5,
                ZIndex = 999,
            };
            Canvas.SetLeft(circle, centerPoint.X);
            Canvas.SetTop(circle, centerPoint.Y);
            BoardCanvas.Children.Add(circle);
            circle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.OrangeRed),
                Width = 5,
                Height = 5,
                ZIndex = 999,
            };
            Canvas.SetLeft(circle, endPoint.X);
            Canvas.SetTop(circle, endPoint.Y);
            BoardCanvas.Children.Add(circle);
        }

        // Subtract the boarders to get the real top left corner
        startPoint = new(startPoint.X - s1.BorderThickness.Left, startPoint.Y - s1.BorderThickness.Top);
        centerPoint = new(centerPoint.X - o.BorderThickness.Left, centerPoint.Y - o.BorderThickness.Top);
        endPoint = new(endPoint.X - s2.BorderThickness.Left, endPoint.Y - s2.BorderThickness.Top);


        // Calculate the width including the boarders
        var s1Width = s1.Width + s1.BorderThickness.Left + s1.BorderThickness.Right;
        var oWidth = o.Width + o.BorderThickness.Left + o.BorderThickness.Right;
        var s2Width = s2.Width + s2.BorderThickness.Left + s2.BorderThickness.Right;

        // Calculate the height including the boarders
        var s1Height = s1.Height + s1.BorderThickness.Bottom + s1.BorderThickness.Top;
        var oHeight = o.Height + o.BorderThickness.Bottom + o.BorderThickness.Top;
        var s2Height = s2.Height + s2.BorderThickness.Bottom + s2.BorderThickness.Top;

        // Calculate the center
        startPoint = new Point(startPoint.X + s1Width / 2, startPoint.Y + s1Height / 2);
        centerPoint = new Point(centerPoint.X + oWidth / 2, centerPoint.Y + oHeight / 2);
        endPoint = new Point(endPoint.X + s2Width / 2, endPoint.Y + s2Height / 2);

        // Scale up around center so lines don't start from center of the tiles
        // Desmos graph of the scaling math: https://www.desmos.com/calculator/aws7k4yil5
        var scalingFactor = 1.3;
        startPoint = centerPoint + (scalingFactor * (startPoint - centerPoint));
        endPoint = centerPoint + (scalingFactor * (endPoint - centerPoint));


        // Create line object
        var line = new Line
        {
            StartPoint = startPoint,
            EndPoint = endPoint,
            Stroke = gameBoard.PlayerTurn == Player.BlueLeft ? Brushes.DarkBlue : Brushes.DarkRed,
            StrokeThickness = 40 / (double)currentBoardSize,
        };

        // Add to canvas
        BoardCanvas.Children.Add(line);
    }
}