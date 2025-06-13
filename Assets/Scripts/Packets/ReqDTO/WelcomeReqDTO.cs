using Newtonsoft.Json;

public class WelcomeReqDTO : IMessagePayload
{
    public int UserID { get; set; }

    [JsonIgnore]
    public ChatServerPackets PacketType => ChatServerPackets.welcome;

    public WelcomeReqDTO(int userID)
    {
        UserID = userID;
    }
}