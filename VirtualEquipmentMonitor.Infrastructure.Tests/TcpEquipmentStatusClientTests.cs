using System.Net;
using System.Net.Sockets;
using System.Text;
using VirtualEquipmentMonitor.Infrastructure.Communication;

namespace VirtualEquipmentMonitor.Infrastructure.Tests;

public sealed class TcpEquipmentStatusClientTests
{
    [Fact]
    public async Task ReceiveAsync_WithValidMessage_ReturnsSnapshot()
    {
        var listener = new TcpListener(
            IPAddress.Loopback,
            port: 0);

        listener.Start();

        int port =
            ((IPEndPoint)listener.LocalEndpoint).Port;

        Task serverTask = SendTestMessageAsync(listener);

        var client = new TcpEquipmentStatusClient();

        using var cancellationSource =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

        var snapshots = new List<
            Domain.Equipment.EquipmentSnapshot>();

        await foreach(var snapshot in client.ReceiveAsync(
            "127.0.0.1",
            port,
            cancellationSource.Token))
        {
            snapshots.Add(snapshot);
        }

        await serverTask;
        listener.Stop();

        var received = Assert.Single(snapshots);

        Assert.Equal("EQ-TEST", received.DeviceId);
        Assert.Equal(42.5, received.Temperature);
        Assert.Equal(1500, received.Rpm);
        Assert.Equal(1.25, received.Vibration);
    }

    private static async Task SendTestMessageAsync(
        TcpListener listener)
    {
        using TcpClient client =
            await listener.AcceptTcpClientAsync();

        await using NetworkStream stream =
            client.GetStream();

        await using var writer = new StreamWriter(
            stream,
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false))
        {
            AutoFlush = true
        };

        const string json =
            """
            {"deviceId":"EQ-TEST","timestamp":"2026-08-12T12:00:00+00:00","state":"Running","temperature":42.5,"rpm":1500,"vibration":1.25}
            """;

        await writer.WriteLineAsync(json);
    }
}
