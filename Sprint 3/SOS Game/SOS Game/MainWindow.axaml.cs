using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
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
            BorderThickness = new Thickness(1), // No border on the button
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
            if(control is Button tile)
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
            bool result = gameBoard.PlaceTile(Grid.GetRow(button), Grid.GetColumn(button), tileSelection, out Sos[] completedSosArray);

            // TODO Change to colored lines
            // Color background of buttons in sos
            foreach (var control in GameBoardGrid.Children)
            {
                if (control is Button curTile)
                {
                    var row = Grid.GetRow(curTile);
                    var col = Grid.GetColumn(curTile);
                    
                    foreach (var (s1, o, s2) in completedSosArray)
                    {
                        if ((s1.row == row && s1.column == col)
                            || (o.row == row && o.column == col)
                            || (s2.row == row && s2.column == col))
                        {
                            // Color letter based on player turn
                            if (placingPlayer == Player.BlueLeft)
                                curTile.Background = Brushes.DarkBlue;
                            else
                                curTile.Background = Brushes.DarkRed;
                        }
                        
                    }
                }
            }

            if (result)
            {//Tile was placed successfully
                // Color letter based on player turn
                if (placingPlayer == Player.BlueLeft)
                    button.Foreground = Brushes.Blue;
                else
                    button.Foreground = Brushes.Red;
                
                // Set tile letter
                button.Content = Enum.GetName(tileSelection);
                
                
                
                
                
            }
            else
            {//Failed to place tile
                Debug.WriteLine($"Failed to place tile {tileSelection} at Row:{Grid.GetRow(button)}, Column:{Grid.GetColumn(button)}. (From top left corner.)");
            }
            
            // Check for game completion based on Simple/General game mode
            

            updateTurnText();
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
        
        updateTurnText();
    }
    
    
    // Helper Functions //

}