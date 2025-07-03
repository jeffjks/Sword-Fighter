using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared.Enums;

public class SkillRegistry : MonoBehaviour
{
    public SkillMap m_SkillMap;

    public static Dictionary<PlayerSkill, SkillBase> SkillMap = new();

    private void Awake()
    {
        foreach (var item in m_SkillMap.entries)
        {
            SkillMap.Add(item.playerSkill, item.skillBase);
        }
    }
}
