// This is Auto Generated Code by  (Editors.PlayerStateScriptMaker)
using Shared.Enums;

public class IdleState : PlayerStateBase
{
    public IdleState(PlayerManager manager) : base(manager) { }
    public override PlayerState Type => PlayerState.Idle;
    public override void Enter() {
        _playerManager.SetStateAnimation(Type);
    }
    public override void Update() { }
}

