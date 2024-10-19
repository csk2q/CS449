using Avalonia;
using Avalonia.Headless;
using UnitTestProject;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace UnitTestProject;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<SOS_Game.App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}