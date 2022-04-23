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
    public int m_RollDistance; // 1280
    public bool m_OppositeCharacter;
    public int m_CurrentHp;
    public int m_MaxHp;

    [HideInInspector]
    public Vector2 m_Movement = Vector2.zero;
    [HideInInspector]
    public Vector3 direction = Vector3.forward;
    [HideInInspector]
    public int m_State = 0;

    private bool m_CanMove = true;
    private bool m_IsRolling = false;

    void Update()
    {
        if (m_State == -1) {
            m_CanMove = false;
            m_PlayerCollider.enabled = false;
            m_Animator.SetInteger("State", m_State);
            return;
        }

        SetRotation();

        int horizontal_raw = (int) Input.GetAxisRaw("Horizontal");
        int vertical_raw = (int) Input.GetAxisRaw("Vertical");

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
                if (horizontal_raw != 0 || vertical_raw != 0) {
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

        if (m_OppositeCharacter) {
            m_Animator.SetFloat("MovementHorizontal", m_Movement.x);
            m_Animator.SetFloat("MovementVertical", m_Movement.y);
        }
        else {
            m_Animator.SetFloat("MovementHorizontal", Input.GetAxis("Horizontal"));
            m_Animator.SetFloat("MovementVertical", Input.GetAxis("Vertical"));
        }
        m_Animator.SetInteger("State", m_State);

        if (m_State != 3) {
            Finish_DealDamage_Attack1();
        }

        //Debug.Log(m_State);
    }

    private IEnumerator StartRoll() {
        //Vector3 tmp_pos = (Vector3) unit.pos;
        Vector3 character_forward = Vector3.Normalize(new Vector3(m_CharacterModel.forward.x, 0, m_CharacterModel.forward.z));
        //Vector3 target_pos = tmp_pos + character_forward*m_RollDistance;
        Vector3 start_pos = transform.position;
        Vector3 target_pos = transform.position + character_forward*m_RollDistance/256;
        float ctime = 0f;
        float roll_time = 1f;
        Vector3 vel = Vector3.zero;

        while (ctime < roll_time) {
            float dt = (1f - Mathf.Cos(ctime*180f*Mathf.Deg2Rad)) / 2f;
            //unit.pos = Vector3Int.FloorToInt(Vector3.Lerp(tmp_pos, target_pos, dt/roll_time));
            if (!m_OppositeCharacter) {
                transform.position = Vector3.Lerp(start_pos, target_pos, dt/roll_time);
            }
            //transform.position = (Vector3) unit.pos/256;
            ctime += Time.deltaTime;
            yield return null;
        }
        m_IsRolling = false;
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
}
