using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Shared.Enums;

[System.Serializable]
public class AnimationTriggerModule : SkillModuleBase
{
    public PlayerSkill playerSkill;
    
    public override SkillEffect SkillEffectType => SkillEffect.None;

    public async override UniTask ExecuteModule(long timestamp, SkillContext skillContext, CancellationToken token)
    {
        skillContext.caster.SetSkillAnimation(playerSkill);
        await UniTask.CompletedTask;
    }
}