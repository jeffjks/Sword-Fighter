using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public UIManager m_UIManager;
    public ObjectPooling m_ObjectPooling;

    [HideInInspector]
    public MainCharacter m_MainCharacter;

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
            //player = Instantiate(localPlayerPrefab, position, rot);
            //m_MainCharacter = player.GetComponent<MainCharacter>();
            m_MainCharacter = m_ObjectPooling.GetLocalPlayer();
            playerManager = m_MainCharacter.m_PlayerManager;
        }
        else {
            //Client.instance.oppositeId = id;
            //player = Instantiate(playerPrefab, position, rot);
            playerManager = m_ObjectPooling.GetOppositePlayer();
        }

        playerManager.id = id;
        playerManager.username = username;
        playerManager.m_CurrentHp = hp;
        playerManager.m_State = state;
        players.Add(id, playerManager);

        //ClientSend.PlayerReady(); // 캐릭터 생성 시 해당 플레이어의 PlayerReady 패킷 전송
    }
}
