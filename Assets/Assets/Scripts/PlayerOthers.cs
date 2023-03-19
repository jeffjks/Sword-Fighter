using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NextMovement
{
    public Vector3 nextTarget;
    public Vector3 deltaPos;
    public Vector2 movement;

    public NextMovement(Vector3 nextTarget, Vector3 deltaPos, Vector2 movement) {
        this.nextTarget = nextTarget;
        this.deltaPos = deltaPos;
        this.movement = movement;
    }
}

public class PlayerOthers : PlayerManager
{
    private readonly Queue<NextMovement> q_nextMovement = new Queue<NextMovement>();
    private Vector3 nextTargetPosition;
    private bool hasTarget = false;
    private const float SPEED = 4.8f;
    private int seqNum;

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

    /*
    movement, position, direction, deltaPos가 담긴 패킷을 받으면 바로 큐에 push
    큐에 push된 움직임 정보는 다음과 같은 규칙으로 적용
    현재 멈춰있을 경우 : N초 뒤 큐 적용
    움직이고 있을 경우 : 목표물 target ON 하고 해당 목표물로 MoveTowards, 도달 시 큐 적용
    멈추는 패킷일 경우 : 목표물 target ON 하고 해당 목표물로 MoveTowards, 도달 시 큐 적용하고 target OFF
    */

    private IEnumerator OnStateReceivedDelay(Vector2 movement, Vector3 position, Vector3 direction, Vector3 deltaPos) { // 좌표 받으면 앞에 있을 경우 다음 예약 지점으로 설정
        //float randomNum = 1f;
        //realPosition = position;
        q_nextMovement.Enqueue(new NextMovement(position, deltaPos, movement));

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

    void FixedUpdate() // Camera
    {
        ProcessMovement();

        if (deltaPos == Vector3.zero) {
            //realPosition = Vector3.MoveTowards(realPosition, nextTargetPosition, SPEED * Time.fixedDeltaTime);
        }
    }

    private void ProcessMovement() { // deltaPos에 기반한 이동
        if (q_nextMovement.Count > 0) {
            Vector3 r_nextTargetPosition = q_nextMovement.Peek().nextTarget;
            Vector3 r_deltaPos = q_nextMovement.Peek().deltaPos;
            Vector3 r_movement = q_nextMovement.Peek().movement;
            float sec = 0f;

            if (r_deltaPos == Vector3.zero) { // 멈추는 명령이면
                sec = 0f;
                Debug.Log("0f");
                //hasTarget = false;
                //nextTargetPosition = r_nextTargetPosition;
                //deltaPos = r_deltaPos;
            }
            else {
                //hasTarget = true;
                //nextTargetPosition = r_nextTargetPosition;
                sec = 1f;
                Debug.Log("1f");
                /*
                if (Vector2.Dot(r_nextTargetPosition - realPosition, deltaPos) > 0){
                    sec = Vector3.Distance(realPosition, r_nextTargetPosition) / (SPEED * Time.fixedDeltaTime);
                    Debug.Log(Vector3.Distance(realPosition, r_nextTargetPosition) + ", " + (SPEED * Time.fixedDeltaTime));
                }*/
            }
            StartCoroutine(SetMovement(q_nextMovement.Peek(), sec));
            q_nextMovement.Dequeue();
        }

        realPosition += deltaPos;
    }

    private IEnumerator SetMovement(NextMovement nextMovement, float sec) { // 좌표 받으면 앞에 있을 경우 다음 예약 지점으로 설정
        int time = (int) (sec*1000f);
        //Debug.Log($"Next movement (after {time} ms): {nextMovement.nextTarget}, {nextMovement.deltaPos}");
        yield return new WaitForSeconds(sec);
        this.realPosition = nextMovement.nextTarget;
        this.deltaPos = nextMovement.deltaPos;
        m_Movement = nextMovement.movement;
        //direction = nextMovement.deltaPos;
        yield break;
    }
}
