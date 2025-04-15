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
        ClientSend.RequestServerTime();
    }

    public static void RequestServerTime(Packet packet) {
        long serverTime = packet.ReadLong();
        long clientTime = packet.ReadLong();
        
        TimeSync.OnServerTimeResponse(serverTime, clientTime);
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

    public static void UpdatePlayer(Packet packet) {
        int id = packet.ReadInt();

        var timestamp = packet.ReadLong();
        Vector3 position = packet.ReadVector3();
        Vector3 direction = packet.ReadVector3();
        Vector3 deltaPos = packet.ReadVector3();
        var clientInput = new ClientInput(timestamp, Vector2Int.zero, direction, deltaPos);

        if (Client.instance.myId == id) {
            GameManager.players[id].OnStateReceived(position, clientInput);
        }
        else { // 다른 플레이어
            Debug.LogError("Received UpdatePlayer Packet with Controller's PlayerID");
        }
    }

    public static void BroadcastPlayer(Packet packet) {
        int id = packet.ReadInt();

        var timestamp = packet.ReadLong();
        Vector3 position = packet.ReadVector3();
        Vector3 direction = packet.ReadVector3();
        Vector3 deltaPos = packet.ReadVector3();
        Vector2Int movementRaw = Vector2Int.FloorToInt(packet.ReadVector2());
        var clientInput = new ClientInput(timestamp, movementRaw, direction, deltaPos);

        if (Client.instance.myId == id) { // 자신 플레이어
            Debug.LogError("Received UpdatePlayer Packet with Other's PlayerID");
        }
        else { // 다른 플레이어
            GameManager.players[id].OnStateReceived(position, clientInput);
        }
    }

    public static void PlayerState(Packet packet) {
        int id = packet.ReadInt();

        int state = packet.ReadInt();
        
        try {
            GameManager.players[id].CurrentState = (PlayerState) state;
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
