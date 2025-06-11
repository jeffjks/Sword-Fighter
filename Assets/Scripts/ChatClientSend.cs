using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using Newtonsoft.Json;

public interface IMessagePayload {}

public class ChatMessage : IMessagePayload
{
    public int UserID { get; set; }
    public string Message { get; set; }

    public ChatMessage(int userID, string message)
    {
        UserID = userID;
        Message = message;
    }
}

public class JoinRoomMessage : IMessagePayload
{
    public int RoomID { get; set; }

    public JoinRoomMessage(int roomID)
    {
        RoomID = roomID;
    }
}

[Serializable]
public class NetworkMessage<T> where T : IMessagePayload
{
    public ChatServerPackets Type { get; set; }
    public T Payload { get; set; }

    public NetworkMessage(ChatServerPackets type, T payload)
    {
        Type = type;
        Payload = payload;
    }
}

public class ChatClientSend : MonoBehaviour
{
    /*
    private static void SendTCPData(Packet packet) {
        SendTCPDataAsync(packet).Forget();
    }

    private static async UniTaskVoid SendTCPDataAsync(Packet packet)
    {
        packet.WriteLength(); // 패킷 길이 쓰기
        await Client.instance.tcp.SendDataAsync(packet);
        packet.Dispose();
    }*/

    public static void SendChatMessage<T>(T payload) where T : IMessagePayload
    {
        SendChatMessageAsync(payload).Forget();
    }

    #region Packets
    private static async UniTaskVoid SendChatMessageAsync<T>(T payload) where T : IMessagePayload
    {
        var networkMessage = new NetworkMessage<T>(ChatServerPackets.chatMessage, payload);

        string json = JsonConvert.SerializeObject(networkMessage);
        byte[] data = Encoding.UTF8.GetBytes(json + "\n");

        await ChatClient.instance.tcp.SendDataAsync(data);
    }
    /*
    public static void WelcomeMessageReceived() {
        int myId = Client.instance.myId;
        using (Packet packet = new Packet((int) ChatClientPackets.welcomeMessageReceived)) { // 패킷 생성 시 가장 앞 부분에 패킷id(종류) 삽입
            packet.Write(myId);
            packet.Write(Client.instance.myUsername);

            SendTCPData(packet);
        }
    }

    public static void SendChatMessage(int fromId, string message) {
        using (Packet packet = new Packet((int) ChatClientPackets.chatClientMessage)) { // 패킷 생성 시 가장 앞 부분에 패킷id(종류) 삽입
            packet.Write(fromId);
            packet.Write(message);
            
            SendTCPData(packet);
        }
    }*/
    #endregion
}