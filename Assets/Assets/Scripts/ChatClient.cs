using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;

public class ChatClient : ClientBase
{
    public static ChatClient instance;
    public UI_ChatWindow m_UI_ChatWindow;
    public override int port { get { return 26960; } }

    protected override Dictionary<int, PacketHandler> packetHandlers { get; set; }

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

    protected override void InitializeClientData() {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int) ChatServerPackets.welcomeMessage, ChatClientHandle.WelcomeMessage },
            { (int) ChatServerPackets.chatServerMessage, ChatClientHandle.ChatServerMessage },
        };
        Debug.Log("Initialize packets.");
    }

    public override void Disconnect() {
        if (!isConnected) {
            return;
        }
        base.Disconnect();

        Debug.Log("Disconnceted from chat server.");
    }

    public void GetTextMessage(int fromId, string message) {
        m_UI_ChatWindow.PushTextMessage(fromId, message);
    }
}
