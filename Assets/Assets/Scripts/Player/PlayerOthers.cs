using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared.Enums;

public class PlayerOthers : PlayerManager
{
    private Vector3 _prevPosition, _nextPosition;
    private bool _hasTarget;
    private Vector3 _nextDeltaPos;
    private float _moveTimer;

    private const int MaxPredictionTime = 500;
    private const int UpdateInterval = 200;

    protected override void Update() {
        base.Update();
        
        DeadReckoning();
    }

    public override void Start_DealDamage_Attack1() {
        return;
    }

    public override void Finish_DealDamage_Attack1() {
        return;
    }

    public override void OnStateReceived(int seqNum, long timestamp, Vector3 facingDirection, Vector3 deltaPos, Vector2 inputVector, Vector3 position)
    {
        if (seqNum >= 0)
        {
            if (seqNum < _lastSeqNum)
                return;
            
            _lastSeqNum = seqNum;
        }

        long now = TimeSync.GetSyncTime();
        int delay = Mathf.Clamp((int) (now - timestamp), 0, MaxPredictionTime); // 예측 제한 500ms
        
        _prevPosition = m_RealPosition; // 현재 위치
        _nextPosition = position + deltaPos * (delay / 1000f); // 다음 타겟 위치
        _nextDeltaPos = deltaPos; // 타겟 도착 후 이동 방향
        _hasTarget = true;
        _moveTimer = 0f;
        m_DeltaPos = _nextPosition - position;
        
        if (IsCurrentState(PlayerState.Idle) || IsCurrentState(PlayerState.Move))
        {
            CurrentStateMachine.SetState((deltaPos == Vector3.zero) ? PlayerState.Idle : PlayerState.Move);
        }

        SetMovementAnimation(inputVector);

        SetRotation(facingDirection);

        //var playerMovement = new PlayerMovement(timestamp, position, deltaPos);
        //_playerMovementQueue.Enqueue(playerMovement);
    }

    public override void OnStateReceived(long timestamp, PlayerSkill playerSkill, Vector3 facingDirection, Vector3 targetPosition)
    {
        ExecutePlayerSkill(timestamp, playerSkill, facingDirection, targetPosition);
    }

    private void DeadReckoning()
    {
        if (!_hasTarget) // 목표 지점이 없으면 deltaPos 방향으로 이동
        {
            m_RealPosition += m_DeltaPos;
            return;
        }

        _moveTimer += Time.deltaTime;
        float t = _moveTimer / (UpdateInterval / 1000f);

        if (t >= 1f) // 목표 지점 도착
        {
            t = 1f;
            _hasTarget = false;
            _moveTimer = 0f;
            m_DeltaPos = _nextDeltaPos;
        }

        // UpdateInterval millisecond 동안 목표 지점으로 이동
        m_RealPosition = Vector3.Lerp(_prevPosition, _nextPosition, t);
    }
}
