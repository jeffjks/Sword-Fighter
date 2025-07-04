using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Enums;

[System.Serializable]
public class BlockModule : SkillModuleBase
{
    public int duration;

    public async override UniTask Execute(SkillContext skillContext)
    {
        skillContext.caster.m_CharacterStatus.IsBlocking = true;
        await UniTask.Delay(duration);
        skillContext.caster.m_CharacterStatus.IsBlocking = false;
    }
}