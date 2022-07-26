using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

// 클라이언트로부터 받은 패킷 Handle

namespace SwordFighterServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }

            Server.clients[fromClient].SendIntoGame(username);
        }

        public static void SpawnPlayerReceived(int fromClient, Packet packet)
        {
            int spawnedPlayerId = packet.ReadInt();

            Server.clients[fromClient].SetReady(spawnedPlayerId);
        }

        public static void PlayerInput(int fromClient, Packet packet) // 플레이어의 스킬 사용 input
        {
            bool[] inputs = new bool[packet.ReadInt()];
            for (int i = 0; i < inputs.Length; ++i)
            {
                inputs[i] = packet.ReadBool();
            }

            if (Server.clients[fromClient].player != null)
            {
                Server.clients[fromClient].player.SetInput(inputs);
            }
        }

        public static void PlayerMovement(int fromClient, Packet packet) // 플레이어의 움직임, 좌표, 방향 벡터
        {
            Vector2 movement = packet.ReadVector2();

            ClientInput clientInput = new ClientInput()
            {
                seqNum = packet.ReadInt(),
                horizontal_raw = packet.ReadInt(),
                vertical_raw = packet.ReadInt(),
                cam_forward = packet.ReadVector3()
            };

            //Vector3 position = packet.ReadVector3();
            Vector3 direction = packet.ReadVector3();
            //Quaternion rotation = packet.ReadQuaternion();

            if (Server.clients[fromClient].player != null)
            {
                Server.clients[fromClient].player.SetMovement(movement, clientInput, direction);
            }
        }

        public static void ChangeHp(int fromClient, Packet packet) // 피격 판정
        {
            int hitPoints = packet.ReadInt();
            int targetPlayer = packet.ReadInt();

            if (Server.clients[targetPlayer].player != null)
            {
                Server.clients[targetPlayer].player.ChangePlayerHp(fromClient, hitPoints);
            }
        }
    }
}
