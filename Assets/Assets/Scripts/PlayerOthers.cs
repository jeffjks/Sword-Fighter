using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NextMovement
{
    public Vector3 nextTarget;
    public Vector3 direction;
    public Vector3 deltaPos;

    public NextMovement(Vector3 nextTarget, Vector3 direction, Vector3 deltaPos) {
        this.nextTarget = nextTarget;
        this.direction = direction;
        this.deltaPos = deltaPos;
    }
}

public class PlayerOthers : PlayerManager
{
    private readonly Queue<NextMovement> q_nextMovement = new Queue<NextMovement>();
    private Vector3 nextTargetPosition;
    private const float DELAY = 0.2f;

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

    public override void OnStateReceived(float timestamp, Vector3 position, Vector3 direction, Vector3 deltaPos) {
        StartCoroutine(OnStateReceivedDelay(position, direction, deltaPos));
    }

    private IEnumerator OnStateReceivedDelay(Vector3 position, Vector3 direction, Vector3 deltaPos) { // 받은 정보를 다음 예약 지점으로 설정
        q_nextMovement.Enqueue(new NextMovement(position, direction, deltaPos));

        /*
        if (this.deltaPos != Vector3.zero) {
            q_nextMovement.Enqueue(new NextMovement(position, deltaPos));
        }
        else {
            yield return new WaitForSeconds(randomNum);
            m_Movement = movement;
            this.direction = direction;
            this.deltaPos = deltaPos;
        }*/
        yield break;
    }

    private bool CheckFront(Vector3 direction, Vector3 target_pos) {
        float dot = Vector3.Dot(direction, target_pos - realPosition);
        return dot > 0;
    }

    void FixedUpdate()
    {
        ProcessMovement();
    }

    private void ProcessMovement() { // deltaPos에 기반한 이동
        if (q_nextMovement.Count > 0) {
            Vector3 r_deltaPos = q_nextMovement.Peek().deltaPos;
            float sec = 0f;

            if (r_deltaPos == Vector3.zero) { // 멈추는 명령이면 즉시 적용
                sec = 0f;
            }
            else { // 이외의 명령이면 딜레이 적용
                sec = DELAY;
            }
            StartCoroutine(SetMovement(q_nextMovement.Peek(), sec));
            q_nextMovement.Dequeue();
        }

        realPosition += deltaPos;
    }

    private IEnumerator SetMovement(NextMovement nextMovement, float sec) { // 좌표 받으면 앞에 있을 경우 다음 예약 지점으로 설정
        int time = (int) (sec*1000f);
        yield return new WaitForSeconds(sec);
        m_Movement = nextMovement.deltaPos;
        this.realPosition = nextMovement.nextTarget;
        this.direction = nextMovement.direction;
        this.deltaPos = nextMovement.deltaPos;
        yield break;
    }
}
