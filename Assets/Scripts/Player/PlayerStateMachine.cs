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


// ───────── 하위 스킬 상태들 ─────────

public abstract class PlayerSkillBase
{
    public abstract PlayerSkill Type { get; }
    protected PlayerManager _playerManager;
    public PlayerSkillBase(PlayerManager manager) => _playerManager = manager;

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
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