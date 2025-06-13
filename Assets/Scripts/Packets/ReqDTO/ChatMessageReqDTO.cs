using Newtonsoft.Json;

public class ChatMessageReqDTO : IMessagePayload
{
    public int UserID { get; set; }
    public string Message { get; set; }

    [JsonIgnore]
    public ChatServerPackets PacketType => ChatServerPackets.chatMessage;

    public ChatMessageReqDTO(int userID, string message)
    {
        UserID = userID;
        Message = message;
    }
}