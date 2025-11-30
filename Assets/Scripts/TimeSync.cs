using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSync : MonoBehaviour
{
    private static long _lastSyncTime;
    private static long _timeOffset;
    private static bool _waitingForResponse;

    private const int RequestInterval = 5000;

    static readonly double _msPerTick = 1000.0 / System.Diagnostics.Stopwatch.Frequency;
    static readonly long _startTimeStamp = System.Diagnostics.Stopwatch.GetTimestamp();

    private void Update()
    {
        if (!_waitingForResponse && GetSyncedTime() - _lastSyncTime >= RequestInterval)
        {
            StartTimeSync();
        }
    }

    public static long NowMs()
    {
        return (long)((System.Diagnostics.Stopwatch.GetTimestamp() - _startTimeStamp) * _msPerTick);
    }

    public static long GetSyncedTime()
    {
        return NowMs() + _timeOffset;
    }

    public static void OnServerTimeResponse(long serverTime, long clientSendTime)
    {
        var clientReceiveTime = NowMs();
        var rtt = clientReceiveTime - clientSendTime;
        var halfRtt = rtt / 2L;

        long estimatedServerTime = serverTime + halfRtt;
        _timeOffset = estimatedServerTime - clientReceiveTime;
        _waitingForResponse = false;
        _lastSyncTime = GetSyncedTime();
    }

    private void StartTimeSync()
    {
        _waitingForResponse = true;
        long clientTime = NowMs();
        ClientSend.RequestServerTime(clientTime);
    }
}