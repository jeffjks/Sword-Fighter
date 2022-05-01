using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet) { // msg 후 toClient 읽기
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        Client.instance.myId = myId;

        ClientSend.WelcomeReceived();
    }

    public static void SpawnPlayer(Packet packet) {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Vector3 direction = packet.ReadVector3();
        int hp = packet.ReadInt();
        int state = packet.ReadInt();
        //Quaternion rotation = packet.ReadQuaternion();
        
        GameManager.instance.SpawnPlayer(id, username, position, direction, hp, state);
    }

    public static void PlayerMovement(Packet packet) {
        int id = packet.ReadInt();

        Vector2 movement = packet.ReadVector2();
        int seqNum = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        Vector3 direction = packet.ReadVector3();
        //Quaternion rotation = packet.ReadQuaternion();

        //Debug.Log(id);
        if (Client.instance.myId == id) {
            GameManager.instance.m_MainCharacter.OnStateReceived(seqNum, position);
        }
        else { // 다른 플레이어
            if (GameManager.players.ContainsKey(id)) {
                GameManager.players[id].m_Movement = movement;
                GameManager.players[id].transform.position = position;
                GameManager.players[id].direction = direction;
            }
        }
    }

    public static void PlayerState(Packet packet) {
        int id = packet.ReadInt();

        int state = packet.ReadInt();
        
        if (GameManager.players.ContainsKey(id)) {
            GameManager.players[id].m_State = state;
        }
    }

    public static void PlayerHp(Packet packet) {
        int id = packet.ReadInt();

        int hitPoints = packet.ReadInt();
        
        if (GameManager.players.ContainsKey(id)) {
            GameManager.players[id].m_CurrentHp = hitPoints;
        }
    }

    public static void PlayerDisconnected(Packet packet) {
        int id = packet.ReadInt();

        Destroy(GameManager.players[id].gameObject);
        GameManager.players.Remove(id);
    }
}
