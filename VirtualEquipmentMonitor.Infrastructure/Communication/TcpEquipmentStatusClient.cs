using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VirtualEquipmentMonitor.Application.Abstractions;
using VirtualEquipmentMonitor.Contracts.Messages;
using VirtualEquipmentMonitor.Domain.Equipment;
using VirtualEquipmentMonitor.Infrastructure.Mapping;

namespace VirtualEquipmentMonitor.Infrastructure.Communication;

public sealed class TcpEquipmentStatusClient
    : IEquipmentStatusClient
{
    private readonly JsonSerializerOptions _jsonOptions;

    public TcpEquipmentStatusClient()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _jsonOptions.Converters.Add(
            new JsonStringEnumConverter());
    }

    public async IAsyncEnumerable<EquipmentSnapshot> ReceiveAsync(
        string host,
        int port,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        using var client = new TcpClient();

        await client.ConnectAsync(
            host,
            port,
            cancellationToken);

        await using NetworkStream stream = client.GetStream();

        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: false);

        while (!cancellationToken.IsCancellationRequested)
        {
            string? json = await reader.ReadLineAsync(
                cancellationToken);

            if (json is null)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                continue;
            }

            EquipmentStatusMessage? message;

            try
            {
                message =
                    JsonSerializer.Deserialize<EquipmentStatusMessage>(
                        json,
                        _jsonOptions);
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException(
                    "수신한 장비 메시지의 JSON 형식이 올바르지 않습니다.",
                    exception);
            }

            if (message is null)
            {
                throw new InvalidDataException(
                    "수신한 장비 메시지가 비어 있습니다.");
            }

            yield return EquipmentStatusMapper.ToDomain(message);
        }
    }
}