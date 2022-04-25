using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet) {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet);
    }

    #region Packets
    public static void WelcomeReceived() {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived)) {
            packet.Write(Client.instance.myId);
            packet.Write(UIManager.instance.m_UsernameField.text);

            SendTCPData(packet);
        }
    }

    public static void PlayerInput(bool[] inputs) {
        using (Packet packet = new Packet((int) ClientPackets.playerInput)) {
            packet.Write(inputs.Length);
            foreach (bool input in inputs) {
                packet.Write(input);
            }

            SendTCPData(packet);
        }
    }

    public static void PlayerMovement(Vector2 movement, ClientInput clientInput) {
        using (Packet packet = new Packet((int) ClientPackets.playerMovement)) {
            packet.Write(movement);
            packet.Write(clientInput.seqNum);
            packet.Write(clientInput.horizontal_raw);
            packet.Write(clientInput.vertical_raw);
            packet.Write(clientInput.cam_forward);
            //packet.Write(GameManager.players[Client.instance.myId].transform.position);
            packet.Write(GameManager.players[Client.instance.myId].m_CharacterModel.forward);

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