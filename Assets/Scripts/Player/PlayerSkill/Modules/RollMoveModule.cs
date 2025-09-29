using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Shared.Enums;

[System.Serializable]
public class RollMoveModule : SkillModuleBase
{
    public float duration;
    public float rollDistance;

    public override SkillEffect SkillEffectType => SkillEffect.Roll;

    public override async UniTask ExecuteModule(long timestamp, SkillContext skillContext, CancellationToken token)
    {
        var caster = skillContext.caster;
        var direction = skillContext.direction;
        var start = caster.transform.position;
        var end = start + direction.normalized * rollDistance;
        
        var deltaPos = direction.normalized * rollDistance;
        var clientInput = new ClientInput(timestamp, deltaPos);
        caster.m_RealPosition += deltaPos;
        PlayerMe.ClientInputQueue.Enqueue(clientInput);

        caster.m_EnableInterpolate = false;

        try {
            token.ThrowIfCancellationRequested();

            float ctime = 0f;
            while (ctime < duration)
            {
                float t = (1f - Mathf.Cos(ctime * Mathf.PI / duration)) * 0.5f;
                caster.transform.position = Vector3.Lerp(start, end, t);
                ctime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token); // 프레임 기다리기
            }
            caster.transform.position = end;
        }
        finally
        {
            caster.m_EnableInterpolate = true;
        }
    }
}