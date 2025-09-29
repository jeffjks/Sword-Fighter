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

    [HideInInspector] public Vector3 m_RealPosition;

    protected int _lastSeqNum;

    private CancellationTokenSource _cts;

    private string _username;
    private Vector2 _animationMovement;


    private readonly int _animatorMovementHorizontal = Animator.StringToHash("MovementHorizontal");
    private readonly int _animatorMovementVertical = Animator.StringToHash("MovementVertical");
    private readonly int _animatorPlayerState = Animator.StringToHash("State");
    private readonly int _animatorPlayerSkill = Animator.StringToHash("Skill");

    private void Awake()
    {
        CurrentStateMachine = new(this);
    }

    private void OnEnable()
    {
        if (_cts != null)
        {
            _cts.Dispose();
        }
        _cts = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
    }

    public void Init() {
        SetPlayerUI(_username);
        SetCurrentHitPoint(m_CharacterStatus.CurrentHitPoint);
    }

    protected virtual void Update()
    {
        PlayMovementAnimation();
        InterpolatePosition();

        var delta = Time.deltaTime;
        m_CharacterStatus.Tick(delta);
    }

    public abstract void OnStateReceived(int seqNum, long timestamp, Vector3 facingDirection, Vector3 deltaPos, Vector2 inputVector, Vector3 position);
    public abstract void OnStateReceived(long timestamp, PlayerSkill playerSkill, Vector3 facingDirection, Vector3 targetPosition);

    private void PlayMovementAnimation()
    {
        if (IsCurrentState(PlayerState.Dead)) {
            return;
        }

        m_Animator.SetFloat(_animatorMovementHorizontal, _animationMovement.x, 0.25f, Time.deltaTime);
        m_Animator.SetFloat(_animatorMovementVertical, _animationMovement.y, 0.25f, Time.deltaTime);
    }

    private void CancelCurrentSkill()
    {
        // 이전 실행 취소
        _cts?.Cancel();
        _cts?.Dispose();

        _cts = new CancellationTokenSource();
    }

    public async UniTask ExecuteSkillAsync(long timestamp, PlayerSkill skillType, Vector3 direction, Vector3 targetPos)
    {
        if (!SkillRegistry.SkillMap.TryGetValue(skillType, out ModularSkill skill))
        {
            Debug.LogWarning($"스킬 {skillType} 이 등록되어 있지 않음");
            return;
        }

        CancelCurrentSkill();

        CurrentStateMachine.SetState(PlayerState.UsingSkill);
        CurrentSkill = skillType;

        try
        {
            await skill.Execute(timestamp, new SkillContext(this, direction, targetPos), _cts.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"스킬 취소됨: {skillType}");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        finally
        {
            CurrentStateMachine.SetState(PlayerState.Idle);
            SetSkillAnimation(PlayerSkill.None);
            CurrentSkill = PlayerSkill.None;

            _cts?.Dispose();
            _cts = null;
        }
    }

    public void SetRotation(Vector3 direction)
    {
        var rotationDirection = new Vector3(direction.x, 0f, direction.z);
        m_CharacterModel.rotation = Quaternion.LookRotation(rotationDirection);
    }

    public abstract void SetPlayerUI(string userName);

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
