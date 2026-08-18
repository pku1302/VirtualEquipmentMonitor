using System.Runtime.CompilerServices;
using VirtualEquipmentMonitor.Application.Abstractions;
using VirtualEquipmentMonitor.Application.Abstractions.Persistence;
using VirtualEquipmentMonitor.Domain.Alarms;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Application.Services;
public sealed class AlarmEvaluatingEquipmentStatusClient
    : IEquipmentStatusClient
{
    private readonly IEquipmentStatusClient _innerClient;
    private readonly EquipmentAlarmEvaluator _evaluator;
    private readonly IEquipmentAlarmRepository _alarmRepository;

    private readonly Dictionary<string, EquipmentSnapshot>
        _previousSnapshots = [];

    public AlarmEvaluatingEquipmentStatusClient(
        IEquipmentStatusClient innerClient,
        EquipmentAlarmEvaluator evaluator,
        IEquipmentAlarmRepository alarmRepository)
    {
        _innerClient = innerClient;
        _evaluator = evaluator;
        _alarmRepository = alarmRepository;
    }

    public async IAsyncEnumerable<EquipmentSnapshot> ReceiveAsync(
        string host,
        int port,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        await foreach (EquipmentSnapshot current in
            _innerClient.ReceiveAsync(
                host,
                port,
                cancellationToken))
        {
            _previousSnapshots.TryGetValue(
                current.DeviceId,
                out EquipmentSnapshot? previous);

            IReadOnlyList<EquipmentAlarm> alarms =
                _evaluator.Evaluate(
                    previous,
                    current);

            if (alarms.Count > 0)
            {
                await _alarmRepository.AddRangeAsync(
                    alarms,
                    cancellationToken);
            }

            _previousSnapshots[current.DeviceId] = current;

            yield return current;
        }
    }

}
