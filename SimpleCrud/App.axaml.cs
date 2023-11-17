using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Infrastructure.Data;
using MongoFramework;
using Russkyc.DependencyInjection.Implementations;
using SimpleCrud.Services;
using SimpleCrud.ViewModels;
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
        var connection = MongoDbConnection.FromConnectionString(Environment.GetEnvironmentVariable("MONGODB_URI"));
        var container = ApplicationHost<MainWindow>.CreateDefault()
            .ConfigureServices(services =>
            {
                services.Add(new DbContext(connection))
                    .AddSingleton<DialogService>()
                    .AddTransient<StudentManagementViewModel>();
            });

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = container.Root;
        }

        base.OnFrameworkInitializationCompleted();
    }

}