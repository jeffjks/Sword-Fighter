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

    public static event Action<int> Action_OnPingUpdate;

    private void Update()
    {
        if (!_waitingForResponse && GetSyncTime() - _lastSyncTime >= RequestInterval)
        {
            SyncServerTime();
        }
    }

    public static long GetSyncTime()
    {
        return GetLocalUnixTime() + _timeOffset;
    }

    public static long GetLocalUnixTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static void SyncServerTime(long serverTime, long clientSendTime)
    {
        var clientReceiveTime = GetLocalUnixTime();
        var rtt = clientReceiveTime - clientSendTime;
        var halfRtt = rtt / 2L;

        long estimatedServerTime = serverTime + halfRtt;
        _timeOffset = estimatedServerTime - clientReceiveTime;
        _waitingForResponse = false;
        _lastSyncTime = GetSyncTime();
        
        Action_OnPingUpdate?.Invoke((int) rtt);
    }

    private void SyncServerTime()
    {
        _waitingForResponse = true;
        ClientSend.RequestServerTime();
    }
}