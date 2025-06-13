using System;
using Newtonsoft.Json;

public interface IMessagePayload {
    public ChatServerPackets PacketType { get; }
}

[Serializable]
public class NetworkMessage<T> where T : IMessagePayload
{
    public ChatServerPackets Type;
    public T Payload { get; set; }

    [JsonIgnore]
    public ChatServerPackets PacketType => ChatServerPackets.none;

    public NetworkMessage(ChatServerPackets type, T payload)
    {
        Type = type;
        Payload = payload;
    }
}