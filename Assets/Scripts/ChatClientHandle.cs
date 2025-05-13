using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ClientState {
    CLIENT_JOINED = 1,
    CLIENT_LEFT = 2,
};

public class ChatClientHandle : MonoBehaviour
{
    public static void WelcomeMessage(Packet packet) {
        ChatServerMessage(packet);
        
        ChatClientSend.WelcomeMessageReceived();
    }
    
    public static void ChatServerMessage(Packet packet) { // msg 후 toClient 읽기
        int id = packet.ReadInt();
        string msg = packet.ReadString();

        //Debug.Log($"Message from {id}: {msg}");
        
        ChatClient.instance.GetMessageFromServer(id, msg);
    }

    public static void ClientStateMessage(Packet packet) {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        int state = packet.ReadInt();
        string msg;

        if (state == (int) ClientState.CLIENT_JOINED) {
            msg = $"{username} 님이 접속하셨습니다.";
        }
        else if (state == (int) ClientState.CLIENT_LEFT) {
            msg = $"{username} 님이 접속을 종료하셨습니다.";
        }
        else {
            return;
        }

        ChatClient.instance.GetMessageFromServer((int) MessageType.SYSTEM_MESSAGE, msg);
    }
}
