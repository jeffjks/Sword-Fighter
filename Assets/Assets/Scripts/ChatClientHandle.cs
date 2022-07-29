using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatClientHandle : MonoBehaviour
{
    public static void WelcomeMessage(Packet packet) {
        int id = packet.ReadInt();
        //ChatServerMessage(packet);
        
        ChatClientSend.WelcomeMessageReceived();
    }
    
    public static void ChatServerMessage(Packet packet) { // msg 후 toClient 읽기
        int id = packet.ReadInt();
        string msg = packet.ReadString();

        Debug.Log($"Message from {id}: {msg}");
        //Client.instance.myId = id;

        //ClientSend.WelcomeReceived();
        ChatClient.instance.GetTextMessage(id, msg);
    }
}
