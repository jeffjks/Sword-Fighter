using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatClientHandle : MonoBehaviour
{
    const int SERVER_MESSAGE = 127;
    const int ADMIN_MESSAGE = 126;

    public static void Temp() {
        using (Packet packet = new Packet((int) ChatClientPackets.chatMessage)) { // 패킷 생성 시 가장 앞 부분에 패킷id(종류) 삽입
            packet.Write("Test Message");

            packet.WriteLength(); // 패킷 가장 앞 부분에 패킷 길이 삽입 (패킷id보다 앞에)
            ChatClient.instance.tcp.SendData(packet);
        }
    }
    
    public static void MessageReceived(Packet packet) { // msg 후 toClient 읽기
        int id = packet.ReadInt();
        string msg = packet.ReadString();

        Debug.Log($"Message from {id}: {msg}");
        //Client.instance.myId = id;

        //ClientSend.WelcomeReceived();
        //Temp();
        ChatClient.instance.GetTextMessage($"Message from {id}: {msg}");
    }
}
