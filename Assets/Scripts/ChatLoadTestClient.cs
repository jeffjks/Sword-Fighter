using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;

public class ChatLoadTestClient : ChatClientBase
{
    public UI_ChatWindow m_UI_ChatWindow;
    public override int port { get { return 26960; } }

    public override void Disconnect() {
        if (!isConnected) {
            return;
        }
        base.Disconnect();

        Debug.Log("Disconnceted from chat server.");
    }

    public void EnableChat()
    {
        isReceivedWelcomeMessage = true;
        m_UI_ChatWindow.PushSpecialMessage(MessageType.SYSTEM_MESSAGE, "채팅 서버에 접속하였습니다. (테스트 클라이언트)");
    }

    public void HandleChatMessage(int fromId, string message) {
        m_UI_ChatWindow.PushTextMessage(fromId, message);
    }
}
