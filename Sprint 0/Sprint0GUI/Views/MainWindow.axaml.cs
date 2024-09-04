using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace Sprint0GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Add button to grid at run time.
        // var button = new Button() { [Grid.ColumnProperty] = 1, Content = "Push me"};
        // button.AddHandler(Button.ClickEvent, (sender, args) => { Console.WriteLine("The button was pushed.");});
        // RootGrid.Children.Add(button);
        
    }
    
    /*public void ButtonClicked(object source, RoutedEventArgs args)
    {
        Debug.WriteLine($"Click! Celsius={celsius.Text}");
        if (Double.TryParse(celsius.Text, out double C))
        {
            var F = C * (9d / 5d) + 32;
            fahrenheit.Text = F.ToString("0.0");
        }
        else
        {
            celsius.Text = "0";
            fahrenheit.Text = "0";
        }
    }*/
}