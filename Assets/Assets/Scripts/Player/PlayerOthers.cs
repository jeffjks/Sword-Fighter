using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerMovement
{
    public long timestamp;
    public Vector3 position;
    public Vector3 deltaPos;

    public PlayerMovement(long timestamp, Vector3 position, Vector3 direction, Vector3 deltaPos) {
        this.timestamp = timestamp;
        this.position = position;
        this.deltaPos = deltaPos;
    }
}

public class PlayerOthers : PlayerManager
{
    private Vector3 _prevPosition, _nextPosition;
    private bool _hasTarget;
    private Vector3 _deltaPos;
    private Vector3 _nextDeltaPos;
    private float _moveTimer;

    private const int MaxPredictionTime = 500;
    private const int UpdateInterval = 200;

    protected override void Update() {
        base.Update();

        correctedPos = m_RealPosition;
        
        ProcessMovement();
    }

    public override void Start_DealDamage_Attack1() {
        return;
    }

    public override void Finish_DealDamage_Attack1() {
        return;
    }

    public override void OnStateReceived(Vector3 position, ClientInput clientInput) {
        long now = TimeSync.GetSyncTime();
        int delay = Mathf.Clamp((int) (now - clientInput.timestamp), 0, MaxPredictionTime); // 예측 제한 500ms
        
        _prevPosition = m_RealPosition;
        _nextPosition = position + m_DeltaPos * (delay / 1000f);
        _deltaPos = _nextPosition - position;
        _nextDeltaPos = m_DeltaPos;
        _hasTarget = true;
        m_Movement = clientInput.inputVector;
        _moveTimer = 0f;

        SetRotation(clientInput.forwardDirection);

        //var playerMovement = new PlayerMovement(timestamp, position, deltaPos);
        //_playerMovementQueue.Enqueue(playerMovement);
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
