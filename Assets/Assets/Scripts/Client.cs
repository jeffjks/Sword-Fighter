﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string defaultIp = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public int oppositeId = 0; // TEMP
    public TCP tcp;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake() // Singleton
    {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists. Destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();
    }

    private void OnApplicationQuit() {
        Disconnect();
    }

    public void ConnectToServer(string ip) {
        InitializeClientData();

        isConnected = true;
        tcp.Connect(ip);
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect(string ip) {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            IAsyncResult result = socket.BeginConnect(System.Net.IPAddress.Parse(ip), instance.port, ConnectCallback, socket);

            result.AsyncWaitHandle.WaitOne(5000, true);

            if (!socket.Connected) {
                socket.Close();
                throw new TimeoutException();
            }
        }

        private void ConnectCallback(IAsyncResult result) {
            socket.EndConnect(result);

            if (!socket.Connected) {
                return;
            }

            stream = socket.GetStream();
            receivedData = new Packet();
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet packet) {
            try {
                if (socket != null) {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e) {
                Debug.Log($"Error sending data to server via TCP: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            try {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0) {
                    Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data)); 
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch {
                Disconnect();
            }
        }
        
        // TCP는 데이터의 순서를 보장해주지만 데이터의 크기까지는 보장하지 않는다. 받은 데이터가 데이터 전체인지 일부분만인지 체크 필요
        private bool HandleData(byte[] data) {
            int packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4) {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0) {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= receivedData.UnreadLength()) {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes)) {
                        int packetId = packet.ReadInt();
                        packetHandlers[packetId](packet);
                    }
                });

                packetLength = 0;

                if (receivedData.UnreadLength() >= 4) {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0) {
                        return true;
                    }
                }
            }

            if (packetLength <= 1) {
                return true;
            }
            return false;
        }

        private void Disconnect() {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    private void InitializeClientData() {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int) ServerPackets.welcome, ClientHandle.Welcome },
            { (int) ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            { (int) ServerPackets.playerMovement, ClientHandle.PlayerMovement },
            { (int) ServerPackets.playerState, ClientHandle.PlayerState },
            { (int) ServerPackets.playerHp, ClientHandle.PlayerHp },
            { (int) ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
        };
        Debug.Log("Initialize packets.");
    }

    private void Disconnect() {
        if (isConnected) {
            isConnected = false;
            tcp.socket.Close();

            Debug.Log("Disconnceted from server.");
        }
    }
}