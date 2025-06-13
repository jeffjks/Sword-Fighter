using Newtonsoft.Json;

public class WelcomeRespDTO : IMessagePayload
{
    [JsonIgnore]
    public ChatServerPackets PacketType => ChatServerPackets.welcome;

    public WelcomeRespDTO()
    {
    }
}