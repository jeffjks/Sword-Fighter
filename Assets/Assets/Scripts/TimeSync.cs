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

    public static void SetServerTime(long serverTime, long clientSendTime)
    {
        var clientReceiveTime = GetLocalUnixTime();
        var rtt = clientReceiveTime - clientSendTime;
        var oneWay = rtt / 2L;

        long estimatedServerTime = serverTime + oneWay;
        _timeOffset = estimatedServerTime - clientReceiveTime;
        _waitingForResponse = false;
        _lastSyncTime = GetSyncTime();

        Debug.Log($"[TimeSync] {serverTime}, {clientSendTime} / {clientReceiveTime}");
    }

    private void SyncServerTime()
    {
        _waitingForResponse = true;
        ClientSend.RequestServerTime();
    }
}