using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public GameObject oppositePlayerPrefab;
    public MainCharacter m_MainCharacter;

    private Queue<PlayerManager> poolingObjectQueue = new Queue<PlayerManager>();
    private List<PlayerManager> allPoolingObjectList = new List<PlayerManager>();
    private const int defaultObjectCount = 3;

    public void Init(int count) {
        int totalCount = count - poolingObjectQueue.Count;
        for(int i = 0; i < totalCount; i++) {
            PlayerManager playerManager;
            poolingObjectQueue.Enqueue(playerManager = CreateNewObject());
            playerManager.gameObject.SetActive(false);
        }
    }

    private PlayerManager CreateNewObject() {
        PlayerManager playerManager = Instantiate(oppositePlayerPrefab).GetComponent<PlayerManager>();
        playerManager.transform.SetParent(transform);
        return playerManager;
    }

    public PlayerManager GetOppositePlayer() {
        if (poolingObjectQueue.Count > 0) {
            PlayerManager obj = poolingObjectQueue.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else {
            PlayerManager newObj = CreateNewObject();
            //newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public MainCharacter GetLocalPlayer() {
        m_MainCharacter.gameObject.SetActive(true);
        return m_MainCharacter;
    }

    public void ReturnOppositePlayer(PlayerManager obj) {
        if (!obj.gameObject.activeSelf) {
            return;
        }
        poolingObjectQueue.Enqueue(obj);
        obj.gameObject.SetActive(false);
        //obj.transform.SetParent(transform);
    }
}