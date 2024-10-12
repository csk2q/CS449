using Avalonia.Markup.Xaml;
using Avalonia;
using Avalonia.Headless;

namespace UnitTestProject;



public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}