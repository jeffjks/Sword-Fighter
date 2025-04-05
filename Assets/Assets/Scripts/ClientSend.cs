using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet) {
        packet.WriteLength(); // 패킷 가장 앞 부분에 패킷 길이 삽입 (패킷id보다 앞에)
        Client.instance.tcp.SendData(packet);
    }

    #region Packets
    public static void WelcomeReceived() {
        using (Packet packet = new Packet((int) ClientPackets.welcomeReceived)) { // 패킷 생성 시 가장 앞 부분에 패킷id(종류) 삽입
            packet.Write(Client.instance.myId);
            packet.Write(UIManager.instance.m_UsernameField.text);

            SendTCPData(packet);
        }
    }

    public static void RequestServerTime() {
        using (Packet packet = new Packet((int) ClientPackets.requestServerTime)) { // 패킷 생성 시 가장 앞 부분에 패킷id(종류) 삽입
            long clientTime = TimeSync.GetLocalUnixTime();
            packet.Write(clientTime);
            SendTCPData(packet);
        }
    }

    public static void SpawnPlayerReceived(int id) {
        using (Packet packet = new Packet((int) ClientPackets.spawnPlayerReceived)) { // 패킷 생성 시 가장 앞 부분에 패킷id(종류) 삽입
            packet.Write(id);

            SendTCPData(packet);
        }
    }

    public static void PlayerInput(long timestamp, bool[] inputs) { // 움직임을 제외한 나머지 키 입력에 대한 패킷 (스킬 등)
        using (Packet packet = new Packet((int) ClientPackets.playerInput)) {
            packet.Write(timestamp);
            packet.Write(inputs.Length);
            foreach (bool input in inputs) {
                packet.Write(input);
            }

            SendTCPData(packet);
        }
    }

    public static void PlayerMovement(ClientInput clientInput, Vector3 position) { // 움직임에 관련된 키 입력에 대한 패킷
        using (Packet packet = new Packet((int) ClientPackets.playerMovement)) {
            packet.Write(clientInput.timestamp);
            packet.Write(clientInput.horizontal_raw);
            packet.Write(clientInput.vertical_raw);
            packet.Write(clientInput.cam_forward);
            packet.Write(clientInput.deltaPos);
            packet.Write(position);
            packet.Write(clientInput.cam_forward);

            SendTCPData(packet);
        }
    }

    public static void PlayerAttack() {
        using (Packet packet = new Packet((int) ClientPackets.playerAttack)) {
            SendTCPData(packet);
        }
    }

    public static void ChangeHp(int hitPoints, int targetPlayer) {
        using (Packet packet = new Packet((int) ClientPackets.changeHp)) {
            packet.Write(hitPoints);
            packet.Write(targetPlayer);

            SendTCPData(packet);
        }
    }
    #endregion
}