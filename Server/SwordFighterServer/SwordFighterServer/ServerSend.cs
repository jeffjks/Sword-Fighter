using System;
using System.Collections.Generic;
using System.Text;
using static SwordFighterServer.Client;

namespace SwordFighterServer
{
    class ServerSend
    {
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendTCPDataToAll(int fromId, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                if (Server.IsReady(fromId)) // SpawnPlayer 패킷을 보낸 플레이어에게만 전송
                {
                    Server.clients[i].tcp.SendData(packet);
                }
            }
        }

        private static void SendTCPDataToAll(int exceptClient, int fromId, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                if (Server.IsReady(i)) // SpawnPlayer 패킷을 보낸 플레이어에게만 전송
                {
                    if (i != exceptClient)
                    {
                        Server.clients[i].tcp.SendData(packet);
                    }
                }
            }
        }

        #region Packets
        public static void Welcome(int toClient, string msg) {
            Packet packet = new Packet((int)ServerPackets.welcome);

            packet.Write(msg);
            packet.Write(toClient);

            SendTCPData(toClient, packet);
        }

        public static void SendServerTime(int toClient, long serverTime, long clientTime)
        {
            Packet packet = new Packet((int)ServerPackets.requestServerTime);

            packet.Write(serverTime);
            packet.Write(clientTime);

            SendTCPData(toClient, packet);
        }

        public static void SpawnPlayer(int toClient, Player player) // 플레이어 최초 생성 패킷 전달
        {
            Packet packet = new Packet((int)ServerPackets.spawnPlayer);

            packet.Write(player.id);
            packet.Write(player.username);
            packet.Write(player.position);
            packet.Write(player.direction);
            packet.Write(player.hitPoints);
            packet.Write(player.state);
            //packet.Write(Server.CurrentPlayers);

            SendTCPData(toClient, packet);
        }

        public static void UpdatePlayer(Player player, long timestamp) // 플레이어 움직임, 좌표, 방향 패킷 전달
        {
            Packet packet = new Packet((int)ServerPackets.updatePlayer);

            packet.Write(player.id);

            packet.Write(timestamp);
            packet.Write(player.position);
            packet.Write(player.direction);
            packet.Write(player.deltaPos);

            SendTCPDataToAll(player.id, packet);
            //SendTCPDataToAll(player.id, packet); // except
        }

        public static void PlayerState(Player player) // 플레이어 스킬 상태 패킷 전달
        {
            Packet packet = new Packet((int)ServerPackets.playerState);

            packet.Write(player.id);
            packet.Write(player.state);

            SendTCPDataToAll(player.id, packet);
        }

        public static void PlayerHp(Player player) // 플레이어 체력 패킷 전달
        {
            Packet packet = new Packet((int)ServerPackets.playerHp);

            packet.Write(player.id);
            packet.Write(player.hitPoints);

            SendTCPDataToAll(player.id, packet);
        }

        public static void PlayerDisconnected(int playerId) // 연결 끊김 패킷 전달
        {
            Packet packet = new Packet((int)ServerPackets.playerDisconnected);

            packet.Write(playerId);

            SendTCPDataToAll(playerId, playerId, packet);
        }
        #endregion
    }
}
