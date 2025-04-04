using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSync : MonoBehaviour
{
    private static float _lastSyncTime;
    private static float _timeOffset;
    private static float _clientTime;
    private static bool _waitingForResponse;

    private const float RequestInterval = 5f;

    private void Start()
    {
        _clientTime = Time.time;
        _lastSyncTime = Time.time;
    }

    private void Update()
    {
        if (!_waitingForResponse && Time.time - _lastSyncTime >= RequestInterval)
        {
            SyncServerTime();
        }

        // 클라이언트 시간이 서버 시간 기준으로 동기화되는 부분 (서버와 비교)
        if (_timeOffset != 0)
        {
            _clientTime = Time.time + _timeOffset;
        }
    }

    // 현재 클라이언트의 시간을 반환 (서버 시간 기준)
    public static float GetSyncTime()
    {
        return _clientTime;
    }

    // 서버로부터 동기화된 시간 받는 예시 메소드
    public static void SetServerTime(float serverTime)
    {
        _timeOffset = serverTime - Time.time;
        _clientTime = Time.time + _timeOffset;
        _lastSyncTime = Time.time;
        _waitingForResponse = false;
    }

    private void SyncServerTime()
    {
        _waitingForResponse = true;
        ClientSend.RequestServerTime();
    }
}