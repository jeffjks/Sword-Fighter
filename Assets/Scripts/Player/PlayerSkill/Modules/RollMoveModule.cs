using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Enums;

[System.Serializable]
public class RollMoveModule : SkillModuleBase
{
    public float duration;
    public float rollDistance;

    public override SkillEffect SkillEffectType => SkillEffect.Roll;

    public override async UniTask Execute(SkillContext skillContext)
    {
        var caster = skillContext.caster;
        var direction = skillContext.direction;
        var start = caster.transform.position;
        var end = start + direction.normalized * rollDistance;
        caster.m_EnableInterpolate = false;

        float ctime = 0f;
        while (ctime < duration)
        {
            float t = (1f - Mathf.Cos(ctime * Mathf.PI / duration)) * 0.5f;
            caster.transform.position = Vector3.Lerp(start, end, t);
            ctime += Time.deltaTime;
            await UniTask.Yield(); // 프레임 기다리기
        }
        caster.m_EnableInterpolate = true;

        caster.transform.position = end;
    }
}