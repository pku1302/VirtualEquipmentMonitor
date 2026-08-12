using System.Windows;
using VirtualEquipmentMonitor.Desktop.ViewModels;
using VirtualEquipmentMonitor.Infrastructure.Communication;

namespace VirtualEquipmentMonitor.Desktop;
public partial class App : System.Windows.Application
{
    private MainWindowViewModel? _mainViewModel;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var statusClient = new TcpEquipmentStatusClient();

        _mainViewModel =
            new MainWindowViewModel(statusClient);

        var mainWindow = new MainWindow
        {
            DataContext = _mainViewModel
        };

        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mainViewModel?.Stop();
        base.OnExit(e);
    }
}
