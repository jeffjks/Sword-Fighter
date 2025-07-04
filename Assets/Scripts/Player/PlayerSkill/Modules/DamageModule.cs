using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Enums;

[System.Serializable]
public class DamageModule : SkillModuleBase
{
    public int damageAmount;
    public float radius;

    public async override UniTask Execute(SkillContext skillContext)
    {
        /*
        var enemies = Physics.OverlapSphere(skillContext.caster.transform.position, radius);
        foreach (var enemy in enemies)
        {
            if (enemy.TryGetComponent(out IDamageable dmg))
                dmg.TakeDamage(damageAmount);
        }
        */
        await UniTask.CompletedTask;
    }
}