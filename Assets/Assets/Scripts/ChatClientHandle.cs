using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatClientHandle : MonoBehaviour
{
    // Unused
    public static void GetUserId(Packet packet) {
        ChatClientSend.SendUserId();
    }
    
    public static void MessageReceived(Packet packet) { // msg 후 toClient 읽기
        int id = packet.ReadInt();
        string msg = packet.ReadString();

        //Debug.Log($"Message from {id}: {msg}");
        //Client.instance.myId = id;

        //ClientSend.WelcomeReceived();
        ChatClient.instance.GetTextMessage(id, msg);
    }
}
