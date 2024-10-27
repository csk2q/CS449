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
            TurnTextBlock.Text = "Game Over!";
            TurnTextBlock.Foreground = Brushes.Gold;
        }
        else if (gameBoard.PlayerTurn == Player.BlueLeft)
        {//BlueLeft's turn
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
            {//BlueLeft's turn
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

            // TODO Check for game completion based on Simple/General game mode
            // TODO Add message box announcing the winner

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
    }


    // Helper Functions //

    private void markSos(Sos sos)
    {
        var s1 = getTile(sos.S1.row, sos.S1.column);
        var o = getTile(sos.O.row, sos.O.column);
        var s2 = getTile(sos.S2.row, sos.S2.column);
        if (s1 is null || o is null || s2 is null)
        {
            Debug.WriteLine($"Failed get tiles at {sos.S1}, {sos.S2}. Yet, they were a part of an SOS?");
            return;
        }
        
        // Get the position of the top-left corner of the tile
        Point startPoint = s1.TranslatePoint(new Point(0, 0), BoardCanvas)!.Value;
        Point centerPoint = o.TranslatePoint(new Point(0, 0), BoardCanvas)!.Value;
        Point endPoint = s2.TranslatePoint(new Point(0, 0), BoardCanvas)!.Value;
        
        // Calculate center of buttons
        // Note: The values in Bounds does not seem to give exactly correct results.
        //  Using Bounds.Center gave even more erroneous values so manual calculation is necessary to get ok results.
        startPoint = new Point(startPoint.X + s1.Bounds.Width / 2, startPoint.Y + s1.Bounds.Height / 2);
        centerPoint = new Point(centerPoint.X + o.Bounds.Width, centerPoint.Y + o.Bounds.Height / 2);
        endPoint = new Point(endPoint.X + s2.Bounds.Width / 2, endPoint.Y + s2.Bounds.Height / 2);
        
        // Scale up around center so lines don't start from center of the tile
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

        // Ad to canvas
        BoardCanvas.Children.Add(line);
    }
}