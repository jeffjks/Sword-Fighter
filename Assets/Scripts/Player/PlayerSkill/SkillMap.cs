using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared.Enums;

[System.Serializable]
public class SkillEntry
{
    public PlayerSkill playerSkill;
    public SkillBase skillBase;
}

[CreateAssetMenu(menuName = "Skill/Skill Map")]
public class SkillMap : ScriptableObject
{
    [SerializeReference, SubclassSelector]
    public List<SkillEntry> entries = new();
}