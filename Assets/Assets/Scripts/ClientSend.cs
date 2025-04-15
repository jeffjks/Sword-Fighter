using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet) {
        if (GameManager.IsDebugPing)
        {
            SendTCPDataDelayed(packet).Forget();
        }
        else
        {
            packet.WriteLength(); // 패킷 가장 앞 부분에 패킷 길이 삽입 (패킷id보다 앞에)
            Client.instance.tcp.SendData(packet);
            packet.Dispose();
        }
    }

    private static async UniTaskVoid SendTCPDataDelayed(Packet packet)
    {
        int ping = GameManager.instance.GetDebugPing() / 2;

        if (ping > 0)
            await UniTask.Delay(ping);

        packet.WriteLength(); // 패킷 길이 쓰기
        Client.instance.tcp.SendData(packet);
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

    public static void PlayerSkill(long timestamp, PlayerSkill playerSkill, Vector3 direction) { // 움직임을 제외한 나머지 키 입력에 대한 패킷 (스킬 등)
        Packet packet = new ((int) ClientPackets.playerSkill);
        packet.Write(timestamp);
        packet.Write((int) playerSkill);
        packet.Write(direction);

        SendTCPData(packet);
    }

    public static void PlayerMovement(ClientInput clientInput, Vector3 position) { // 움직임에 관련된 키 입력에 대한 패킷
        Packet packet = new ((int) ClientPackets.playerMovement);
        packet.Write(clientInput.timestamp);
        packet.Write(clientInput.inputVector);
        packet.Write(clientInput.forwardDirection);
        packet.Write(clientInput.deltaPos);
        packet.Write(position);

        SendTCPData(packet);
    }

    public static void ChangeHp(int hitPoints, int targetPlayer) {
        Packet packet = new ((int) ClientPackets.changeHp);
        packet.Write(hitPoints);
        packet.Write(targetPlayer);

        SendTCPData(packet);
    }
    #endregion
}