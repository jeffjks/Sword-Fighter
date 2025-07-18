using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Shared.Enums;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new ();

    //public GameObject localPlayerPrefab;
    //public GameObject playerPrefab;
    public UIManager m_UIManager;
    public ObjectPooling m_ObjectPooling;
    [Space(10)]
    public bool m_DebugTestClients;

    [SerializeField] private int _pingMin, _pingMax;

    public static bool IsDebugPing;

    [HideInInspector]
    public PlayerController m_PlayerController;

    public static string dirSend = $"{Application.dataPath}/Debug";
    public static string dirReceived = $"{Application.dataPath}/Debug";

    private void Awake() // Singleton
    {
        Application.targetFrameRate = 30;

        if (!Directory.Exists(dirSend))
            Directory.CreateDirectory(dirSend);
        if (!Directory.Exists(dirReceived))
            Directory.CreateDirectory(dirReceived);
        File.WriteAllText($"{dirSend}/send.txt", string.Empty); // DEBUG
        File.WriteAllText($"{dirReceived}/received.txt", string.Empty); // DEBUG
        
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists. Destroying object!");
            Destroy(this);
        }

        IsDebugPing = _pingMin != 0 || _pingMax != 0;
    }

    public int GetDebugPing()
    {
        return Random.Range(_pingMin, _pingMax);
    }

    public void SpawnPlayer(int id, string username, Vector3 position, Vector3 direction, int currentHp, int maxHp, int state) {
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
        playerManager.m_CharacterStatus.CurrentHitPoint = currentHp;
        playerManager.m_CharacterStatus.MaxHitPoint = maxHp;
        playerManager.CurrentStateMachine.SetState((PlayerState) state);
        playerManager.m_RealPosition = position;
        playerManager.transform.position = position;
        playerManager.SetUserName(username);
        playerManager.Init();
        players.Add(id, playerManager);
    }

    public string GetUserNameWithId(int id) {
        string userName;
        try {
            userName = players[id].GetUserName();
        }
        catch (KeyNotFoundException) {
            userName = "(Unknown)";
        }
        return userName;
    }

    public void Reset() {
        foreach (KeyValuePair<int, PlayerManager> playerManager in players) {
            if (playerManager.Value.id == Client.instance.myId) {
                playerManager.Value.gameObject.SetActive(false);
                continue;
            }
            else {
                m_ObjectPooling.ReturnOppositePlayer(playerManager.Value);
            }
        }
        players.Clear();
    }
}
