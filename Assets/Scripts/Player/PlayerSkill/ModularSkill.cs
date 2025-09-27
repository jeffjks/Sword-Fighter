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
    [HideInInspector]
    public abstract SkillEffect SkillEffectType { get; }
    public abstract UniTask Execute(long timestamp, SkillContext skillContext);
}

public abstract class SkillBase : ScriptableObject
{
    public string skillName;
    public float duration;
    public float cooldown;
    public PlayerSkill skillType;
    
    [SerializeReference, SubclassSelector]
    public List<SkillModuleBase> modules = new();

    public abstract UniTask Execute(long timestamp, SkillContext skillContext, CancellationToken token);
}

[CreateAssetMenu(menuName = "Skill/Modular Skill")]
public class ModularSkill : SkillBase
{
    public override async UniTask Execute(long timestamp, SkillContext skillContext, CancellationToken token)
    {
        skillContext.caster.CurrentStateMachine.SetState(PlayerState.UsingSkill);
        skillContext.caster.CurrentSkill = skillType;

        foreach (var module in modules)
        {
            module.Execute(timestamp, skillContext).Forget();
        }

        if (duration > 0f)
            await UniTask.Delay((int) (duration*1000f), cancellationToken: token);
        skillContext.caster.CurrentStateMachine.SetState(PlayerState.Idle);
        skillContext.caster.SetSkillAnimation(PlayerSkill.None);
        skillContext.caster.CurrentSkill = PlayerSkill.None;
    }
}