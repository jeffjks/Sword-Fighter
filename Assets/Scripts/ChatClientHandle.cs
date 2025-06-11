using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

enum ClientState {
    CLIENT_JOINED = 1,
    CLIENT_LEFT = 2,
};

public class ChatClientHandle : MonoBehaviour
{
    public static void GetChatMessage(JToken token) {
        var chatMessage = token.ToObject<ChatMessage>();
        ChatClient.instance.HandleChatMessage(chatMessage.UserID, chatMessage.Message);
    }

    public static void JoinChatSession(JToken payload) {
        /*
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

        ChatClient.instance.HandleChatMessage((int) MessageType.SYSTEM_MESSAGE, msg);
        */
    }

    public static void LeaveChatSession(JToken payload) {
    }
}
