// This is Auto Generated Code by  (Editors.PlayerStateScriptMaker)
using Shared.Enums;

public class MoveState : PlayerStateBase
{
    public MoveState(PlayerManager manager) : base(manager) { }
    public override PlayerState Type => PlayerState.Move;
    public override void Enter() {
        _playerManager.SetStateAnimation(Type);
    }
    public override void Update() { }
}

