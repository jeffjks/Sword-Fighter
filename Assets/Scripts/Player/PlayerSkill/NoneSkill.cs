// This is Auto Generated Code by (Editors.PlayerSkillScriptMaker)
using Shared.Enums;

public class NoneSkill : PlayerSkillBase
{
    public NoneSkill(PlayerManager manager) : base(manager) { }
    public override PlayerSkill Type => PlayerSkill.None;
    public override void Enter() {
        _playerManager.SetSkillAnimation(Type);
    }
    public override void Update() { }
}

