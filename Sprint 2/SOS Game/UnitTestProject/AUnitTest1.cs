using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

namespace UnitTestProject;

public class AUnitTest1
{
    /*
     * Instead of the typical [Fact] attribute, we need to use [AvaloniaFact] as it sets up the UI thread. Similarly, instead of [Theory], there is a [AvaloniaTheory] attribute.
     */
    
    [AvaloniaFact]
    public void Should_Type_Text_Into_TextBox()
    {
        // Setup controls:
        var textBox = new TextBox();
        var window = new Window { Content = textBox };

        // Open window:
        window.Show();

        // Focus text box:
        textBox.Focus();

        // Simulate text input:
        window.KeyTextInput("Hello World");

        // Assert:
        Assert.Equal("Hello World", textBox.Text);
    }
}