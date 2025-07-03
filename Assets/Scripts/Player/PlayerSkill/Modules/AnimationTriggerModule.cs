using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Enums;

[System.Serializable]
public class AnimationTriggerModule : SkillModuleBase
{
    public PlayerSkill playerSkill;

    private readonly int _animatorPlayerSkill = Animator.StringToHash("Skill");

    public async override UniTask Execute(SkillContext skillContext)
    {
        skillContext.caster.m_Animator.SetInteger(_animatorPlayerSkill, (int) playerSkill);

        await UniTask.CompletedTask;
    }
}