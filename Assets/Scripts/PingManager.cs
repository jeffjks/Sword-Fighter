using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

public class PingManager : MonoBehaviour
{
    private CancellationTokenSource _cts;

    private void Start()
    {
        StartPingLoop();
    }

    public void StartPingLoop()
    {
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ClientPing.PingLoop(Client.instance.defaultIp, _cts.Token));
    }

    public void StopPingLoop()
    {
        _cts?.Cancel();
    }

    private void OnDestroy()
    {
        StopPingLoop();
    }
}
