using System;
using System.Collections.Generic;
using System.Text;

namespace SwordFighterServer
{
    class ServerSend
    {
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }

        private static void SendTCPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].tcp.SendData(packet);
                }
            }
        }

        #region Packets
        public static void Welcome(int toClient, string msg) {
            using (Packet packet = new Packet((int) ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static void SpawnPlayer(int toClient, Player player) // 플레이어 최초 생성 패킷 전달
        {
            using (Packet packet = new Packet((int) ServerPackets.spawnPlayer))
            {
                packet.Write(player.id);
                packet.Write(player.username);
                packet.Write(player.position);
                packet.Write(player.direction);
                //Console.WriteLine($"(To {toClient} > SpawnPlayer: {player.id}");

                SendTCPData(toClient, packet);
            }
        }

        public static void PlayerMovement(Player player) // 플레이어 움직임, 좌표, 방향 패킷 전달 (자신에게는 제외)
        {
            using (Packet packet = new Packet((int) ServerPackets.playerMovement))
            {
                packet.Write(player.id);

                packet.Write(player.movement);
                packet.Write(player.position);
                packet.Write(player.direction);

                SendTCPDataToAll(player.id, packet); // except
            }
        }

        public static void PlayerState(Player player) // 플레이어 스킬 상태 패킷 전달
        {
            using (Packet packet = new Packet((int) ServerPackets.playerState))
            {
                packet.Write(player.id);

                packet.Write(player.state);

                SendTCPDataToAll(packet);
            }
        }

        public static void PlayerHp(Player player) // 플레이어 체력 패킷 전달
        {
            using (Packet packet = new Packet((int) ServerPackets.playerHp))
            {
                packet.Write(player.id);

                packet.Write(player.hitPoints);

                SendTCPDataToAll(packet);
            }
        }

        public static void PlayerDisconnected(int playerId) // 연결 끊김 패킷 전달
        {
            using (Packet packet = new Packet((int) ServerPackets.playerDisconnected))
            {
                packet.Write(playerId);

                SendTCPDataToAll(packet);
            }
        }
        #endregion
    }
}
