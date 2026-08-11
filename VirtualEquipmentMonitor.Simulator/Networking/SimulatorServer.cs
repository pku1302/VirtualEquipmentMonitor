using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VirtualEquipmentMonitor.Simulator.Simulation;

namespace VirtualEquipmentMonitor.Simulator.Networking
{
    public sealed class SimulatorServer
    {
        private readonly TcpListener _listener;
        private readonly EquipmentStatusGenerator _generator;
        private readonly JsonSerializerOptions _jsonOptions;

        public SimulatorServer(
            int port,
            EquipmentStatusGenerator generator)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
            _generator = generator;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _listener.Start();

            Console.WriteLine(
                $"Simulator가 {_listener.LocalEndpoint}에서 대기 중입니다.");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("클라이언트 연결 대기 중...");

                    using TcpClient client =
                        await _listener.AcceptTcpClientAsync(cancellationToken);

                    Console.WriteLine(
                        $"클라이언트가 연결되었습니다: {client.Client.RemoteEndPoint}");

                    await SendStatusMessagesAsync(client, cancellationToken);
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Simulator 종료 요청을 받았습니다");
            }
            finally
            {
                _listener.Stop();
            }
        }

        private async Task SendStatusMessagesAsync(
            TcpClient client,
            CancellationToken cancellationToken)
        {
            try
            {
                await using NetworkStream stream = client.GetStream();
                await using var writer = new StreamWriter(
                    stream,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
                {
                    AutoFlush = true
                };

                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = _generator.Generate();
                    string json = JsonSerializer.Serialize(message, _jsonOptions);

                    await writer.WriteLineAsync(
                        json.AsMemory(),
                        cancellationToken);

                    Console.WriteLine($"전송: {json}");

                    await Task.Delay(
                        TimeSpan.FromSeconds(1),
                        cancellationToken);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("클라이언트 연결이 종료되었습니다");
            }
            catch (SocketException)
            {
                Console.WriteLine("TCP 연결 중 오류가 발생했습니다");
            }
        }
    }
}
