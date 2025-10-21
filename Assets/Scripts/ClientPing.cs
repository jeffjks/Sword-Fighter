using System;
using System.Net;
using System.Net.Sockets;
using System.Buffers.Binary;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

public class ClientPing
{
    private static int port { get { return 26951; } }

    private const uint PING = 0x474E4950;
    private const uint PONG = 0x474E4F50;
    private const int timeoutMs = 10000;

    private static readonly Dictionary<uint, long> _pingSeqSent = new Dictionary<uint, long>();
    public static event UnityAction<int> Action_OnPingUpdate;

    public static async UniTaskVoid SendPingAsync(string host, uint seq)
    {
        try
        {
            using var udp = new UdpClient();
            var addresses = await Dns.GetHostAddressesAsync(host);
            var remote = new IPEndPoint(addresses[0], port);
            
            byte[] sendBuf = new byte[8];
            BinaryPrimitives.WriteUInt32LittleEndian(sendBuf.AsSpan(0), PING);
            BinaryPrimitives.WriteUInt32LittleEndian(sendBuf.AsSpan(4), seq);

            long clientSendTime = TimeSync.GetLocalTimeMs();
            _pingSeqSent.Add(seq, clientSendTime);
            await udp.SendAsync(sendBuf, sendBuf.Length, remote);

            long clientReceiveTime = TimeSync.GetLocalTimeMs();
            
            var recvTask = udp.ReceiveAsync();
            var timeoutTask = Task.Delay(timeoutMs);

            var completed = await Task.WhenAny(recvTask, timeoutTask);
            if (completed != recvTask)
            {
                _pingSeqSent.Remove(seq);
                return;
            }

            var res = recvTask.Result;
            var buf = res.Buffer;

            if (buf.Length < 8)
                return;
            if (BinaryPrimitives.ReadUInt32LittleEndian(buf) != PONG)
                return;
            if (BinaryPrimitives.ReadUInt32LittleEndian(buf.AsSpan(4)) != seq)
                return;

            var rtt = clientReceiveTime - clientSendTime;
            Action_OnPingUpdate?.Invoke((int) rtt);

            return;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Server ping Failed: {ex.Message}");
            return;
        }
    }
}
