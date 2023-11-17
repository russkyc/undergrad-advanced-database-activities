using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
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
        var host = AppHost.Build();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = host.Services.GetService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

}