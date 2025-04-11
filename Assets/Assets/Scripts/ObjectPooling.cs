using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public GameObject oppositePlayerPrefab;
    public PlayerController m_PlayerController;

    private Queue<PlayerManager> _poolingObjectQueue = new Queue<PlayerManager>();
    private List<PlayerManager> _allPoolingObjectList = new List<PlayerManager>();
    private const int DefaultObjectCount = 3;

    public void Init(int count) {
        int totalCount = count - _poolingObjectQueue.Count;
        for(int i = 0; i < totalCount; i++) {
            PlayerManager playerManager;
            _poolingObjectQueue.Enqueue(playerManager = CreateNewObject());
            playerManager.gameObject.SetActive(false);
        }
    }

    private PlayerManager CreateNewObject() {
        PlayerManager playerManager = Instantiate(oppositePlayerPrefab).GetComponent<PlayerManager>();
        playerManager.transform.SetParent(transform);
        return playerManager;
    }

    public PlayerManager GetOppositePlayer() {
        if (_poolingObjectQueue.Count > 0) {
            PlayerManager obj = _poolingObjectQueue.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else {
            PlayerManager newObj = CreateNewObject();
            //newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public PlayerController GetPlayerController() {
        m_PlayerController.gameObject.SetActive(true);
        return m_PlayerController;
    }

    public void ReturnOppositePlayer(PlayerManager obj) {
        if (!obj.gameObject.activeSelf) {
            return;
        }
        _poolingObjectQueue.Enqueue(obj);
        obj.gameObject.SetActive(false);
        //obj.transform.SetParent(transform);
    }
}