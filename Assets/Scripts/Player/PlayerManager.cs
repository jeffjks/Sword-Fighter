using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Shared.Enums;

public abstract class PlayerManager : MonoBehaviour
{
    public int id;
    public Transform m_CharacterModel;
    public UI_HpBar m_UI_HpBar;
    public Collider m_PlayerCollider;
    public CharacterStatus m_CharacterStatus;
    public Vector3 m_DeltaPos;
    public Animator m_Animator;
    public bool m_IsMovable = true;
    public bool m_EnableInterpolate = true;
    public PlayerStateMachine CurrentStateMachine { get; private set; }
    public PlayerState CurrentState {
        get {
            return CurrentStateMachine.CurrentState.Type;
        }
    }
    public PlayerSkill CurrentSkill { get; set; }
    public readonly Dictionary<PlayerSkill, int> m_SkillDurations = new()
    {
        { PlayerSkill.Basic, 800 },
        { PlayerSkill.Block, 1500 },
        { PlayerSkill.Roll, 1000 }
    };


    [HideInInspector] public Vector3 m_RealPosition;

    protected int _lastSeqNum;

    private CancellationTokenSource _cts;

    // [HideInInspector] public PlayerState m_PlayerState = PlayerState.Idle;
    // [HideInInspector] public PlayerSkill m_PlayerSkill = PlayerSkill.None;

    private string _username;
    private Vector2 _animationMovement;


    private readonly int _animatorMovementHorizontal = Animator.StringToHash("MovementHorizontal");
    private readonly int _animatorMovementVertical = Animator.StringToHash("MovementVertical");
    private readonly int _animatorPlayerState = Animator.StringToHash("State");
    private readonly int _animatorPlayerSkill = Animator.StringToHash("Skill");
    private const float ROLL_DISTANCE = 5f;

    private void Awake()
    {
        CurrentStateMachine = new(this);
    }

    public void Init() {
        SetUserNameUI(_username);
        SetCurrentHitPoint(m_CharacterStatus.CurrentHitPoint);
    }

    protected virtual void Update()
    {
        PlayMovementAnimation();
        InterpolatePosition();

        var delta = Time.deltaTime;
        m_CharacterStatus.Tick(delta);
    }

    private void PlayMovementAnimation()
    {
        if (IsCurrentState(PlayerState.Dead)) {
            return;
        }

        m_Animator.SetFloat(_animatorMovementHorizontal, _animationMovement.x, 0.25f, Time.deltaTime);
        m_Animator.SetFloat(_animatorMovementVertical, _animationMovement.y, 0.25f, Time.deltaTime);
    }

    public void RegisterSkill(PlayerSkill skillType, SkillBase skillAsset)
    {
        SkillRegistry.SkillMap[skillType] = skillAsset;
    }

    public bool ExecuteSkill(PlayerSkill skillType, Vector3 direction, Vector3 targetPos)
    {
        if (!SkillRegistry.SkillMap.TryGetValue(skillType, out SkillBase skill))
        {
            Debug.LogWarning($"스킬 {skillType} 이 등록되어 있지 않음");
            return false;
        }

        // 이전 스킬 취소
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        // CurrentStateMachine.SetSkill(playerSkill);

        try
        {
            skill.Execute(new(this, direction, targetPos), _cts.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"스킬 취소됨: {skillType}");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        return true;
    }

    public void SetRotation(Vector3 direction)
    {
        var rotationDirection = new Vector3(direction.x, 0f, direction.z);
        m_CharacterModel.rotation = Quaternion.LookRotation(rotationDirection);
    }

    public abstract void Start_DealDamage_Basic();

    public abstract void Finish_DealDamage_Basic();

    public abstract void OnStateReceived(int seqNum, long timestamp, Vector3 facingDirection, Vector3 deltaPos, Vector2 inputVector, Vector3 position);
    public abstract void OnStateReceived(PlayerSkill playerSkill, Vector3 facingDirection, Vector3 targetPosition);

    public Vector3 ClampPosition(Vector3 position)
    {
        return new Vector3
        (
            Mathf.Clamp(position.x, -50f, 50f),
            position.y,
            Mathf.Clamp(position.z, -50f, 50f)
        );
    }

    public void SetStateAnimation(PlayerState playerState)
    {
        m_Animator.SetInteger(_animatorPlayerState, (int) playerState);
    }

    public void SetSkillAnimation(PlayerSkill playerSkill)
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
        m_CharacterStatus.CurrentHitPoint = hitPoints;
        m_UI_HpBar.UpdateHpBarFill();
    }

    public void SetUserNameUI(string _username) {
        m_UI_HpBar = GameManager.instance.m_UIManager.m_UI_HpBarMain;
        m_UI_HpBar.SetUserNameUI(_username);
    }

    private void InterpolatePosition() {
        if (m_EnableInterpolate == false)
            return;
        transform.position = Vector3.Slerp(transform.position, m_RealPosition, 0.25f);
    }

    public bool IsCurrentState(PlayerState type)
    {
        return CurrentStateMachine.CurrentState?.Type == type;
    }
}
