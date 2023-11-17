using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SimpleCrud.Services;
using SimpleCrud.Views;

namespace SimpleCrud;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        DotNetEnv.Env.TraversePath().Load();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var container = AppHost.Build();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = container.Resolve<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

}