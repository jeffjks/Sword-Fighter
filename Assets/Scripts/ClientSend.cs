using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.IO;
using Shared.Enums;

public class ClientSend : MonoBehaviour
{
    private static int SeqNum;

    private static void SendTCPData(Packet packet) {
        SendTCPDataAsync(packet).Forget();
    }

    private static async UniTaskVoid SendTCPDataAsync(Packet packet)
    {
        int ping = GameManager.instance.GetDebugPing() / 2;

        if (ping > 0)
            await UniTask.Delay(ping);

        packet.WriteLength(); // 패킷 길이 쓰기
        await Client.instance.tcp.SendDataAsync(packet);
        packet.Dispose();
    }

    #region Packets
    public static void WelcomeReceived() {
        Packet packet = new ((int) ClientPackets.welcomeReceived);
        packet.Write(Client.instance.myId);
        packet.Write(UIManager.instance.m_UsernameField.text);

        SendTCPData(packet);
    }

    public static void RequestServerTime() {
        Packet packet = new ((int) ClientPackets.requestServerTime);
        long clientTime = TimeSync.GetLocalUnixTime();
        packet.Write(clientTime);
        SendTCPData(packet);
    }

    public static void SpawnPlayerReceived(int id) {
        Packet packet = new ((int) ClientPackets.spawnPlayerReceived);
        packet.Write(id);

        SendTCPData(packet);
    }

    public static void PlayerSkill(long timestamp, Vector3 facingDirection, PlayerSkill playerSkill) { // 움직임을 제외한 나머지 키 입력에 대한 패킷 (스킬 등)
        Packet packet = new ((int) ClientPackets.playerSkill);

        var tempSeqNum = SeqNum;
        packet.Write(SeqNum++);
        packet.Write(timestamp);
        packet.Write(facingDirection);
        packet.Write((int) playerSkill);

        SendTCPData(packet);

#if UNITY_EDITOR
        using (StreamWriter writer = new ($"{GameManager.dirSend}/send.txt", append: true))
        {
            writer.WriteLine($"[{tempSeqNum}, {timestamp}] ClientSend (PlayerSkill): {playerSkill}");
        }
#endif
    }

    public static void PlayerMovement(long timestamp, Vector3 facingDirection, Vector3 deltaPos, Vector2 inputVector) { // 움직임에 관련된 키 입력에 대한 패킷
        Packet packet = new ((int) ClientPackets.playerMovement);

        var tempSeqNum = SeqNum;
        packet.Write(SeqNum++);
        packet.Write(timestamp);
        packet.Write(facingDirection);
        packet.Write(deltaPos);
        packet.Write(inputVector);

        SendTCPData(packet);

#if UNITY_EDITOR
        if (deltaPos != Vector3.zero)
        {
            using (StreamWriter writer = new ($"{GameManager.dirSend}/send.txt", append: true))
            {
                writer.WriteLine($"[{tempSeqNum}, {timestamp}] ClientSend: {deltaPos} (inputVector: {inputVector})");
            }
        }
#endif
    }

    public static void ChangeHp(int hitPoints, int targetPlayer) {
        Packet packet = new ((int) ClientPackets.changeHp);
        packet.Write(hitPoints);
        packet.Write(targetPlayer);

        SendTCPData(packet);
    }
    #endregion
}