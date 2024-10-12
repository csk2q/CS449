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
    // Variables //

    private int currentBoardSize;
    private GameBoard gameBoard = new(GameType.Simple, 3);
    
    
    // Constructor //
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        currentBoardSize = GetBoardSizeInput();
    }
    
    // Getters & Setters //

    private int GetBoardSizeInput()
    {
        int value = (int)Math.Clamp(Convert.ToInt32(BoardSizeNumericUpDown.Value), GameBoard.MinBoardSize,
            GameBoard.MaxBoardSize);
        
        //Override displayed value to show used value.
        BoardSizeNumericUpDown.Value = (decimal)value;
        
        return value;
    }

    // Returns a new tile for use on the board
    private Button GetNewTile(TileType tileType)
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
            BorderBrush = Brushes.Red,
        };

        //Click callback
        button.Click += PlaceTile;
        
        // Set letter of tile
        if (tileType != TileType.None)
        {
            button.Content = Enum.GetName<TileType>(tileType);
        }

        return button;
    }
    
    private void SetTurnText()
    {
        if (gameBoard.PlayerTurn == Player.BlueLeft)
        {//BlueLeft's turn
            TurnTextBlock.Text = "Blue's Turn";
            TurnTextBlock.Foreground = Brushes.Blue;
        }
        else
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
    
    private void PlaceTile(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            //TODO add turn and tile placement functionality
            
            // Variables
            TileType tileSelection = TileType.None;

            // Get player choices
            if (gameBoard.PlayerTurn == Player.BlueLeft)
            {//BlueLeft's turn
                if (BlueSChoice.IsChecked ?? true)
                    tileSelection = TileType.S;
                else
                    tileSelection = TileType.O;
            }
            else
            {//RedRight's turn
                if (RedSChoice.IsChecked ?? true)
                    tileSelection = TileType.S;
                else
                    tileSelection = TileType.O;
            }
            
            // Try place tile
            bool result = gameBoard.PlaceTile(Grid.GetRow(button), Grid.GetColumn(button), tileSelection);

            if (result)
            {//Tile was placed successfully
                button.Content = Enum.GetName<TileType>(tileSelection);
            }
            else
            {//Failed to place tile
                //TODO ? show message box about tile placement failure
                Debug.WriteLine($"Failed to place tile.");
            }

            SetTurnText();

        }
        else
            Debug.Assert(false, "ClickTile called but sender is not a button! Sender: " + sender);
    }

    private void StartNewGame(object? sender, RoutedEventArgs e)
    {
        // Get input
        var boardSize = currentBoardSize = GetBoardSizeInput();
        
        //Default game mode is simple
        GameType gameMode = (SimpleGameRadioButton.IsChecked ?? true) ? GameType.Simple : GameType.General;
        
        // Set up variables
        gameBoard = new GameBoard(gameMode, boardSize);
        var newTiles = new List<Button>(boardSize);

        //Generate new tiles
        for (int i = 0; i < Math.Pow(boardSize, 2); i++)
        {
            var tile = GetNewTile(TileType.None);
            
            //Set position on board
            // tile!.Child!.Tag = new Position(i / boardSize, i % boardSize);
            Grid.SetRow(tile, i / boardSize);
            Grid.SetColumn(tile, i % boardSize);

            // Set tile size and maintain aspect ratio
            var gridSize = Math.Min(GameBoarder.Bounds.Width, GameBoarder.Bounds.Height);
            var sideLength = gridSize / boardSize;
            tile.Width = tile.Height = sideLength;
            
            //Scale font size
            tile.FontSize = sideLength * 0.80;
            
            newTiles.Add(tile);
        }
        
        //Clear and update board UI
        GameBoardGrid.Children.Clear();
        GameBoardGrid.Children.AddRange(newTiles);
        
        SetTurnText();
    }
    
    
    // Helper Functions //

}