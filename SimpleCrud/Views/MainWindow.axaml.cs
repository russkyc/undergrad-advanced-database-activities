using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleCrud.Services;
using SimpleCrud.ViewModels;

namespace SimpleCrud.Views;

public partial class MainWindow : Window
{
    // AvaloniaRider doesn't like constructors with dependencies
    // so we manually define an empty constructor
    public MainWindow()
    {
        InitializeComponent();
    }
    
    public MainWindow(StudentManagementViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}