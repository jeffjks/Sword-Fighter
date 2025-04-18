using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ChatClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet) {
        SendTCPDataAsync(packet).Forget();
    }

    private static async UniTaskVoid SendTCPDataAsync(Packet packet)
    {
        packet.WriteLength(); // 패킷 길이 쓰기
        await Client.instance.tcp.SendDataAsync(packet);
        packet.Dispose();
    }

    #region Packets
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
    }
    #endregion
}