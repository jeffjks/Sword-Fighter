using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChatClient : ChatClientBase
{
    public static ChatClient instance;
    public UI_ChatWindow m_UI_ChatWindow;
    public override int port { get { return 26960; } }

    protected override void Awake() // Singleton
    {
        base.Awake();

        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists. Destroying object!");
            Destroy(this);
        }
    }

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
        m_UI_ChatWindow.PushSpecialMessage(MessageType.SYSTEM_MESSAGE, "채팅 서버에 접속하였습니다.");
    }

    public void HandleChatMessage(int fromId, string message) {
        m_UI_ChatWindow.PushTextMessage(fromId, message);
    }
}
