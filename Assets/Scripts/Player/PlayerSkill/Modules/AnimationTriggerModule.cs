using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Enums;

[System.Serializable]
public class AnimationTriggerModule : SkillModuleBase
{
    public PlayerSkill playerSkill;
    
    public override SkillEffect SkillEffectType => SkillEffect.None;

    public async override UniTask Execute(SkillContext skillContext)
    {
        skillContext.caster.SetSkillAnimation(playerSkill);
        await UniTask.CompletedTask;
    }
}