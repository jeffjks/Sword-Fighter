// This is Auto Generated Code by (Editors.PlayerSkillScriptMaker)
using Shared.Enums;

public class RollSkill : PlayerSkillBase
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

