using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

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
    public int id;
    public Transform m_CharacterModel;
    public Collider m_PlayerCollider;
    public UI_HpBar m_UI_HpBar;
    public int m_CurrentHp;
    public int m_MaxHp;
    public Vector3 m_DeltaPos;
    public Animator m_Animator;
    public bool m_IsMovable = true;
    public readonly Dictionary<PlayerSkill, int> m_SkillDurations = new()
    {
        { PlayerSkill.Attack1, 800 },
        { PlayerSkill.Block, 1500 },
        { PlayerSkill.Roll, 1000 }
    };


    [HideInInspector] public Vector3 m_RealPosition;

    protected int _lastSeqNum;

    private CancellationTokenSource _cts;
    private PlayerState _currentState;
    private PlayerSkill _currentSkill;

    public PlayerState CurrentState
    {
        get {
            return _currentState;
        }
        set
        {
            if (_currentState == value)
                return;
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
            if (_currentSkill == value)
                return;
            _currentSkill = value;
            OnPlayerSkillChanged?.Invoke(_currentSkill);
        }
    }

    public event UnityAction<PlayerState> OnPlayerStateChanged;
    public event UnityAction<PlayerSkill> OnPlayerSkillChanged;

    // [HideInInspector] public PlayerState m_PlayerState = PlayerState.Idle;
    // [HideInInspector] public PlayerSkill m_PlayerSkill = PlayerSkill.None;

    private string _username;
    private Vector2 _animationMovement;


    private readonly int _animatorMovementHorizontal = Animator.StringToHash("MovementHorizontal");
    private readonly int _animatorMovementVertical = Animator.StringToHash("MovementVertical");
    private readonly int _animatorPlayerState = Animator.StringToHash("State");
    private readonly int _animatorPlayerSkill = Animator.StringToHash("Skill");
    private const float ROLL_DISTANCE = 5f;

    public void Init() {
        SetUserNameUI(_username);
        SetCurrentHitPoint(m_CurrentHp);
    }

    private void OnEnable()
    {
        OnPlayerStateChanged += HandleStateChange;
        OnPlayerStateChanged += SetStateAnimation;
        OnPlayerSkillChanged += SetSkillAnimation;
    }

    private void OnDisable()
    {
        OnPlayerStateChanged -= HandleStateChange;
        OnPlayerStateChanged -= SetStateAnimation;
        OnPlayerSkillChanged -= SetSkillAnimation;
    }

    private void HandleStateChange(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Dead:
                m_IsMovable = false;
                m_PlayerCollider.enabled = false;
                //m_Animator.SetInteger("State", (int)playerState);
                // m_Movement = Vector2.zero;
                break;
            case PlayerState.Idle:
            case PlayerState.Move:
                CurrentSkill = PlayerSkill.None;
                break;
            case PlayerState.UsingSkill:
                // m_Movement = Vector2.zero;
                break;
        }
    }

    protected virtual void Update()
    {
        if (CurrentState == PlayerState.Dead) {
            return;
        }

        m_Animator.SetFloat(_animatorMovementHorizontal, _animationMovement.x, 0.25f, Time.deltaTime);
        m_Animator.SetFloat(_animatorMovementVertical, _animationMovement.y, 0.25f, Time.deltaTime);

        //Debug.Log($"Skill: {CurrentSkill}, State: {CurrentState}");

        if (CurrentSkill != PlayerSkill.Attack1) {
            Finish_DealDamage_Attack1();
        }
        
        InterpolatePosition();

        //Debug.Log(m_State);
    }

    public bool ExecutePlayerSkill(long timestamp, PlayerSkill playerSkill, Vector3 facingDirection, Vector3? targetPosition = null)
    {
        if (m_SkillDurations.TryGetValue(playerSkill, out int duration) == false)
            return false;
        if (playerSkill == PlayerSkill.None)
            return false;
        
        _cts?.Cancel(); // 이전 예약 취소
        _cts = new CancellationTokenSource();
        
        IdleAfterDelay(duration, _cts.Token).Forget();

        if (this is PlayerMe)
            ClientSend.PlayerSkill(timestamp, facingDirection, playerSkill);


        CurrentSkill = playerSkill;
        
        CurrentState = PlayerState.UsingSkill;

        switch(playerSkill)
        {
            case PlayerSkill.Roll:
                StartRoll(facingDirection, targetPosition);
                break;
        }
        return true;
    }

    private async UniTaskVoid IdleAfterDelay(int delayMilliseconds, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(delayMilliseconds, cancellationToken: token);
            CurrentState = PlayerState.Idle;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void StartRoll(Vector3 direction, Vector3? targetPosition)
    {
        StartCoroutine(RollCoroutine(direction, targetPosition));
    }

    private IEnumerator RollCoroutine(Vector3 facingDirection, Vector3? targetPosition) {
        Vector3 start_pos = transform.position;
        Vector3 target_pos = targetPosition ?? m_RealPosition + facingDirection * ROLL_DISTANCE;
        target_pos = ClampPosition(target_pos);

        m_RealPosition = target_pos;

        float ctime = 0f;
        float roll_time = 1f;
        SetRotation(facingDirection);
        //Debug.Log($"{start_pos}, ({correctedPos}), {target_pos}");

        while (ctime < roll_time) {
            float dt = (1f - Mathf.Cos(ctime*180f*Mathf.Deg2Rad)) / 2f;
            transform.position = Vector3.Lerp(start_pos, target_pos, dt/roll_time);

            ctime += Time.deltaTime;
            yield return null;
        }
        yield break;
    }

    public void SetRotation(Vector3 direction)
    {
        var rotationDirection = new Vector3(direction.x, 0f, direction.z);
        m_CharacterModel.rotation = Quaternion.LookRotation(rotationDirection);
    }

    public abstract void Start_DealDamage_Attack1();

    public abstract void Finish_DealDamage_Attack1();

    public abstract void OnStateReceived(int seqNum, long timestamp, Vector3 facingDirection, Vector3 deltaPos, Vector2 inputVector, Vector3 position);
    public abstract void OnStateReceived(long timestamp, PlayerSkill playerSkill, Vector3 facingDirection, Vector3 targetPosition);

    public Vector3 ClampPosition(Vector3 position)
    {
        return new Vector3
        (
            Mathf.Clamp(position.x, -50f, 50f),
            position.y,
            Mathf.Clamp(position.z, -50f, 50f)
        );
    }

    private void SetStateAnimation(PlayerState playerState)
    {
        m_Animator.SetInteger(_animatorPlayerState, (int) playerState);
    }

    private void SetSkillAnimation(PlayerSkill playerSkill)
    {
        m_Animator.SetInteger(_animatorPlayerSkill, (int) playerSkill);
    }

    public void SetMovementAnimation(Vector2 movement)
    {
        _animationMovement = movement;
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
        m_UI_HpBar = GameManager.instance.m_UIManager.m_UI_HpBarMain;
        m_UI_HpBar.SetUserNameUI(_username);
    }

    private void InterpolatePosition() {
        if (CurrentSkill == PlayerSkill.Roll)
            return;
        transform.position = Vector3.Slerp(transform.position, m_RealPosition, 0.25f);
    }
}
