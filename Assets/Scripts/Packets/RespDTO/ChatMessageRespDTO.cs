using Newtonsoft.Json;

public class ChatMessageRespDTO : IMessagePayload
{
    public int UserID { get; set; }
    public string Message { get; set; }

    [JsonIgnore]
    public ChatServerPackets PacketType => ChatServerPackets.chatMessage;

    public ChatMessageRespDTO(int userID, string message)
    {
        UserID = userID;
        Message = message;
    }
}