using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;

namespace SwordFighterServer
{
    public class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public Player player;
        public TCP tcp;

        public bool isConnected = false;

        public Client(int id)
        {
            this.id = id;
            tcp = new TCP(this, id);
        }

        public class TCP
        {
            public TcpClient _socket;

            private readonly Client instance;
            private readonly int id;
            private NetworkStream _stream;
            private Packet _receivedData;
            private byte[] _receiveBuffer;

            public TCP(Client client, int id)
            {
                instance = client;
                this.id = id;
            }

            public void Connect(TcpClient socket)
            {
                _socket = socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                _stream = socket.GetStream();

                _receivedData = new Packet();
                _receiveBuffer = new byte[dataBufferSize];

                _ = ReceiveLoopAsync();

                ServerSend.Welcome(id, "Welcome to the server!");
            }

            private async Task ReceiveLoopAsync()
            {
                try
                {
                    while (true)
                    {
                        int byteLength = await _stream.ReadAsync(_receiveBuffer, 0, dataBufferSize);

                        if (byteLength <= 0)
                        {
                            Server.clients[id].Disconnect();
                            break;
                        }

                        byte[] data = new byte[byteLength];
                        Array.Copy(_receiveBuffer, data, byteLength);

                        _receivedData.Reset(HandleData(data));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error receiving TCP data: {e}");
                    Server.clients[id].Disconnect();
                }
            }

            public async Task SendDataAsync(Packet packet)
            {
                try
                {
                    if (_socket != null && _stream != null)
                    {
                        await _stream.WriteAsync(packet.ToArray(), 0, packet.Length());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {e}");
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                _receivedData.SetBytes(data); // Add data

                if (_receivedData.UnreadLength() >= 4) // Read packetLength
                {
                    packetLength = _receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
                {
                    byte[] packetBytes = _receivedData.ReadBytes(packetLength); // receivedData에서 packetLength만큼 다 읽음

                    Packet packet = new Packet(packetBytes);
                    int packetId = packet.ReadInt();
                    Server.packetHandlers[packetId](id, packet);

                    packetLength = 0;
                    if (_receivedData.UnreadLength() >= 4) // byte가 남았다면 packetLength를 읽고 다시 읽기 진행
                    {
                        packetLength = _receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 0)
                {
                    return true;
                }

                return false;
            }

            public void Disconnect()
            {
                _stream?.Close();
                _stream = null;
                _socket?.Close();
                _socket = null;

                _receivedData = null;
                _receiveBuffer = null;

                instance.isConnected = false;
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
                }
            }

            foreach (Client client in Server.clients.Values) // 자신 캐릭 생성 (모든 유저에게 전달)
            {
                if (client.player != null)
                {
                    ServerSend.SpawnPlayer(client.id, player);
                }
            }
        }

        public void SendServerTime(long clientTime)
        {
            var serverTime = Server.ElapsedMs;

            ServerSend.SendServerTime(id, serverTime, clientTime);
        }

        private void Disconnect()
        {
            if (isConnected == false)
                return;
            isConnected = false;

            Console.WriteLine($"{tcp._socket.Client.RemoteEndPoint} has disconnected.");

            player = null;
            tcp.Disconnect();
            Server.spawnedPlayers.Remove(id);

            ServerSend.PlayerDisconnected(id);
            Server.CurrentPlayers--;
        }
    }
}
