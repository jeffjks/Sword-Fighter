using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerSkill
{
    Dead = -1,
    Idle,
    Move,
    Block,
    Attack1,
    Attack2,
    Roll
}

public abstract class PlayerManager : MonoBehaviour
{
    public Animator m_Animator;
    public int id;
    public Transform m_CharacterModel;
    public Collider m_PlayerCollider;
    public Sword m_Sword;
    public UI_HpBar m_UI_HpBar;
    public int m_CurrentHp;
    public int m_MaxHp;
    public Vector3 deltaPos;

    [HideInInspector] public Vector2Int m_MovementRaw = Vector2Int.zero;
    [HideInInspector] public Vector2 m_Movement = Vector2.zero;
    [HideInInspector] public Vector3 direction = Vector3.forward;
    [HideInInspector] public PlayerSkill m_State = PlayerSkill.Idle;
    [HideInInspector] public Vector3 realPosition;

    protected Vector3 correctedPos;

    private string _username;
    private bool m_CanMove = true;

    public const float ROLL_DISTANCE = 5f;

    public void Init() {
        SetUserNameUI(_username);
        SetCurrentHitPoint(m_CurrentHp);
    }

    public bool IsDead() {
        if (m_State == PlayerSkill.Dead) {
            m_CanMove = false;
            m_PlayerCollider.enabled = false;
            m_Animator.SetInteger("State", (int)m_State);
            m_MovementRaw = Vector2Int.zero;
            m_Movement = Vector2.zero;
            return true;
        }
        return false;
    }

    protected virtual void Update()
    {
        if (IsDead()) {
            return;
        }

        if (m_State == PlayerSkill.Idle || m_State == PlayerSkill.Move) {
            if (m_Movement != Vector2.zero) {
                if (m_CanMove) {
                    m_State = PlayerSkill.Move;
                }
            }
            else {
                m_State = PlayerSkill.Idle;
            }
        }

        m_Animator.SetFloat("MovementHorizontal", m_Movement.x);
        m_Animator.SetFloat("MovementVertical", m_Movement.y);
        m_Animator.SetInteger("State", (int)m_State);

        if (m_State != PlayerSkill.Attack1) {
            Finish_DealDamage_Attack1();
        }
        
        InterpolatePosition();

        //Debug.Log(m_State);
    }

    public void ExecutePlayerSkill(PlayerSkill playerSkill, Vector3 direction)
    {
        m_State = playerSkill;

        switch(playerSkill)
        {
            case PlayerSkill.Roll:
                StartRoll(direction);
                break;
        }
    }

    private void StartRoll(Vector3 direction)
    {
        StartCoroutine(RollCoroutine(direction));
    }

    private IEnumerator RollCoroutine(Vector3 character_forward) {
        Vector3 start_pos = transform.position;
        Vector3 target_pos = correctedPos + character_forward*ROLL_DISTANCE;
        target_pos = ClampPosition(target_pos);

        realPosition = target_pos;

        float ctime = 0f;
        float roll_time = 1f;
        SetRotation(character_forward);

        while (ctime < roll_time) {
            float dt = (1f - Mathf.Cos(ctime*180f*Mathf.Deg2Rad)) / 2f;
            transform.position = Vector3.Lerp(start_pos, target_pos, dt/roll_time);

            ctime += Time.deltaTime;
            yield return null;
        }
        yield break;
    }

    protected void SetRotation(Vector3 direction) {
        m_CharacterModel.rotation = Quaternion.LookRotation(direction);
    }

    void ReleaseBlock() {
        //m_State = 0;
    }

    void FinishAttack1() {
        //m_State = 0;
    }

    void FinishRoll() {
        //m_State = 0;
    }

    public abstract void Start_DealDamage_Attack1();

    public abstract void Finish_DealDamage_Attack1();

    public abstract void OnStateReceived(Vector3 position, ClientInput clientInput);

    public Vector3 ClampPosition(Vector3 position)
    {
        return new Vector3
        (
            Mathf.Clamp(position.x, -50f, 50f),
            position.y,
            Mathf.Clamp(position.z, -50f, 50f)
        );
    }

    public void SetUserName(string _username) {
        this._username = _username;
    }

    public string GetUserName() {
        return _username;
    }

    public void SetCurrentHitPoint(int hitPoints) {
        m_CurrentHp = hitPoints;
        m_UI_HpBar.UpdateHpBarFill();
    }

    public void SetUserNameUI(string _username) {
        m_UI_HpBar.SetUserNameUI(_username);
    }

    private void InterpolatePosition() {
        if (m_State == PlayerSkill.Roll)
            return;
        transform.position = Vector3.Slerp(transform.position, realPosition, 0.25f);
    }
}
