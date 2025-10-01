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
        if (!_waitingForResponse && GetSyncedTime() - _lastSyncTime >= RequestInterval)
        {
            StartTimeSync();
        }
    }

    public static long GetSyncedTime()
    {
        return GetLocalTimeMs() + _timeOffset;
    }

    public static long GetLocalTimeMs()
    {
        return (long) (Time.realtimeSinceStartup * 1000f);
    }

    public static void OnServerTimeResponse(long serverTime, long clientSendTime)
    {
        var clientReceiveTime = GetLocalTimeMs();
        var rtt = clientReceiveTime - clientSendTime;
        var halfRtt = rtt / 2L;

        long estimatedServerTime = serverTime + halfRtt;
        _timeOffset = estimatedServerTime - clientReceiveTime;
        _waitingForResponse = false;
        _lastSyncTime = GetSyncedTime();
        
        Action_OnPingUpdate?.Invoke((int) rtt);
    }

    private void StartTimeSync()
    {
        _waitingForResponse = true;
        ClientSend.RequestServerTime();
    }
}