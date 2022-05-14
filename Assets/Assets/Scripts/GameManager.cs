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
        GameObject player;
        Quaternion rot = Quaternion.LookRotation(direction);

        if (id == Client.instance.myId) {
            player = Instantiate(localPlayerPrefab, position, rot);
            m_MainCharacter = player.GetComponent<MainCharacter>();
        }
        else {
            //Client.instance.oppositeId = id;
            player = Instantiate(playerPrefab, position, rot);
        }

        player.GetComponent<PlayerManager>().id = id;
        player.GetComponent<PlayerManager>().username = username;
        player.GetComponent<PlayerManager>().m_CurrentHp = hp;
        player.GetComponent<PlayerManager>().m_State = state;
        players.Add(id, player.GetComponent<PlayerManager>());
    }
}
