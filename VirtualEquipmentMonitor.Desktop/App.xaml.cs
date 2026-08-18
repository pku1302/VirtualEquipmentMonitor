using System.IO;
using System.Windows;
using VirtualEquipmentMonitor.Application.Services;
using VirtualEquipmentMonitor.Desktop.ViewModels;
using VirtualEquipmentMonitor.Domain.Alarms;
using VirtualEquipmentMonitor.Infrastructure.Communication;
using VirtualEquipmentMonitor.Infrastructure.Persistence;

namespace VirtualEquipmentMonitor.Desktop;
public partial class App : System.Windows.Application
{
    private MainWindowViewModel? _mainViewModel;

    protected override async void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            string databaseDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "VirtualEquipmentMonitor");

            Directory.CreateDirectory(databaseDirectory);

            string databasePath = Path.Combine(
                databaseDirectory,
                "equipment-monitor.db");

            var contextFactory =
                new SqliteEquipmentDbContextFactory(
                    databasePath);

            var databaseInitailizer =
                new EquipmentDatabaseInitializer(
                    contextFactory);

            await databaseInitailizer.InitializeAsync();

            var snapshotRepository =
                new EquipmentSnapshotRepository(
                    contextFactory);

            var alarmRepository =
                new EquipmentAlarmRepository(
                    contextFactory);

            var historyService =
                new EquipmentHistoryService(
                    snapshotRepository);

            var tcpClient =
                new TcpEquipmentStatusClient();

            var persistingClient =
                new PersistingEquipmentStatusClient(
                    tcpClient,
                    snapshotRepository);

            var alarmEvaluator =
                new EquipmentAlarmEvaluator(
                    EquipmentThresholds.Default);

            var alarmEvaluatingClient =
                new AlarmEvaluatingEquipmentStatusClient(
                    persistingClient,
                    alarmEvaluator,
                    alarmRepository);

            _mainViewModel =
                new MainWindowViewModel(
                    alarmEvaluatingClient,
                    historyService);

            var mainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };

            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"애플리케이션을 시작할 수 없습니다.\n\n{exception.Message}",
                "초기화 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(exitCode: 1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mainViewModel?.Stop();
        base.OnExit(e);
    }
}
