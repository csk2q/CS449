using System.Reflection;
using Avalonia.Controls;
using SOS_Game;
using SOS_Game.Logic;

namespace UnitTestProject;

public static class TestHelper
{
    public static void SetBoardSize(decimal size, in MainWindow window)
    {
        var numericUpDown = window.GetControl<NumericUpDown>("BoardSizeNumericUpDown");
        Assert.NotNull(numericUpDown);
        numericUpDown.Value = size;
    }
    
    public static void SetGameMode(GameType gameType, in MainWindow window)
    {
        // Set game mode selection
        var simpleRadioButton = window.FindControl<RadioButton>("SimpleGameRadioButton");
        Assert.NotNull(simpleRadioButton);
        var generalRadioButton = window.FindControl<RadioButton>("GeneralGameRadioButton");
        Assert.NotNull(generalRadioButton);

        simpleRadioButton.IsChecked = GameType.Simple == gameType;
        generalRadioButton.IsChecked = GameType.General == gameType;
    }

    public static void SetTileChoice(in PlayerType playerType, in TileType tileChoice, in MainWindow window)
    {
        RadioButton? sRadioButton;
        RadioButton? oRadioButton;

        if (playerType == PlayerType.BlueLeft)
        {
            sRadioButton = window.FindControl<RadioButton>("BlueSChoice");
            oRadioButton = window.FindControl<RadioButton>("BlueOChoice");
        }
        else if (playerType == PlayerType.RedRight)
        {
            sRadioButton = window.FindControl<RadioButton>("RedSChoice");
            oRadioButton = window.FindControl<RadioButton>("RedOChoice");
        }
        else
            throw new ArgumentException(
                $"Argument player must be {PlayerType.BlueLeft} or {PlayerType.RedRight}! Given player is {playerType}");
        
        Assert.NotNull(sRadioButton);
        Assert.NotNull(oRadioButton);

        // Set the value of the tile selection
        oRadioButton.IsChecked = tileChoice == TileType.O;
        sRadioButton.IsChecked = tileChoice == TileType.S;
    }

    public static void SetIsComputerRadioButtons(PlayerType playerType, bool isComputer, in MainWindow window)
    {        
        // RadioButton? isHumanRadioButton;
        // RadioButton? isComputerRadioButton;
        RadioButton? isHumanRadioButton;
        RadioButton? isComputerRadioButton;

        if (playerType == PlayerType.BlueLeft)
        {
            isHumanRadioButton = window.FindControl<RadioButton>("BlueHumanRadioButton");
            isComputerRadioButton = window.FindControl<RadioButton>("BlueComputerRadioButton");
        }
        else if (playerType == PlayerType.RedRight)
        {
            isHumanRadioButton = window.FindControl<RadioButton>("RedHumanRadioButton");
            isComputerRadioButton = window.FindControl<RadioButton>("RedComputerRadioButton");
        }
        else
            throw new ArgumentOutOfRangeException(
                $"Argument player must be {PlayerType.BlueLeft} or {PlayerType.RedRight}! Given player is {playerType}");
        
        Assert.NotNull(isHumanRadioButton);
        Assert.NotNull(isComputerRadioButton);

        // Set the value of the radio button
        isHumanRadioButton.IsChecked = !isComputer;
        isComputerRadioButton.IsChecked = isComputer;
    }

    public static void SetIsComputerGameBoard(PlayerType playerType, bool isComputer, GameBoard board)
    {
        PropertyInfo? playerInfo;

        switch (playerType)
        {
            case PlayerType.BlueLeft:
               playerInfo = typeof(GameBoard).GetProperty("Blue", BindingFlags.Public | BindingFlags.Instance);
                break;
            case PlayerType.RedRight:
                playerInfo = typeof(GameBoard).GetProperty("Red", BindingFlags.Public | BindingFlags.Instance);
                break;
            case PlayerType.None:
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, "Player must be either Blue or Red!");
        }
        
        Assert.NotNull(playerInfo);
        playerInfo.SetValue(board, Player.Create(board.Blue.PlayerType, isComputer));
    }
    
}