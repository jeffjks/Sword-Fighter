using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Animator m_Animator;
    public int id;
    public string username;
    public Transform m_CharacterModel;
    public Collider m_PlayerCollider;
    public Sword m_Sword;
    public bool m_OppositeCharacter;
    public int m_CurrentHp;
    public int m_MaxHp;

    [HideInInspector] public Vector2 m_Movement = Vector2.zero;
    [HideInInspector] public Vector3 direction = Vector3.forward;
    [HideInInspector] public int m_State = 0;
    [HideInInspector] public Vector2Int inputVector_raw;
    [HideInInspector] public Vector2 inputVector;

    private bool m_CanMove = true;
    private bool m_IsRolling = false;
    private const float ROLL_DISTANCE = 5f;

    void Update()
    {
        if (m_State == -1) {
            m_CanMove = false;
            m_PlayerCollider.enabled = false;
            m_Animator.SetInteger("State", m_State);
            inputVector_raw = Vector2Int.zero;
            inputVector = Vector2.zero;
            return;
        }

        if (GameManager.instance.m_UIManager.GetWritingChat()) {
            inputVector_raw = Vector2Int.zero;
            inputVector = Vector2.zero;
        }
        else {
            inputVector_raw = new Vector2Int((int) Input.GetAxisRaw("Horizontal"), (int) Input.GetAxisRaw("Vertical"));
            inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        SetRotation();

        if (m_OppositeCharacter) {
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
        }
        else {
            if (m_State <= 1) {
                if (inputVector_raw != Vector2Int.zero) {
                    if (m_CanMove) {
                        m_State = 1;
                    }
                }
                else {
                    m_State = 0;
                }
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

        if (m_OppositeCharacter) {
            m_Animator.SetFloat("MovementHorizontal", m_Movement.x);
            m_Animator.SetFloat("MovementVertical", m_Movement.y);
        }
        else {
            m_Animator.SetFloat("MovementHorizontal", inputVector.x);
            m_Animator.SetFloat("MovementVertical", inputVector.y);
        }
        m_Animator.SetInteger("State", m_State);

        if (m_State != 3) {
            Finish_DealDamage_Attack1();
        }

        //Debug.Log(m_State);
    }

    private IEnumerator StartRoll() {
        Vector3 character_forward = Vector3.Normalize(new Vector3(m_CharacterModel.forward.x, 0, m_CharacterModel.forward.z));
        Vector3 start_pos = transform.position;
        Vector3 target_pos = transform.position + character_forward*ROLL_DISTANCE;
        float ctime = 0f;
        float roll_time = 1f;
        Vector3 vel = Vector3.zero;

        while (ctime < roll_time) {
            float dt = (1f - Mathf.Cos(ctime*180f*Mathf.Deg2Rad)) / 2f;
            transform.position = Vector3.Lerp(start_pos, target_pos, dt/roll_time);
            transform.position = ClampPosition(transform.position);

            ctime += Time.deltaTime;
            yield return null;
        }
        yield break;
    }

    public void SetRotation() {
        if (!m_OppositeCharacter) {
            return;
        }
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

    void Start_DealDamage_Attack1() {
        if (m_OppositeCharacter) {
            return;
        }
        m_Sword.StartDeal();
    }

    void Finish_DealDamage_Attack1() {
        if (m_OppositeCharacter) {
            return;
        }
        m_Sword.FinishDeal();
    }

    public Vector3 ClampPosition(Vector3 position)
    {
        return new Vector3
        (
            Mathf.Clamp(position.x, -50f, 50f),
            position.y,
            Mathf.Clamp(position.z, -50f, 50f)
        );
    }
}
