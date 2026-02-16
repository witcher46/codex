using System.Windows;
using WinOptimizePro.ViewModels;

namespace WinOptimizePro.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
