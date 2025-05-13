// This is Auto Generated Code by  (Editors.PlayerStateScriptMaker)
using Shared.Enums;

public class DeadState : PlayerStateBase
{
    public DeadState(PlayerManager manager) : base(manager) { }
    public override PlayerState Type => PlayerState.Dead;
    public override void Enter() {
        _playerManager.m_IsMovable = false;
        _playerManager.m_PlayerCollider.enabled = false;
        _playerManager.SetStateAnimation(Type);
    }
    public override void Update() { }
}

