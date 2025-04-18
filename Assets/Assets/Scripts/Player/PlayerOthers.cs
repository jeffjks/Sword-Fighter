using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOthers : PlayerManager
{
    private Vector3 _prevPosition, _nextPosition;
    private bool _hasTarget;
    private Vector3 _deltaPos;
    private Vector3 _nextDeltaPos;
    private float _moveTimer;

    private const int MaxPredictionTime = 500;
    private const int UpdateInterval = 100;

    protected override void Update() {
        base.Update();

        _correctedPos = m_RealPosition;
        
        ProcessMovement();
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
        
        _prevPosition = m_RealPosition;
        _nextPosition = position + m_DeltaPos * (delay / 1000f);
        _deltaPos = _nextPosition - position;
        _nextDeltaPos = m_DeltaPos;
        _hasTarget = true;
        _moveTimer = 0f;
        
        if (CurrentState == PlayerState.Idle || CurrentState == PlayerState.Move)
            CurrentState = (deltaPos == Vector3.zero) ? PlayerState.Idle : PlayerState.Move;

        SetMovementAnimation(inputVector);

        SetRotation(facingDirection);

        //var playerMovement = new PlayerMovement(timestamp, position, deltaPos);
        //_playerMovementQueue.Enqueue(playerMovement);
    }

    public override void OnStateReceived(long timestamp, PlayerSkill playerSkill, Vector3 facingDirection)
    {
        ExecutePlayerSkill(timestamp, playerSkill, facingDirection);
    }

    private void ProcessMovement()
    {
        if (!_hasTarget)
        {
            m_RealPosition += _deltaPos;
            return;
        }

        _moveTimer += Time.deltaTime;
        float t = _moveTimer / (UpdateInterval / 1000f);

        if (t >= 1f)
        {
            t = 1f;
            _hasTarget = false;
            _deltaPos = _nextDeltaPos;
            _moveTimer = 0f;
        }

        m_RealPosition = Vector3.Lerp(_prevPosition, _nextPosition, t);
    }
}
