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
                Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");

            Server.spawnedPlayers.Add(fromClient);
            Server.clients[fromClient].SendIntoGame(username);
        }

        public static void RequestServerTime(int fromClient, Packet packet)
        {
            long clientTime = packet.ReadLong();

            Server.clients[fromClient].SendServerTime(clientTime);
        }

        public static void SpawnPlayerReceived(int fromClient, Packet packet)
        {
            int spawnedPlayerId = packet.ReadInt();

            Server.spawnedPlayers.Add(spawnedPlayerId);

            //Server.clients[fromClient].SetReady(spawnedPlayerId);
        }

        public static void PlayerSkill(int fromClient, Packet packet) // 플레이어의 스킬 사용
        {
            var timestamp = packet.ReadLong();
            var playerSkill = packet.ReadInt();
            var direction = packet.ReadVector3();

            if (Server.clients[fromClient].player != null)
            {
                Server.clients[fromClient].player.ExecutePlayerSkill(timestamp, (PlayerSkill) playerSkill, direction);
            }
        }

        public static void PlayerMovement(int fromClient, Packet packet) // 플레이어의 움직임, 좌표, 방향 벡터
        {
            ClientInput clientInput = new ClientInput()
            {
                timestamp = packet.ReadLong(),
                movementRaw = packet.ReadVector2(),
                forwardDirection = packet.ReadVector3(),
                deltaPos = packet.ReadVector3()
            };

            Vector3 position = packet.ReadVector3();
            //Quaternion rotation = packet.ReadQuaternion();

            if (Server.clients[fromClient].player != null)
            {
                Server.clients[fromClient].player.SetMovement(clientInput, position);
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
