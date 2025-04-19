using System;
using System.Collections.Generic;
using System.Text;
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

                _ = ReceiveLoopAsync();

                ServerSend.Welcome(id, "Welcome to the server!");
            }

            private async Task ReceiveLoopAsync()
            {
                try
                {
                    while (true)
                    {
                        int byteLength = await stream.ReadAsync(receiveBuffer, 0, dataBufferSize);

                        if (byteLength <= 0)
                        {
                            Server.clients[id].Disconnect();
                            break;
                        }

                        byte[] data = new byte[byteLength];
                        Array.Copy(receiveBuffer, data, byteLength);

                        receivedData.Reset(HandleData(data));
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
                    if (socket != null && stream != null)
                    {
                        await stream.WriteAsync(packet.ToArray(), 0, packet.Length());
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

                    Packet packet = new Packet(packetBytes);
                    int packetId = packet.ReadInt();
                    Server.packetHandlers[packetId](id, packet);

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

                if (packetLength <= 0)
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
