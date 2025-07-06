[System.Serializable]
public class CharacterStatus
{
    public TimedState BlockState = new();
    public int CurrentHitPoint;
    public int MaxHitPoint;

    public void Tick(float delta)
    {
        BlockState.Tick(delta);
    }
}