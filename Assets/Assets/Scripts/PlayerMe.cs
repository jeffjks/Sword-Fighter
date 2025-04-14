using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class PlayerMe : PlayerManager
{
    public readonly Queue<ClientInput> m_ClientInputQueue = new ();

    private ClientInput _lastClientInput;
    private Vector3 _lastPositionFromServer;

    private const float PositionCorrectionThreshold = 0.25f;

    private void Awake() {
        m_UI_HpBar = GameManager.instance.m_UIManager.m_UI_HpBarMain;
    }

    public override void Start_DealDamage_Attack1() {
        //m_Sword.StartDeal();
        //ClientSend.PlayerAttack();
    }

    public override void Finish_DealDamage_Attack1() {
        //m_Sword.FinishDeal();
    }
    
    private void CorrectPosition()
    {
        while (m_ClientInputQueue.Count > 0 && m_ClientInputQueue.Peek().timestamp <= _lastClientInput.timestamp) { // 처리된 요청은 삭제
            m_ClientInputQueue.Dequeue();
        }

        Vector3 newPos = _lastPositionFromServer; // 서버로부터 받은 가장 최신 좌표
        
        foreach (var input in m_ClientInputQueue) { // 지금까지 input기록에 따라 시뮬레이션하여 현재 좌표 계산
            newPos += input.deltaPos;
        }

        correctedPos = newPos;
        
        var distance = Vector3.Distance(correctedPos, realPosition);

        if (distance > PositionCorrectionThreshold) { // 계산한 좌표가 맞는지 확인
            Debug.Log($"Wrong ({distance}): {realPosition} -> {correctedPos}");
            realPosition = correctedPos;
        }
    }

    public override void OnStateReceived(Vector3 positionFromServer, ClientInput clientInput)
    {
        if (clientInput.timestamp < _lastClientInput.timestamp)
            return;
        
        _lastPositionFromServer = positionFromServer;
        _lastClientInput = clientInput;
        
        CorrectPosition();

#if UNITY_EDITOR
        using (StreamWriter writer = new ("Assets/Resources/received.txt", append: true))
        {
            var queueString = string.Join(", ", m_ClientInputQueue.Select(i => $"[{i.timestamp}] {i.deltaPos}"));
            writer.WriteLine($"[{clientInput.timestamp}] ClientReceived: {clientInput.deltaPos}, {positionFromServer}, ({m_ClientInputQueue.Count}), [{TimeSync.GetSyncTime()}] {realPosition}");
            //writer.WriteLine($"\t{queueString}");
        }
#endif
    }
}
