using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Net.Sockets;
using VirtualEquipmentMonitor.Application.Abstractions;

namespace VirtualEquipmentMonitor.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private const string Host = "127.0.0.1";
    private const int Port = 5000;

    private readonly IEquipmentStatusClient _statusClient;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    private bool _isSessionActive;

    [ObservableProperty]
    private string _connectionStatus = "연결 안 됨";

    [ObservableProperty]
    private string _deviceId = "-";

    [ObservableProperty]
    private string _operatingState = "-";

    [ObservableProperty]
    private double _temperature;

    [ObservableProperty]
    private int _rpm;

    [ObservableProperty]
    private double _vibration;

    [ObservableProperty]
    private DateTimeOffset? _lastUpdatedAt;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public MainWindowViewModel(
        IEquipmentStatusClient statusClient)
    {
        _statusClient = statusClient;
    }
    private bool CanConnect()
    {
        return !IsSessionActive;
    }
    private bool CanDisconnect()
    {
        return IsSessionActive;
    }

    [RelayCommand(
        CanExecute = nameof(CanConnect),
        AllowConcurrentExecutions = false)]
    private async Task ConnectAsync(
        CancellationToken cancellationToken)
    {
        IsSessionActive = true;
        ConnectionStatus = "연결 시도 중...";
        ErrorMessage = string.Empty;

        try
        {
            await foreach(var snapshot in
                _statusClient.ReceiveAsync(
                    Host,
                    Port,
                    cancellationToken))
            {
                ConnectionStatus = "연결됨";

                DeviceId = snapshot.DeviceId;
                OperatingState = snapshot.State.ToString();
                Temperature = snapshot.Temperature;
                Rpm = snapshot.Rpm;
                Vibration = snapshot.Vibration;
                LastUpdatedAt = snapshot.Timestamp;
            }

            ConnectionStatus = "서버에서 연결을 종료했습니다.";
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            ConnectionStatus = "연결 안 됨";
        }
        catch (SocketException)
        {
            ConnectionStatus = "연결 실패";
            ErrorMessage =
                "Simulator에 연결할 수 없습니다. 실행 상태를 확인하세요.";
        }
        catch (InvalidDataException exception)
        {
            ConnectionStatus = "데이터 오류";
            ErrorMessage = exception.Message;
        }
        catch (Exception exception)
        {
            ConnectionStatus = "오류 발생";
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsSessionActive = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private void Disconnect()
    {
        ConnectionStatus = "연결 종료 중...";
        ConnectCommand.Cancel();
    }
    public void Stop()
    {
        ConnectCommand.Cancel();
    }
}