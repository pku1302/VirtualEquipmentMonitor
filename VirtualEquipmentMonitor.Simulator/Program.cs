using VirtualEquipmentMonitor.Simulator.Networking;
using VirtualEquipmentMonitor.Simulator.Simulation;

const int port = 5000;

using var cancellationSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

var generator = new EquipmentStatusGenerator();
var server = new SimulatorServer(port, generator);

Console.WriteLine("가상 장비 Simulator");
Console.WriteLine("종료하려면 Ctrl+C를 누르세요.");

await server.RunAsync(cancellationSource.Token);