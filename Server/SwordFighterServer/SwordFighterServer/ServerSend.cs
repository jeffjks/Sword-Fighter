using System;
using System.Collections.Generic;
using System.Numerics;

namespace SwordFighterServer
{
    class ServerSend
    {
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            _ = Server.clients[toClient].tcp.SendDataAsync(packet);
        }

        private static void SendTCPDataToAll(int fromId, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                if (Server.IsReady(fromId)) // SpawnPlayer 패킷을 보낸 플레이어에게만 전송
                {
                    _ = Server.clients[i].tcp.SendDataAsync(packet);
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
                        _ = Server.clients[i].tcp.SendDataAsync(packet);
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
            packet.Write((int) player.currentState);

            SendTCPData(toClient, packet);
        }

        public static void UpdatePlayerPosition(int seqNum, Player player, long timestamp, bool isBroadcasting = false) // 플레이어 움직임, 좌표, 방향 패킷 전달
        {
            Packet packet = new Packet((int)ServerPackets.updatePlayerPosition);

            packet.Write(player.id);

            packet.Write(seqNum);
            packet.Write(timestamp);
            packet.Write(player.direction);
            packet.Write(player.deltaPos);
            packet.Write(player.inputVector);
            packet.Write(player.position);

            if (isBroadcasting)
                SendTCPDataToAll(player.id, player.id, packet);
            else
                SendTCPData(player.id, packet);
        }

        public static void PlayerSkill(int playerId, long timestamp, PlayerSkill playerSkill, Vector3 facingDirection, Vector3 targetPosition) // 플레이어 스킬
        {
            Packet packet = new Packet((int)ServerPackets.playerSkill);

            packet.Write(playerId);

            packet.Write(timestamp);
            packet.Write((int)playerSkill);
            packet.Write(facingDirection);
            packet.Write(targetPosition);

            SendTCPDataToAll(playerId, playerId, packet);
        }

        public static void PlayerState(Player player) // 플레이어 상태
        {
            Packet packet = new Packet((int)ServerPackets.playerState);

            packet.Write(player.id);
            packet.Write((int) player.currentState);

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
