using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class PlayerMe : PlayerManager
{
    public readonly Queue<ClientInput> m_ClientInputQueue = new ();
    
    private Vector3 _lastPositionFromServer;
    private long _lastTimestamp;

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
    
    private void CorrectPosition(int seqNum, long timestamp)
    {
        while (m_ClientInputQueue.Count > 0 && m_ClientInputQueue.Peek().timestamp <= timestamp) { // 처리된 요청은 삭제
            m_ClientInputQueue.Dequeue();
        }

        Vector3 correctedPos = _lastPositionFromServer; // 서버로부터 받은 가장 최신 좌표
        
        var tempStr = string.Empty;
        foreach (var input in m_ClientInputQueue) { // 지금까지 input기록에 따라 시뮬레이션하여 현재 좌표 계산
            correctedPos += input.deltaPos;
            tempStr += $"{input.timestamp}, {input.deltaPos}\n";
        }
        
        var distance = Vector3.Distance(correctedPos, m_RealPosition);

        if (distance > PositionCorrectionThreshold) { // 계산한 좌표가 맞는지 확인
            Debug.Log($"[{seqNum}, {timestamp}] Wrong ({_lastPositionFromServer}): {m_RealPosition} -> {correctedPos}\n{tempStr}");
            m_RealPosition = correctedPos;
        }
    }

    public override void OnStateReceived(int seqNum, long timestamp, Vector3 facingDirection, Vector3 deltaPos, Vector2 inputVector, Vector3 position)
    {
        if (seqNum < _lastSeqNum)
            return;
        
        _lastPositionFromServer = position;
        _lastTimestamp = timestamp;
        _lastSeqNum = seqNum;
        
        CorrectPosition(seqNum, timestamp);

#if UNITY_EDITOR
        using (StreamWriter writer = new ($"{GameManager.dirReceived}/received.txt", append: true))
        {
            var queueString = string.Join(", ", m_ClientInputQueue.Select(i => $"[{i.timestamp}]"));
            writer.WriteLine($"[{seqNum}, {timestamp}] ClientReceived: {position}), [{TimeSync.GetSyncTime()}] {m_RealPosition}");
            //writer.WriteLine($"\t{queueString}");
        }
#endif
    }

    public override void OnStateReceived(long timestamp, PlayerSkill playerSkill, Vector3 facingDirection, Vector3 targetPosition)
    {
        throw new System.NotImplementedException();
    }
}
