using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        
        ChatClient.instance.GetTextMessage(id, msg);
    }
}
