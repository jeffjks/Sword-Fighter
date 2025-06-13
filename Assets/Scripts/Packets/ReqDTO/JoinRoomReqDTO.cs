using Newtonsoft.Json;

public class JoinRoomReqDTO : IMessagePayload
{
    public int RoomID { get; set; }

    [JsonIgnore]
    public ChatServerPackets PacketType => ChatServerPackets.joinChatRoom;

    public JoinRoomReqDTO(int roomID)
    {
        RoomID = roomID;
    }
}