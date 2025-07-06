using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Enums;

[System.Serializable]
public class DamageModule : SkillModuleBase
{
    public DamageCenterType damageCenterType;
    public int damage;
    public float radius;
    public float angle;

    public override SkillEffect SkillEffectType => SkillEffect.Damage;

    public async override UniTask Execute(SkillContext skillContext)
    {
        await UniTask.CompletedTask;
    }
}