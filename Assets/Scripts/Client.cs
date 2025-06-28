using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;

public class Client : ClientBase
{
    public static Client instance;
    public int myId = 0;
    public string myUsername;
    public override int port { get { return 26950; } }

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

        Debug.Log("Disconnceted from server.");
        
        GameManager.instance.Reset();
    }
}
