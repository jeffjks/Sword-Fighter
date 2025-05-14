// This is Auto Generated Code by  (Editors.PlayerStateScriptMaker)
using System.Collections.Generic;
using Shared.Enums;

public class UsingSkillState : PlayerStateBase
{
    private PlayerSkillBase _subState;

    private readonly Dictionary<PlayerSkill, PlayerSkillBase> _playerSkills;

    public UsingSkillState(PlayerManager manager) : base(manager)
    {
        _playerSkills = PlayerSkillInitializer.GetPlayerSkillDictionary(manager);
    }
    public override PlayerState Type => PlayerState.UsingSkill;
    public override void Enter() {
        _playerManager.SetStateAnimation(Type);
    }
    public override void Update() => _subState?.Update();
    public override void Exit() {
        SetSubState(PlayerSkill.None);
    }

    public void SetSubState(PlayerSkill newPlayerSkill)
    {
        if (_playerSkills.TryGetValue(newPlayerSkill, out var newStateBase))
        {
            _subState?.Exit();
            _subState = newStateBase;
            _subState.Enter();
        }
    }
}

