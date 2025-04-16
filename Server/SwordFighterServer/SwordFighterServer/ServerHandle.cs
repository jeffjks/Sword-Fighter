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
            int seqNum = packet.ReadInt();
            var timestamp = packet.ReadLong();
            var facingDirection = packet.ReadVector3();
            var playerSkill = (PlayerSkill) packet.ReadInt();

            var skillInput = new SkillInput(timestamp, facingDirection, playerSkill);
            skillInput.SeqNum = seqNum;

            if (Server.clients[fromClient].player != null)
            {
                Server.clients[fromClient].player.AddClientInput(skillInput);
            }
        }

        public static void PlayerMovement(int fromClient, Packet packet) // 플레이어의 움직임, 좌표, 방향 벡터
        {
            int seqNum = packet.ReadInt();
            var timestamp = packet.ReadLong();
            var facingDirection = packet.ReadVector3();
            var deltaPos = packet.ReadVector3();
            var inputVector = packet.ReadVector2();

            var moveInput = new MoveInput(timestamp, facingDirection, deltaPos, inputVector);
            moveInput.SeqNum = seqNum;

            //Quaternion rotation = packet.ReadQuaternion();

            if (Server.clients[fromClient].player != null)
            {
                Server.clients[fromClient].player.AddClientInput(moveInput);
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
