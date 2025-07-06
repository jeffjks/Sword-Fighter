// This is Auto Generated Code by  (Editors.PlayerStateScriptMaker)
using Shared.Enums;

public class UsingSkillState : PlayerStateBase
{
    public UsingSkillState(PlayerManager manager) : base(manager) { }
    public override PlayerState Type => PlayerState.UsingSkill;
    public override void Enter() {
        _playerManager.SetStateAnimation(Type);
    }
    public override void Update() { }
}

