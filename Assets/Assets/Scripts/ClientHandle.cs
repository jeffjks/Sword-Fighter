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

        ClientSend.SpawnPlayerReceived(id);
    }

    public static void PlayerMovement(Packet packet) {
        int id = packet.ReadInt();

        Vector2 movement = packet.ReadVector2();
        int seqNum = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        Vector3 direction = packet.ReadVector3();
        //Quaternion rotation = packet.ReadQuaternion();
        Vector3 deltaPos = packet.ReadVector3();

        //Debug.Log(id);
        if (Client.instance.myId == id) { // 자신 플레이어
            GameManager.instance.m_PlayerController.OnStateReceived(seqNum, position);
            //Debug.Log(position);
        }
        else { // 다른 플레이어
            try {
                GameManager.players[id].m_Movement = movement;
                GameManager.players[id].realPosition = position;
                GameManager.players[id].direction = direction;
                GameManager.players[id].deltaPos = deltaPos;
            }
            catch (KeyNotFoundException e) {
                Debug.Log(e);
            }
        }
    }

    public static void PlayerState(Packet packet) {
        int id = packet.ReadInt();

        int state = packet.ReadInt();
        
        try {
            GameManager.players[id].m_State = state;
        }
        catch (KeyNotFoundException e) {
            Debug.Log(e);
        }
    }

    public static void PlayerHp(Packet packet) {
        int id = packet.ReadInt();

        int hitPoints = packet.ReadInt();
        
        if (GameManager.players.ContainsKey(id)) {
            GameManager.players[id].SetCurrentHitPoint(hitPoints);
        }
    }

    public static void PlayerDisconnected(Packet packet) {
        int id = packet.ReadInt();

        if (GameManager.players.ContainsKey(id)) {
            //Destroy(GameManager.players[id].gameObject);
            GameManager.instance.m_ObjectPooling.ReturnOppositePlayer(GameManager.players[id]);
            GameManager.players.Remove(id);
            //GameManager.instance.m_UIManager.DestroyUI(id);
        }
    }
}
