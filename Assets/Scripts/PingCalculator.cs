using UnityEngine;
using Cysharp.Threading.Tasks;

public class PingCalculator : MonoBehaviour
{
    private int _pingTimer;
    private static uint Seq;

    private const int RequestInterval = 2000;
    private const int MaxSeq = 255;

    private void Update()
    {
        if (_pingTimer >= RequestInterval)
        {
            SendPint();
            _pingTimer = 0;
        }

        _pingTimer += (int) (Time.deltaTime * 1000f);
    }

    private void SendPint()
    {
        ClientPing.SendPingAsync(Client.instance.defaultIp, Seq).Forget();
        Seq++;

        if (Seq > MaxSeq)
        {
            Seq = 0;
        }
    }
}
