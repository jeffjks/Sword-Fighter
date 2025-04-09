using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerMe : PlayerManager
{
    public readonly Queue<ClientInput> _clientInputQueue = new Queue<ClientInput>();

    void Awake() {
        m_UI_HpBar = GameManager.instance.m_UIManager.m_UI_HpBarMain;
    }

    public override void Start_DealDamage_Attack1() {
        //m_Sword.StartDeal();
        ClientSend.PlayerAttack();
    }

    public override void Finish_DealDamage_Attack1() {
        //m_Sword.FinishDeal();
    }

    public override void OnStateReceived(Vector3 position, ClientInput clientInput)
    {
        while (_clientInputQueue.Count > 0 && _clientInputQueue.Peek().timestamp < clientInput.timestamp) { // 처리된 요청은 삭제
            _clientInputQueue.Dequeue();
        }

        Vector3 newState = position; // 서버로부터 받은 가장 최신 좌표
        //Debug.Log($"Received: {clientInput.timestamp}, {position}, {deltaPos}");
        
        foreach (var input in _clientInputQueue) { // 지금까지 input기록에 따라 시뮬레이션하여 현재 좌표 계산
            newState += input.deltaPos;
        }

        var distance = Vector3.Distance(newState, realPosition);
        
        if (distance > 0f) { // 계산한 좌표가 맞는지 확인
            //Debug.Log($"Wrong ({distance}): {realPosition} -> {newState}");
            realPosition = newState;
        }
    }
}
