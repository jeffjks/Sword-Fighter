using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    //public GameObject localPlayerPrefab;
    //public GameObject playerPrefab;
    public UIManager m_UIManager;
    public ObjectPooling m_ObjectPooling;
    public int m_PingMin, m_PingMax;

    [HideInInspector]
    public PlayerController m_PlayerController;

    private void Awake() // Singleton
    {
        Application.targetFrameRate = 30;
        
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists. Destroying object!");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int id, string username, Vector3 position, Vector3 direction, int hp, int state) {
        PlayerManager playerManager;
        Quaternion rot = Quaternion.LookRotation(direction);

        if (id == Client.instance.myId) {
            m_PlayerController = m_ObjectPooling.GetPlayerController();
            playerManager = m_PlayerController.m_PlayerMe;
        }
        else {
            playerManager = m_ObjectPooling.GetOppositePlayer();
        }

        playerManager.id = id;
        playerManager.m_CurrentHp = hp;
        playerManager.m_State = state;
        playerManager.realPosition = position;
        playerManager.transform.position = position;
        playerManager.SetUserName(username);
        playerManager.Init();
        players.Add(id, playerManager);
    }

    public string GetUserNameWithId(int id) {
        string userName;
        try {
            userName = GameManager.players[id].GetUserName();
        }
        catch (KeyNotFoundException) {
            userName = "(Unknown)";
        }
        return userName;
    }

    public void Reset() {
        foreach (KeyValuePair<int, PlayerManager> playerManager in GameManager.players) {
            if (playerManager.Value.id == Client.instance.myId) {
                playerManager.Value.gameObject.SetActive(false);
                continue;
            }
            else {
                GameManager.instance.m_ObjectPooling.ReturnOppositePlayer(playerManager.Value);
            }
        }
        GameManager.players.Clear();
    }
}
