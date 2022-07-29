using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet) {
        packet.WriteLength(); // 패킷 가장 앞 부분에 패킷 길이 삽입 (패킷id보다 앞에)
        ChatClient.instance.tcp.SendData(packet);
    }

    #region Packets
    public static void WelcomeMessageReceived() { // Unused
        using (Packet packet = new Packet((int) ChatClientPackets.welcomeMessageReceived)) { // 패킷 생성 시 가장 앞 부분에 패킷id(종류) 삽입
            packet.Write(Client.instance.myId);

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