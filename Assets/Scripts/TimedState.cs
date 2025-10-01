public class TimedState
{
    public bool IsActive => Remaining > 0f;
    public float Remaining;

    public void StartTimer(float duration)
    {
        Remaining = duration;
    }

    public void Tick(float delta)
    {
        if (Remaining > 0f)
        {
            Remaining -= delta;
            if (Remaining <= 0f)
            {
                Remaining = 0f;
                SendBlockEndToServer();
            }
        }
    }

    public void CancelTimer()
    {
        Remaining = 0f;
    }

    public void SendBlockEndToServer()
    {
        var timestamp = TimeSync.GetSyncedTime();
        ClientSend.SetBlockState(timestamp, false);
    }
}