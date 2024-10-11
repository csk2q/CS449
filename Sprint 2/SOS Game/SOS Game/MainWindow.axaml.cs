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
    private bool DisableTokenPickEvents = false;
    // public int BoardSize = 3;

    // private bool BluePlayerIsS = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    private Button GetNewTile(TileType tileType)
    {
        /*// tileElement for reuse in the board
        Border tileElement = new Border
        {
            BorderThickness = new Thickness(0.5),
            BorderBrush = Brushes.Black,
            Child = new Button
            {
                Content = "", // Button content
                // FontSize = 35, // Font size of the button text
                Background = Brushes.Gray, // Background color of the button
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
                // HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                // VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                CornerRadius = new CornerRadius(0), // No rounded corners
                Padding = new Thickness(0), // No padding
                Margin = new Thickness(0), // No margin
                BorderThickness = new Thickness(0), // No border on the button
            }
        };*/

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
        //     ((Button)tileElement.Child).Content = Enum.GetName<TileType>(tileType);
        /*switch (tileType)
        {
            default:
            case TileType.None:
                break;
            case TileType.S:
                ((Button)tileElement.Child).Content = Enum.GetName<TileType>(tileType);
                break;
            case TileType.O:
                ((Button)tileElement.Child).Content = Enum.GetName<TileType>(tileType);
                break;
        }*/
        
        // Add click event function
        // ((Button)tileElement.Child).Click += ClickTile;

        // return (Control)tileElement.Child;
        // return tileElement;

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
        //TODO Remove magic numbers for min(3) and max(20) board size
        var boardSize = Math.Clamp(Convert.ToInt32(BoardSizeNumericUpDown.Value), 3, 20);
        var newTiles = new List<Button>(boardSize);

        //Generate new tiles
        for (int i = 0; i < Math.Pow(boardSize, 2); i++)
        {
            var tile = GetNewTile(TileType.None);
            // tile!.Child!.Tag = new Position(i / boardSize, i % boardSize);
            Grid.SetRow(tile, i / boardSize); 
            Grid.SetColumn(tile, i % boardSize);

            var gridSize = Math.Min(GameBoardGrid.Bounds.Width, GameBoardGrid.Bounds.Height);
            tile.Width = tile.Height = gridSize / boardSize;

            GameBoardGrid.Children.CollectionChanged += (_, __) =>
            {
                var gridSize = Math.Min(GameBoardGrid.Bounds.Width, GameBoardGrid.Bounds.Height);
                GameBoardGrid.Width = GameBoardGrid.Height = gridSize;
            };
            
            GameBoardGrid.SizeChanged += (sender, e) =>
            {
                var gridSize = Math.Min(GameBoardGrid.Bounds.Width, GameBoardGrid.Bounds.Height);
                tile.Width = tile.Height = gridSize / boardSize;
            };
            
            newTiles.Add(tile);
        }
        
        
        //Clear existing board UI
        GameBoardGrid.Children.Clear();
        
        // Update board size
        // GameBoardGrid
        // GameBoardGrid.ColumnDefinitions.Clear();
        // GameBoardGrid.RowDefinitions.Clear();
        // GameBoardGrid.ColumnDefinitions.AddRange(Enumerable.Repeat(new ColumnDefinition(GridLength.Star), boardSize));
        // GameBoardGrid.RowDefinitions.AddRange(Enumerable.Repeat(new RowDefinition(GridLength.Star), boardSize));
        
        //Add new tiles to boardUI
        GameBoardGrid.Children.AddRange(newTiles);
        
        
        
    }
}