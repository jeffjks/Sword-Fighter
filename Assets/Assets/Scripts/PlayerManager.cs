using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [HideInInspector] public int m_State = 0;
    [HideInInspector] public Vector3 realPosition;

    private string username;
    private bool m_CanMove = true;
    private bool m_IsRolling = false;
    private const float ROLL_DISTANCE = 5f;

    public void Init() {
        SetUserNameUI(username);
        SetCurrentHitPoint(m_CurrentHp);
    }

    public bool IsDead() {
        if (m_State == -1) {
            m_CanMove = false;
            m_PlayerCollider.enabled = false;
            m_Animator.SetInteger("State", m_State);
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

        if (m_State <= 1) {
            if (m_Movement != Vector2.zero) {
                if (m_CanMove) {
                    m_State = 1;
                }
            }
            else {
                m_State = 0;
            }
        }

        if (m_State == 5) {
            if (!m_IsRolling) {
                StartCoroutine(StartRoll());
                m_IsRolling = true;
            }
        }
        else {
            m_IsRolling = false;
        }

        m_Animator.SetFloat("MovementHorizontal", m_Movement.x);
        m_Animator.SetFloat("MovementVertical", m_Movement.y);
        m_Animator.SetInteger("State", m_State);

        if (m_State != 3) {
            Finish_DealDamage_Attack1();
        }
        
        InterpolatePosition();

        //Debug.Log(m_State);
    }

    private IEnumerator StartRoll() {
        Vector3 character_forward = Vector3.Normalize(new Vector3(m_CharacterModel.forward.x, 0, m_CharacterModel.forward.z));
        Vector3 start_pos = realPosition;
        Vector3 target_pos = realPosition + character_forward*ROLL_DISTANCE;
        float ctime = 0f;
        float roll_time = 1f;
        Vector3 vel = Vector3.zero;

        while (ctime < roll_time) {
            float dt = (1f - Mathf.Cos(ctime*180f*Mathf.Deg2Rad)) / 2f;
            realPosition = Vector3.Lerp(start_pos, target_pos, dt/roll_time);
            realPosition = ClampPosition(realPosition);

            ctime += Time.deltaTime;
            yield return null;
        }
        yield break;
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

    public Vector3 ClampPosition(Vector3 position)
    {
        return new Vector3
        (
            Mathf.Clamp(position.x, -50f, 50f),
            position.y,
            Mathf.Clamp(position.z, -50f, 50f)
        );
    }

    public void SetUserName(string username) {
        this.username = username;
    }

    public string GetUserName() {
        return username;
    }

    public void SetCurrentHitPoint(int hitPoints) {
        m_CurrentHp = hitPoints;
        m_UI_HpBar.UpdateHpBarFill();
    }

    public void SetUserNameUI(string username) {
        m_UI_HpBar.SetUserNameUI(username);
    }

    private void InterpolatePosition() {
        transform.position = Vector3.Slerp(transform.position, realPosition, 0.15f);
    }
}
