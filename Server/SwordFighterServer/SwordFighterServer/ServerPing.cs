using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwordFighterServer
{
    public class ServerPing
    {
        public static int Port { get; private set; }

        private static UdpClient _udpPing;
        private static int _udpPingPort;
        private static CancellationTokenSource _udpCts;

        private const uint PING = 0x474E4950;
        private const uint PONG = 0x474E4F50;

        public static void Start(int port)
        {
            Port = port;

            _udpPingPort = port;
            _udpCts = new CancellationTokenSource();

            // 바인드
            _udpPing = new UdpClient(new IPEndPoint(IPAddress.Any, _udpPingPort));
            _udpPing.Client.ReceiveBufferSize = 1 << 20;
            _udpPing.Client.SendBufferSize = 1 << 20;

            // 비동기 루프
            _ = Task.Run(() => UdpPingLoopAsync(_udpCts.Token));

            Console.WriteLine($"Ping Server started on {Port}.");
        }

        private static async Task UdpPingLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    UdpReceiveResult recv;
                    try
                    {
                        recv = await _udpPing.ReceiveAsync(ct);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UDP] Receive error: {ex.Message}");
                        continue;
                    }

                    var buf = recv.Buffer;
                    var remote = recv.RemoteEndPoint;

                    if (buf.Length < 8)
                    {
                        continue;
                    }
                    if (BinaryPrimitives.ReadUInt32LittleEndian(buf) != PING)
                    {
                        continue;
                    }

                    uint seq = BinaryPrimitives.ReadUInt32LittleEndian(buf.AsSpan(4));
                    Console.WriteLine($"[UDP] Ping Received: {seq}");

                    byte[] rsp = new byte[8];
                    BinaryPrimitives.WriteUInt32LittleEndian(rsp, PONG);
                    BinaryPrimitives.WriteUInt32LittleEndian(rsp.AsSpan(4), seq);

                    try
                    {
                        await _udpPing.SendAsync(rsp, rsp.Length, remote);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UDP] Send error: {ex.Message}");
                    }
                }
            }
            finally
            {
                _udpPing?.Dispose();
            }
        }
    }
}
