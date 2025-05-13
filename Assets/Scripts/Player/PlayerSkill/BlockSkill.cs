// This is Auto Generated Code by (Editors.PlayerSkillScriptMaker)
using Shared.Enums;

public class BlockSkill : PlayerSkillBase
{
    public BlockSkill(PlayerManager manager) : base(manager) { }
    public override PlayerSkill Type => PlayerSkill.Block;
    public override void Enter() {
        _playerManager.SetSkillAnimation(Type);
    }
    public override void Update() { }
}

