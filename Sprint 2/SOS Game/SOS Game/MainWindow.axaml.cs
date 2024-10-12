using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace SOS_Game;

public partial class MainWindow : Window
{
    // These *must* be public for ui binding to take place
    public const decimal MinBoardSize = 3;
    public const decimal MaxBoardSize = 20;


    private int currentBoardSize;
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        currentBoardSize = GetBoardSizeInput();
    }

    private int GetBoardSizeInput()
    {
        return (int)Math.Clamp(Convert.ToInt32(BoardSizeNumericUpDown.Value), MinBoardSize, MaxBoardSize);
    }

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
        
        // Set letter of tile
        if(tileType != TileType.None)
            button.Content = Enum.GetName<TileType>(tileType);

        return button;
    }
    
    // DEBUG example function for using tag data
    // <Button Content="Click Me" Tag="Some Arbitrary Data" Click="Button_Click" />
    private void TokenButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var tagValue = button.Tag; // Access the Tag property
            // Use the tagValue as needed
            Console.WriteLine(tagValue); // Outputs: Some Arbitrary Data
        }
    }

    private void ClickTile(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            Debug.WriteLine("ClickTile called but sender is not a button! Sender: " + sender);
            return;
        }

        Console.WriteLine(sender.GetType().ToString() + button.GetType().ToString() + button.Tag);
        
        //TODO add turn and tile placement functionality
        
    }

    private void ClickNewGameButton(object? sender, RoutedEventArgs e)
    {

        //Get board size
        var boardSize = currentBoardSize = GetBoardSizeInput();
        var newTiles = new List<Button>(boardSize);
        
        //TODO Clean up event handler leaks in 
        // GameBoardGrid.Children.CollectionChanged
        // GameBoardGrid.SizeChanged

        //Generate new tiles
        for (int i = 0; i < Math.Pow(boardSize, 2); i++)
        {
            var tile = GetNewTile(TileType.None);
            
            //Set position on board
            // tile!.Child!.Tag = new Position(i / boardSize, i % boardSize);
            Grid.SetRow(tile, i / boardSize);
            Grid.SetColumn(tile, i % boardSize);

            // Set tile size to remain square
            var gridSize = Math.Min(GameBoarder.Bounds.Width, GameBoarder.Bounds.Height);
            tile.Width = tile.Height = gridSize / boardSize;
            
            newTiles.Add(tile);
        }
        
        
        //Clear existing board UI
        GameBoardGrid.Children.Clear();
        
        //Add new tiles to boardUI
        GameBoardGrid.Children.AddRange(newTiles);
    }

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
}