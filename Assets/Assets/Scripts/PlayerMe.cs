using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMe : PlayerManager
{
    public readonly Queue<ClientInput> q_inputTimeline = new Queue<ClientInput>();

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

    public override void OnStateReceived(int seqNum, Vector2 movement, Vector3 position, Vector3 direction, Vector3 deltaPos) { // *** 추가 작업 필요 (멈췄을때만 위치 보정?)
        if (position != Vector3.zero) {
            return;
        }
        while (q_inputTimeline.Count > 0 && q_inputTimeline.Peek().seqNum <= seqNum) { // 처리된 요청은 삭제
            q_inputTimeline.Dequeue();
        }
        int seqNumTemp = q_inputTimeline.Peek().seqNum;

        Vector3 newState = position; // 서버로부터 받은 가장 최신 좌표

        foreach (var input in q_inputTimeline) { // 지금까지 input기록에 따라 시뮬레이션하여 현재 좌표 계산
            newState += input.deltaPos;
            //newState = ProcessMovement(newState, input);
        }

        //Debug.Log(seqNum + " : " + receivedState);
        //Debug.Log(seqNumTemp + ", " + newState);
        
        if (Vector3.Distance(newState, realPosition) > 0f) { // 계산한 좌표가 맞는지 확인
            realPosition = newState;
        }
    }
}
