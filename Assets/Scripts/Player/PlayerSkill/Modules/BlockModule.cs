using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Enums;
using System.Threading;
using System;

[System.Serializable]
public class BlockModule : SkillModuleBase
{
    public int maxDuration;

    public override SkillEffect SkillEffectType => SkillEffect.Block;

    public async override UniTask Execute(long timestamp, SkillContext skillContext)
    {
        skillContext.caster.m_CharacterStatus.BlockState.StartTimer(maxDuration);

        await UniTask.CompletedTask;
    }
}