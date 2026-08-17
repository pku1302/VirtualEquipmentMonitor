using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using VirtualEquipmentMonitor.Desktop.Models;
using VirtualEquipmentMonitor.Application.Abstractions;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private const string Host = "127.0.0.1";
    private const string DeviceIdToMonitor = "EQ-001";
    private const int MaximumHistoryCount = 100;
    private const int Port = 5000;

    private readonly IEquipmentStatusClient _statusClient;
    private readonly IEquipmentHistoryService _historyService;

    public ObservableCollection<EquipmentHistoryItem>
        HistoryItems
    { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    private bool _isSessionActive;

    [ObservableProperty]
    private bool _isHistoryLoading;

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
    [NotifyPropertyChangedFor(nameof(LocalLastUpdatedAt))]
    private DateTimeOffset? _lastUpdatedAt;

    public DateTimeOffset? LocalLastUpdatedAt =>
        LastUpdatedAt?.ToLocalTime();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public MainWindowViewModel(
        IEquipmentStatusClient statusClient,
        IEquipmentHistoryService historyService)
    {
        _statusClient = statusClient;
        _historyService = historyService;
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
            await LoadHistoryAsync(cancellationToken);

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

                MergeHistory([snapshot]);
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

    [RelayCommand]
    private async Task LoadHistoryAsync(
        CancellationToken cancellationToken)
    {
        IsHistoryLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            IReadOnlyList<EquipmentSnapshot>
                snapshots =
                    await _historyService.GetRecentAsync(
                        DeviceIdToMonitor,
                        MaximumHistoryCount,
                        cancellationToken);

            MergeHistory(snapshots);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // 사용자가 조회를 취소한 경우 오류로 표시하지 않습니다.
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"측정 이력을 불러오지 못했습니다: {exception.Message}";
        }
        finally
        {
            IsHistoryLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private void Disconnect()
    {
        ConnectionStatus = "연결 종료 중...";
        ConnectCommand.Cancel();
    }
    private void MergeHistory(
        IEnumerable<EquipmentSnapshot> snapshots)
    {
        List<EquipmentHistoryItem> mergedItems =
            HistoryItems
                .Concat(
                    snapshots.Select(
                        EquipmentHistoryItem.FromDomain))
                .GroupBy(item => new
                {
                    item.DeviceId,
                    item.Timestamp
                })
                .Select(group => group.First())
                .OrderByDescending(item => item.Timestamp)
                .Take(MaximumHistoryCount)
                .ToList();

        HistoryItems.Clear();

        foreach (EquipmentHistoryItem item in mergedItems)
        {
            HistoryItems.Add(item);
        }
    }
    public void Stop()
    {
        ConnectCommand.Cancel();
    }
}