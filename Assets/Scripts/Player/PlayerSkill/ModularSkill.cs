using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Shared.Enums;

public class SkillContext
{
    public PlayerManager caster;

    public Vector3 direction;
    public Vector3 targetPosition;

    public float castTime;

    public SkillContext(PlayerManager caster, Vector3 direction, Vector3 targetPosition)
    {
        this.caster = caster;
        this.direction = direction;
        this.targetPosition = targetPosition;
    }
}

[System.Serializable]
public abstract class SkillModuleBase
{
    public abstract UniTask Execute(SkillContext skillContext);
}

public abstract class SkillBase : ScriptableObject
{
    public string skillName;
    public float duration;
    public PlayerSkill skillType;

    public abstract UniTask Execute(SkillContext skillContext, CancellationToken token);
}

[CreateAssetMenu(menuName = "Skill/Modular Skill")]
public class ModularSkill : SkillBase
{
    [SerializeReference, SubclassSelector]
    public List<SkillModuleBase> modules = new();

    public override async UniTask Execute(SkillContext skillContext, CancellationToken token)
    {
        skillContext.caster.CurrentStateMachine.SetState(PlayerState.UsingSkill);
        skillContext.caster.CurrentSkill = skillType;

        foreach (var module in modules)
        {
            module.Execute(skillContext).Forget();
        }

        await UniTask.Delay((int)duration, cancellationToken: token);
        skillContext.caster.CurrentStateMachine.SetState(PlayerState.Idle);
        skillContext.caster.CurrentSkill = PlayerSkill.None;
    }
}