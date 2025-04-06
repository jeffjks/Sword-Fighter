using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerMovement
{
    public long timestamp;
    public Vector3 position;
    public Vector3 direction;
    public Vector3 deltaPos;

    public PlayerMovement(long timestamp, Vector3 position, Vector3 direction, Vector3 deltaPos) {
        this.timestamp = timestamp;
        this.position = position;
        this.direction = direction;
        this.deltaPos = deltaPos;
    }
}

public class PlayerOthers : PlayerManager
{
    private readonly Queue<PlayerMovement> _playerMovementQueue = new ();
    private Vector3 nextTargetPosition;
    private const float MaxPredictionTime = 0.5f;

    protected override void Update() {
        base.Update();
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

    public override void OnStateReceived(long timestamp, Vector3 position, Vector3 direction, Vector3 deltaPos) {
        var playerMovement = new PlayerMovement(timestamp, position, direction, deltaPos);
        StartCoroutine(OnStateReceivedDelay(playerMovement));
    }

    private IEnumerator OnStateReceivedDelay(PlayerMovement playerMovement) {
        int ping = Random.Range(GameManager.instance.m_PingMin, GameManager.instance.m_PingMax) / 2; // 핑 테스트용
        if (ping > 0)
            yield return new WaitForSeconds(ping / 1000f);

        _playerMovementQueue.Enqueue(playerMovement);
    }

    private bool CheckFront(Vector3 direction, Vector3 target_pos) {
        float dot = Vector3.Dot(direction, target_pos - realPosition);
        return dot > 0;
    }

    void FixedUpdate()
    {
        ProcessMovement();
    }

    private void ProcessMovement()
    {
        if (_playerMovementQueue.Count > 0)
        {
            var playerMovement = _playerMovementQueue.Dequeue();

            long clientTime = TimeSync.GetSyncTime();
            
            direction = playerMovement.direction;
            deltaPos = playerMovement.deltaPos;

            float delaySec = Mathf.Clamp((clientTime - playerMovement.timestamp) / 1000f, 0f, MaxPredictionTime);
            realPosition = playerMovement.position + deltaPos * delaySec;
        }

        realPosition += deltaPos;
        m_Movement = deltaPos;
    }
}
