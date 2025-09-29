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
    public abstract SkillEffect SkillEffectType { get; }
    public abstract UniTask ExecuteModule(long timestamp, SkillContext skillContext, CancellationToken token);
}

[CreateAssetMenu(menuName = "Skill/Modular Skill")]
public class ModularSkill : ScriptableObject
{
    [SerializeField] protected string skillName;
    [SerializeField] protected float duration;
    [SerializeField] protected float cooldown;
    [SerializeField] protected PlayerSkill skillType;
    
    [SerializeReference, SubclassSelector]
    [SerializeField] protected List<SkillModuleBase> modules = new();

    public async UniTask Execute(long timestamp, SkillContext skillContext, CancellationToken token)
    {
        foreach (var module in modules)
        {
            module.ExecuteModule(timestamp, skillContext, token).Forget();
        }

        if (duration > 0f)
            await UniTask.Delay((int)(duration * 1000f), cancellationToken: token);
    }
}