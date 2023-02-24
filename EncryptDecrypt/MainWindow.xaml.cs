using EncryptDecrypt.ViewModels;
using System.Windows;

namespace EncryptDecrypt;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainViewModel() { MainWindows = this };
        this.DataContext = vm;
    }
}
