using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

// Begin-End 모델?

namespace SwordFighterServer
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static int CurrentPlayers;
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>(); // 클라이언트 관리용 Dictionary
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;

        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting Server...");
            InitializeServerData();

            // 서버 포트 설정 및 비동기 Listening 시작
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started on {Port}.");
        }

        private static void TCPConnectCallback(IAsyncResult result) // 클라이언트 접속
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}");
            
            for (int i = 1; i <= MaxPlayers; i++) // 비어있는 가장 첫 clients Dictionary에 배정
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    CurrentPlayers++;
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
            client.Close();
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++) // 최대 플레이어 수 만큼 미리 Dictionary에 clients 생성
            {
                clients.Add(i, new Client(i));
            }
                
            packetHandlers = new Dictionary<int, PacketHandler>() // 패킷 종류에 따른 함수 포인터 설정
            {
                { (int) ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int) ClientPackets.spawnPlayerReceived, ServerHandle.SpawnPlayerReceived },
                { (int) ClientPackets.playerInput, ServerHandle.PlayerInput },
                { (int) ClientPackets.playerMovement, ServerHandle.PlayerMovement },
                { (int) ClientPackets.changeHp, ServerHandle.ChangeHp },
            };
            Console.WriteLine("Initialized packets.");
        }
    }
}
