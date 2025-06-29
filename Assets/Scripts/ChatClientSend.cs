using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

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

    private static void SendTCPData(byte[] data) {
        SendTCPDataAsync(data).Forget();
    }

    private static async UniTaskVoid SendTCPDataAsync(byte[] data)
    {
        await ChatClient.instance.tcp.SendDataAsync(data);

        if (GameManager.instance.m_DebugTestClients)
        {
            foreach (var client in ChatClient.ChatLoadTestClients)
            {
                await client.tcp.SendDataAsync(data);
            }
        }
    }

    #region Packets
    public static void SendNetworkMessage<T>(T payload) where T : IMessagePayload
    {
        var networkMessage = new NetworkMessage<T>(payload.PacketType, payload);

        string json = JsonConvert.SerializeObject(networkMessage);
        byte[] data = Encoding.UTF8.GetBytes(json + "\n");

        SendTCPData(data);
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