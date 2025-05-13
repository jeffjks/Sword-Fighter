using System.Collections.Generic;
using Shared.Enums;

public abstract class PlayerStateBase
{
    public abstract PlayerState Type { get; }

    protected PlayerManager _playerManager;

    public PlayerStateBase(PlayerManager manager) => _playerManager = manager;

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}

// ───────── 상위 상태들 ─────────

public class DeadState : PlayerStateBase
{
    public DeadState(PlayerManager manager) : base(manager) { }
    public override PlayerState Type => PlayerState.Dead;
    public override void Enter() {
        _playerManager.m_IsMovable = false;
        _playerManager.m_PlayerCollider.enabled = false;
        _playerManager.SetStateAnimation(Type);
    }
    public override void Update() { }
}

public class IdleState : PlayerStateBase
{
    public IdleState(PlayerManager manager) : base(manager) { }
    public override PlayerState Type => PlayerState.Idle;
    public override void Enter() {
        _playerManager.SetStateAnimation(Type);
    }
    public override void Update() { }
}

public class MoveState : PlayerStateBase
{
    public MoveState(PlayerManager manager) : base(manager) { }
    public override PlayerState Type => PlayerState.Move;
    public override void Enter() {
        _playerManager.SetStateAnimation(Type);
    }
    public override void Update() { }
}

public class UsingSkillState : PlayerStateBase
{
    private SkillStateBase _subState;

    private readonly Dictionary<PlayerSkill, SkillStateBase> _playerSkills;

    public UsingSkillState(PlayerManager manager) : base(manager)
    {
        // 여기서 상태 초기화
        _playerSkills = new Dictionary<PlayerSkill, SkillStateBase>
        {
            { PlayerSkill.None, new NoneSkill(manager) },
            { PlayerSkill.Block, new BlockSkill(manager) },
            { PlayerSkill.Basic, new AttckSkill(manager) },
            { PlayerSkill.Roll, new RollSkill(manager) }
        };
    }

    public override PlayerState Type => PlayerState.UsingSkill;

    public void SetSubState(PlayerSkill newPlayerSkill)
    {
        if (_playerSkills.TryGetValue(newPlayerSkill, out var newStateBase))
        {
            _subState?.Exit();
            _subState = newStateBase;
            _subState.Enter();
        }
    }

    public override void Enter() {
        _playerManager.SetStateAnimation(Type);
    }
    public override void Update() => _subState?.Update();
    public override void Exit() {
        SetSubState(PlayerSkill.None);
    }
}

// ───────── 하위 스킬 상태들 ─────────

public abstract class SkillStateBase
{
    public abstract PlayerSkill Type { get; }
    protected PlayerManager _playerManager;
    public SkillStateBase(PlayerManager manager) => _playerManager = manager;

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}

public class NoneSkill : SkillStateBase
{
    public NoneSkill(PlayerManager manager) : base(manager) { }
    public override PlayerSkill Type => PlayerSkill.None;
    public override void Enter() {
        _playerManager.SetSkillAnimation(Type);
    }
    public override void Update() { }
}

public class AttckSkill : SkillStateBase
{
    public AttckSkill(PlayerManager manager) : base(manager) { }
    public override PlayerSkill Type => PlayerSkill.Basic;
    public override void Enter() {
        _playerManager.SetSkillAnimation(Type);
    }
    public override void Update() { }
}

public class BlockSkill : SkillStateBase
{
    public BlockSkill(PlayerManager manager) : base(manager) { }
    public override PlayerSkill Type => PlayerSkill.Block;
    public override void Enter() {
        _playerManager.SetSkillAnimation(Type);
    }
    public override void Update() { }
}

public class RollSkill : SkillStateBase
{
    public RollSkill(PlayerManager manager) : base(manager) { }
    public override PlayerSkill Type => PlayerSkill.Roll;
    public override void Enter() {
        _playerManager.SetSkillAnimation(Type);
        _playerManager.m_EnableInterpolate = false;
    }
    public override void Update() { }
    public override void Exit() {
        _playerManager.m_EnableInterpolate = true;
    }
}

// ───────── 상태머신 ─────────

public class PlayerStateMachine
{
    private PlayerStateBase _currentStateBase;
    private UsingSkillState _usingSkillState;

    private readonly Dictionary<PlayerState, PlayerStateBase> _playerStates;

    public PlayerStateBase CurrentState => _currentStateBase;

    public PlayerStateMachine(PlayerManager manager)
    {
        // 여기서 상태 초기화
        _playerStates = new Dictionary<PlayerState, PlayerStateBase>
        {
            { PlayerState.Idle, new IdleState(manager) },
            { PlayerState.Move, new MoveState(manager) },
            { PlayerState.UsingSkill, new UsingSkillState(manager) },
            { PlayerState.Dead, new DeadState(manager) }
        };

        _usingSkillState = new(manager);
    }

    public void SetState(PlayerState newState)
    {
        if (_currentStateBase?.Type == newState) // 현재 상태와 동일하면 무시
            return;
        
        if (_playerStates.TryGetValue(newState, out var newStateBase))
        {
            _currentStateBase?.Exit();
            _currentStateBase = newStateBase;
            _currentStateBase.Enter();
        }
    }

    public void SetSkill(PlayerSkill skillState)
    {
        if (_currentStateBase is not UsingSkillState)
        {
            SetState(PlayerState.UsingSkill);
        }

        _usingSkillState.SetSubState(skillState);
    }
}