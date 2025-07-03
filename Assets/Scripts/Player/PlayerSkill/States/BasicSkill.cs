// This is Auto Generated Code by (Editors.PlayerSkillScriptMaker)
using Shared.Enums;

public class BasicSkill : PlayerSkillBase
{
    public BasicSkill(PlayerManager manager) : base(manager) { }
    public override PlayerSkill Type => PlayerSkill.Basic;
    public override void Enter() {
        _playerManager.SetSkillAnimation(Type);
    }
    public override void Update() { }
}

