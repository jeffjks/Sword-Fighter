using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum PlayerState
{
    Dead = -1,
    Idle,
    Move,
    UsingSkill
}

public enum PlayerSkill
{
    None,
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

    [HideInInspector] public Vector2 m_Movement = Vector2.zero;
    [HideInInspector] public Vector3 direction = Vector3.forward;
    [HideInInspector] public Vector3 realPosition;

    protected Vector3 correctedPos;

    private PlayerState _currentState;
    private PlayerSkill _currentSkill;

    public PlayerState CurrentState
    {
        get {
            return _currentState;
        }
        set
        {
            _currentState = value;
            OnPlayerStateChanged?.Invoke(_currentState);
        }
    }

    public PlayerSkill CurrentSkill
    {
        get {
            return _currentSkill;
        }
        set
        {
            _currentSkill = value;
            OnPlayerSkillChanged?.Invoke(_currentSkill);
        }
    }

    public event UnityAction<PlayerState> OnPlayerStateChanged;
    public event UnityAction<PlayerSkill> OnPlayerSkillChanged;

    // [HideInInspector] public PlayerState m_PlayerState = PlayerState.Idle;
    // [HideInInspector] public PlayerSkill m_PlayerSkill = PlayerSkill.None;

    private string _username;
    private bool m_IsMovable = true;

    private const float ROLL_DISTANCE = 5f;

    public void Init() {
        SetUserNameUI(_username);
        SetCurrentHitPoint(m_CurrentHp);
    }

    private void OnEnable()
    {
        OnPlayerStateChanged += HandleStateChange;
    }

    private void OnDisable()
    {
        OnPlayerStateChanged -= HandleStateChange;
    }

    private void HandleStateChange(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Dead:
                m_IsMovable = false;
                m_PlayerCollider.enabled = false;
                m_Animator.SetInteger("State", (int)playerState);
                m_Movement = Vector2.zero;
                break;
            case PlayerState.Idle:
            case PlayerState.Move:
                CurrentSkill = PlayerSkill.None;
                break;
            case PlayerState.UsingSkill:
                m_Movement = Vector2.zero;
                break;
        }
    }

    protected virtual void Update()
    {
        if (CurrentState == PlayerState.Dead) {
            return;
        }

        if (CurrentState == PlayerState.Idle || CurrentState == PlayerState.Move) {
            if (m_Movement != Vector2.zero) {
                if (m_IsMovable) {
                    CurrentState = PlayerState.Move;
                }
            }
            else {
                CurrentState = PlayerState.Idle;
            }
        }

        m_Animator.SetFloat("MovementHorizontal", m_Movement.x);
        m_Animator.SetFloat("MovementVertical", m_Movement.y);
        m_Animator.SetInteger("Skill", (int)CurrentSkill);
        m_Animator.SetInteger("State", (int)CurrentState);

        //Debug.Log($"Skill: {CurrentSkill}, State: {CurrentState}");

        if (CurrentSkill != PlayerSkill.Attack1) {
            Finish_DealDamage_Attack1();
        }
        
        InterpolatePosition();

        //Debug.Log(m_State);
    }

    public void ExecutePlayerSkill(PlayerSkill playerSkill, Vector3 direction)
    {
        CurrentSkill = playerSkill;

        if (playerSkill == PlayerSkill.None)
            return;
        
        CurrentState = PlayerState.UsingSkill;

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
        Vector3 target_pos = realPosition + character_forward*ROLL_DISTANCE; // TEMP
        target_pos = ClampPosition(target_pos);

        realPosition = target_pos;

        float ctime = 0f;
        float roll_time = 1f;
        SetRotation(character_forward);
        Debug.Log($"{start_pos}, ({correctedPos}), {target_pos}");

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
        if (CurrentSkill == PlayerSkill.Roll)
            return;
        transform.position = Vector3.Slerp(transform.position, realPosition, 0.25f);
    }
}
