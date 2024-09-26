using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SOS_Game;

public partial class MainWindow : Window
{
    private bool DisableTokenPickEvents = false;
    public int BoardSize = 3;

    // private bool BluePlayerIsS = true;

    public MainWindow()
    {
        InitializeComponent();
    }
    
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


    /*  Sync token choice between players*/

    private void BluePickSToken(object? sender, RoutedEventArgs e)
    {
        if (DisableTokenPickEvents || !(BlueSChoice?.IsChecked ?? false))
            return;
        DisableTokenPickEvents = true;

        RedSChoice.IsChecked = false;
        RedOChoice.IsChecked = true;

        DisableTokenPickEvents = false;
    }

    private void BluePicksOToken(object? sender, RoutedEventArgs e)
    {
        if (DisableTokenPickEvents || !(BlueOChoice?.IsChecked ?? false))
            return;
        DisableTokenPickEvents = true;

        RedSChoice.IsChecked = true;
        RedOChoice.IsChecked = false;

        DisableTokenPickEvents = false;
    }

    private void RedPickSToken(object? sender, RoutedEventArgs e)
    {
        if (DisableTokenPickEvents || !(RedSChoice?.IsChecked ?? false))
            return;

        DisableTokenPickEvents = true;

        BlueSChoice.IsChecked = false;
        BlueOChoice.IsChecked = true;

        DisableTokenPickEvents = false;
    }

    private void RedPickOToken(object? sender, RoutedEventArgs e)
    {
        if (DisableTokenPickEvents || !(RedOChoice?.IsChecked ?? false))
            return;

        DisableTokenPickEvents = true;

        BlueSChoice.IsChecked = true;
        BlueOChoice.IsChecked = false;

        DisableTokenPickEvents = false;
    }
}