using System;
using System.Net;
using System.Net.Sockets;
using System.Buffers.Binary;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ClientPing
{
    private static int Port { get { return 26951; } }

    private const uint PING = 0x474E4950;
    private const uint PONG = 0x474E4F50;
    private const int TimeoutMs = 10000;
    private const int MaxSeq = 255;
    private const int RequestInterval = 2000;

    private static uint _seq;

    private static readonly Dictionary<uint, long> _pingSeqSent = new Dictionary<uint, long>();
    public static event UnityAction<int> Action_OnPingUpdate;

    public static async UniTask PingLoop(string host, CancellationToken cts)
    {
        Debug.Log("[Ping] PingLoop started.");
        await UniTask.SwitchToThreadPool();
        _seq = 0;
        using var udp = new UdpClient();
        var addresses = await Dns.GetHostAddressesAsync(host);
        var remote = new IPEndPoint(addresses[0], Port);
        byte[] sendBuf = new byte[8];

        while (!cts.IsCancellationRequested)
        {
            try
            {
                BinaryPrimitives.WriteUInt32LittleEndian(sendBuf.AsSpan(0), PING);
                BinaryPrimitives.WriteUInt32LittleEndian(sendBuf.AsSpan(4), _seq);

                long clientSendTime = TimeSync.NowMs();
                _pingSeqSent.Add(_seq, clientSendTime);
                await udp.SendAsync(sendBuf, sendBuf.Length, remote).ConfigureAwait(false);

                var recvTask = udp.ReceiveAsync();
                var timeoutTask = Task.Delay(TimeoutMs);

                var completed = await Task.WhenAny(recvTask, timeoutTask).ConfigureAwait(false);
                if (completed != recvTask)
                {
                    _pingSeqSent.Remove(_seq);
                    return;
                }

                long clientReceiveTime = TimeSync.NowMs();
                //await Task.SwitchToMainThread();

                var res = recvTask.Result;
                var buf = res.Buffer;

                if (buf.Length < 8)
                    return;
                if (BinaryPrimitives.ReadUInt32LittleEndian(buf) != PONG)
                    return;
                if (BinaryPrimitives.ReadUInt32LittleEndian(buf.AsSpan(4)) != _seq)
                    return;

                var rtt = clientReceiveTime - clientSendTime;
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    Debug.Log($"[Ping] Seq: {_seq}, Send: {clientSendTime}, Received: {clientReceiveTime} -> RTT: {rtt}");
                    Action_OnPingUpdate?.Invoke((int) rtt);
                });

                if (++_seq > MaxSeq)
                    _seq = 0;
                await Task.Delay(RequestInterval);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    Debug.LogError($"[Ping] Server ping Failed: {ex.Message}");
                });
                await Task.Delay(RequestInterval);
            }
        }

        await UniTask.SwitchToMainThread();
        Debug.Log($"[Ping] PingLoop has been canceled.");
    }
}
