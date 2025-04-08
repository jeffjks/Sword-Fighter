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

        ProcessMovement();
        SetRotation();
    }

    private void SetRotation() {
        m_CharacterModel.rotation = Quaternion.LookRotation(direction);
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
        
        _prevPosition = realPosition;
        _nextPosition = position + deltaPos * (delay / 1000f);
        _deltaPos = _nextPosition - position;
        _nextDeltaPos = deltaPos;
        _hasTarget = true;
        direction = clientInput.forwardDirection;
        m_Movement = clientInput.movementRaw;

        //var playerMovement = new PlayerMovement(timestamp, position, deltaPos);
        //_playerMovementQueue.Enqueue(playerMovement);
    }

    private bool CheckFront(Vector3 direction, Vector3 target_pos) {
        float dot = Vector3.Dot(direction, target_pos - realPosition);
        return dot > 0;
    }

    private void ProcessMovement()
    {
        if (!_hasTarget)
        {
            realPosition += _deltaPos;
            return;
        }

        _moveTimer += Time.deltaTime;
        float t = _moveTimer / (UpdateInterval / 1000f);

        if (t >= 1f)
        {
            t = 1f;
            _hasTarget = false;
            _deltaPos = _nextDeltaPos;
        }

        realPosition = Vector3.Lerp(_prevPosition, _nextPosition, t);
    }
}
