using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NextMovement
{
    public Vector3 nextTarget;
    public Vector3 deltaPos;

    public NextMovement(Vector3 nextTarget, Vector3 deltaPos) {
        this.nextTarget = nextTarget;
        this.deltaPos = deltaPos;
    }
}

public class PlayerOthers : PlayerManager
{
    private readonly Queue<NextMovement> q_nextMovement = new Queue<NextMovement>();
    private Vector3 nextTargetPosition;
    private const float SPEED = 4.8f;

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

    public override void OnStateReceived(int seqNum, Vector2 movement, Vector3 position, Vector3 direction, Vector3 deltaPos) {
        StartCoroutine(OnStateReceivedDelay(movement, position, direction, deltaPos));
        /*
        GameManager.players[id].m_Movement = movement;
        GameManager.players[id].realPosition = position;
        GameManager.players[id].direction = direction;
        GameManager.players[id].deltaPos = deltaPos;
        */
    }

    private IEnumerator OnStateReceivedDelay(Vector2 movement, Vector3 position, Vector3 direction, Vector3 deltaPos) { // 좌표 받으면 앞에 있을 경우 다음 예약 지점으로 설정
        float randomNum = 1f;
        //realPosition = position;
        if (this.deltaPos != Vector3.zero) {
            q_nextMovement.Enqueue(new NextMovement(position, deltaPos));
        }
        else {
            yield return new WaitForSeconds(randomNum);
            m_Movement = movement;
            this.direction = direction;
            this.deltaPos = deltaPos;
        }
        yield break;
    }

    private bool CheckFront(Vector3 direction, Vector3 target_pos) {
        float dot = Vector3.Dot(direction, target_pos - realPosition);
        return dot > 0;
    }

    void FixedUpdate() // Camera
    {
        ProcessMovement();

        if (deltaPos == Vector3.zero) {
            realPosition = Vector3.MoveTowards(realPosition, nextTargetPosition, SPEED * Time.fixedDeltaTime);
        }
    }

    private Vector3 ProcessMovement() { // deltaPos에 기반한 이동
        if (q_nextMovement.Count > 0) {
            Vector3 target_pos = q_nextMovement.Peek().nextTarget;
            if (deltaPos != Vector3.zero) {
                if (Vector2.Dot(target_pos - realPosition, direction) > 0) {
                    nextTargetPosition = target_pos;
                }
                else {
                    //Debug.Log($"{target_pos}, {realPosition}, {direction}");
                    realPosition = target_pos;
                }
            }
            deltaPos = Vector3.zero;

            if (Vector3.Distance(realPosition, nextTargetPosition) <= 0f) {
                deltaPos = q_nextMovement.Peek().deltaPos;
                q_nextMovement.Dequeue();
            }
        }
        else {
            realPosition += deltaPos;
        }
        realPosition = ClampPosition(realPosition);
        //transform.position = realPosition;

        if (deltaPos == Vector3.zero) {
            m_Movement = Vector2.zero;
        }
        return deltaPos;
    }
}
