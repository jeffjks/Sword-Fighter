﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace SwordFighterServer
{
    class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public Player player;
        public TCP tcp;

        public Client(int id)
        {
            this.id = id;
            tcp = new TCP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int id)
            {
                this.id = id;
            }

            public void Connect(TcpClient socket)
            {
                this.socket = socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {e}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data)); // handle data. true -> reset, false -> unread (4 bytes)
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error receiving TCP data: {e}");
                    Server.clients[id].Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                receivedData.SetBytes(data); // Add data

                if (receivedData.UnreadLength() >= 4) // Read packetLength
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength); // receivedData에서 packetLength만큼 다 읽음
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        Packet packet = new Packet(packetBytes);
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](id, packet);
                    });

                    packetLength = 0;
                    if (receivedData.UnreadLength() >= 4) // byte가 남았다면 packetLength를 읽고 다시 읽기 진행
                    {
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }



        public void SendIntoGame(string username) // 플레이어 접속 시 SpawnPlayer 패킷 전달
        {
            player = new Player(id, username, new Vector3(0, 0, 0));

            foreach (Client client in Server.clients.Values) // 상대방 캐릭 생성
            {
                if (client.player != null && client.id != id)
                {
                    ServerSend.SpawnPlayer(id, client.player);
                    //Console.WriteLine($"SendIntoGame1 - SpawnPlayer: {id}, {client.player.username}");
                    //client.SetReady(client.player.id);
                    //SetReady(client.player.id);
                }
            }

            foreach (Client client in Server.clients.Values) // 자신 캐릭 생성 (모든 유저에게 전달)
            {
                if (client.player != null)
                {
                    ServerSend.SpawnPlayer(client.id, player);
                    //Console.WriteLine($"SendIntoGame2 - SpawnPlayer: {client.id}, {username}");
                    //client.SetReady(player.id);
                }
            }
        }

        public void SendServerTime(long clientTime)
        {
            var serverTime = Server.GetUnixTime();

            ServerSend.SendServerTime(id, serverTime, clientTime);
        }

        private void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            player = null;
            tcp.Disconnect();
            Server.spawnedPlayers.Remove(id);

            ServerSend.PlayerDisconnected(id);
            Server.CurrentPlayers--;
        }
    }
}
