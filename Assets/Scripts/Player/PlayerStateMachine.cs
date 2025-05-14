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

public abstract class PlayerSkillBase
{
    public abstract PlayerSkill Type { get; }
    protected PlayerManager _playerManager;
    public PlayerSkillBase(PlayerManager manager) => _playerManager = manager;

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}

public class PlayerStateMachine
{
    private PlayerStateBase _currentStateBase;
    private UsingSkillState _usingSkillState;

    private readonly Dictionary<PlayerState, PlayerStateBase> _playerStates;

    public PlayerStateBase CurrentState => _currentStateBase;

    public PlayerStateMachine(PlayerManager manager)
    {
        _playerStates = PlayerStateInitializer.GetPlayerStateDictionary(manager);

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