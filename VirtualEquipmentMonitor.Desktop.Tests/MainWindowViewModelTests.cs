using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using VirtualEquipmentMonitor.Desktop.Tests.Fakes;
using VirtualEquipmentMonitor.Desktop.ViewModels;
using VirtualEquipmentMonitor.Domain.Equipment;
using Xunit;
using Xunit.Sdk;

namespace VirtualEquipmentMonitor.Desktop.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public async Task ConnectCommand_WhenSnapshotArrives_UpdatesProperties()
    {
        var timestamp = new DateTimeOffset(
            2026,
            8,
            14,
            12,
            0,
            0,
            TimeSpan.Zero);

        var snapshot = new EquipmentSnapshot(
            "EQ-TEST",
            timestamp,
            EquipmentState.Warning,
            78.5,
            1600,
            3.8);

        var client = new FakeEquipmentStatusClient(
            [snapshot]);

        var viewModel =
            new MainWindowViewModel(
                client,
                new FakeEquipmentHistoryService());

        await viewModel.ConnectCommand.ExecuteAsync(null);

        Assert.Equal("EQ-TEST", viewModel.DeviceId);
        Assert.Equal("Warning", viewModel.OperatingState);
        Assert.Equal(78.5, viewModel.Temperature);
        Assert.Equal(1600, viewModel.Rpm);
        Assert.Equal(3.8, viewModel.Vibration);
        Assert.Equal(timestamp, viewModel.LastUpdatedAt);
    }

    [Fact]
    public async Task ConnectCommand_WhenServerEnds_ChangesConnectionStatus()
    {
        var snapshot = CreateSnapshot();

        var client = new FakeEquipmentStatusClient(
            [snapshot]);

        var viewModel =
            new MainWindowViewModel(
                client,
                new FakeEquipmentHistoryService());

        await viewModel.ConnectCommand.ExecuteAsync(null);

        Assert.Equal(
            "서버에서 연결을 종료했습니다.",
            viewModel.ConnectionStatus);

        Assert.False(viewModel.IsSessionActive);
        Assert.True(viewModel.ConnectCommand.CanExecute(null));
        Assert.False(viewModel.DisconnectCommand.CanExecute(null));
    }

    [Fact]
    public async Task DisconnectCommand_WhenConnected_CancelsReceiving()
    {
        var client = new FakeEquipmentStatusClient(
            [CreateSnapshot()],
            keepConnectionOpen: true);

        var viewModel = 
            new MainWindowViewModel(
                client,
                new FakeEquipmentHistoryService());

        Task connectionTask =
            viewModel.ConnectCommand.ExecuteAsync(null);

        await WaitUntilAsync(
            () => viewModel.DeviceId == "EQ-TEST",
            TimeSpan.FromSeconds(2));

        Assert.True(viewModel.IsSessionActive);
        Assert.False(viewModel.ConnectCommand.CanExecute(null));
        Assert.True(viewModel.DisconnectCommand.CanExecute(null));

        viewModel.DisconnectCommand.Execute(null);

        await connectionTask;

        Assert.False(viewModel.IsSessionActive);
        Assert.Equal(
            "연결 안 됨",
            viewModel.ConnectionStatus);
        Assert.True(viewModel.ConnectCommand.CanExecute(null));
        Assert.False(viewModel.DisconnectCommand.CanExecute(null));
    }

    [Fact]
    public async Task ConnectCommand_WhenSocketExceptionOccurs_ShowsError()
    {
        var client = new FakeEquipmentStatusClient(
            exception: new SocketException());

        var viewModel =
            new MainWindowViewModel(
                client,
                new FakeEquipmentHistoryService());

        await viewModel.ConnectCommand.ExecuteAsync(null);

        Assert.Equal(
            "연결 실패",
            viewModel.ConnectionStatus);

        Assert.Equal(
            "Simulator에 연결할 수 없습니다. 실행 상태를 확인하세요.",
            viewModel.ErrorMessage);

        Assert.False(viewModel.IsSessionActive);
    }

    [Fact]
    public async Task LoadHistoryCommand_LoadsRecentSnapshots()
    {
        DateTimeOffset baseTime =
            new(
                2026,
                8,
                16,
                12,
                0,
                0,
                TimeSpan.Zero);

        var olderSnapshot = new EquipmentSnapshot(
            "EQ-001",
            baseTime,
            EquipmentState.Running,
            30,
            1400,
            1.0);

        var newerSnapshot = new EquipmentSnapshot(
            "EQ-001",
            baseTime.AddSeconds(1),
            EquipmentState.Warning,
            80,
            1600,
            3.7);

        var client =
            new FakeEquipmentStatusClient();

        var historyService =
            new FakeEquipmentHistoryService(
                [olderSnapshot, newerSnapshot]);

        var viewModel =
            new MainWindowViewModel(
                client,
                historyService);

        await viewModel
            .LoadHistoryCommand
            .ExecuteAsync(null);

        Assert.Equal(2, viewModel.HistoryItems.Count);

        Assert.Equal(
            newerSnapshot.Timestamp,
            viewModel.HistoryItems[0].Timestamp);

        Assert.Equal(
            olderSnapshot.Timestamp,
            viewModel.HistoryItems[1].Timestamp);

        Assert.False(viewModel.IsHistoryLoading);
    }

    [Fact]
    public async Task LoadHistoryCommand_WhenItemAlreadyExists_DoesNotDuplicateIt()
    {
        var snapshot = new EquipmentSnapshot(
            "EQ-001",
            DateTimeOffset.UtcNow,
            EquipmentState.Running,
            42.5,
            1500,
            1.25);

        var client =
            new FakeEquipmentStatusClient([snapshot]);

        var historyService =
            new FakeEquipmentHistoryService([snapshot]);

        var viewModel =
            new MainWindowViewModel(
                client,
                historyService);

        await viewModel
            .ConnectCommand
            .ExecuteAsync(null);

        await viewModel
            .LoadHistoryCommand
            .ExecuteAsync(null);

        Assert.Single(viewModel.HistoryItems);
    }

    private static EquipmentSnapshot CreateSnapshot()
    {
        return new EquipmentSnapshot(
            "EQ-TEST",
            DateTimeOffset.UtcNow,
            EquipmentState.Running,
            42.5,
            1500,
            1.25);
    }

    private static async Task WaitUntilAsync(
        Func<bool> condition,
        TimeSpan timeout)
    {
        using var cancellationSource =
            new CancellationTokenSource(timeout);

        while (!condition())
        {
            await Task.Delay(
                TimeSpan.FromMilliseconds(10),
                cancellationSource.Token);
        }
    }
}
