using UnityEngine;

public abstract class PlayerStateBase
{
    protected PlayerStateMachine _fsm;

    public PlayerStateBase(PlayerStateMachine _fsm) => this._fsm = _fsm;

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}

// ───────── 상위 상태들 ─────────

public class DeadState : PlayerStateBase
{
    public DeadState(PlayerStateMachine _fsm) : base(_fsm) { }
    public override void Enter() { }
    public override void Update() { }
}

public class IdleState : PlayerStateBase
{
    public IdleState(PlayerStateMachine _fsm) : base(_fsm) { }
    public override void Enter() { }
    public override void Update() { }
}

public class MoveState : PlayerStateBase
{
    public MoveState(PlayerStateMachine _fsm) : base(_fsm) { }
    public override void Enter() { }
    public override void Update() { }
}

public class UsingSkillState : PlayerStateBase
{
    private SkillStateBase subState;

    public UsingSkillState(PlayerStateMachine _fsm) : base(_fsm) { }

    public void SetSubState(SkillStateBase skillState)
    {
        subState?.Exit();
        subState = skillState;
        subState.Enter();
    }

    public override void Enter() { }
    public override void Update() => subState?.Update();
    public override void Exit() => subState?.Exit();
}

// ───────── 하위 스킬 상태들 ─────────

public abstract class SkillStateBase
{
    protected PlayerStateMachine _fsm;
    public SkillStateBase(PlayerStateMachine _fsm) => this._fsm = _fsm;

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}

public class AttackState : SkillStateBase
{
    public AttackState(PlayerStateMachine _fsm) : base(_fsm) { }
    public override void Enter() { }
    public override void Update() { }
}

public class BlockState : SkillStateBase
{
    public BlockState(PlayerStateMachine _fsm) : base(_fsm) { }
    public override void Enter() { }
    public override void Update() { }
}

public class RollState : SkillStateBase
{
    public RollState(PlayerStateMachine _fsm) : base(_fsm) { }
    public override void Enter() { }
    public override void Update() { }
}

// ───────── 상태머신 ─────────

public class PlayerStateMachine
{
    private PlayerStateBase currentState;
    private UsingSkillState usingSkillState;

    public void SetState(PlayerStateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void SetSkill(SkillStateBase skillState)
    {
        if (currentState is not UsingSkillState)
        {
            usingSkillState ??= new UsingSkillState(this);
            SetState(usingSkillState);
        }

        usingSkillState.SetSubState(skillState);
    }

    public void Update() => currentState?.Update();

    // 상태 전환 예시
    public void OnDead() => SetState(new DeadState(this));
    public void OnMove() => SetState(new MoveState(this));
    public void OnIdle() => SetState(new IdleState(this));
    public void OnAttack() => SetSkill(new AttackState(this));
    public void OnBlock() => SetSkill(new BlockState(this));
    public void OnRoll() => SetSkill(new RollState(this));
}