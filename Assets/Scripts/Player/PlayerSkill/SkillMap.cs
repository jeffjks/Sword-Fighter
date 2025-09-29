using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared.Enums;

[System.Serializable]
public class SkillEntry
{
    public PlayerSkill playerSkill;
    public ModularSkill modularSkill;
}

[CreateAssetMenu(menuName = "Skill/Skill Map")]
public class SkillMap : ScriptableObject
{
    public List<SkillEntry> entries = new();
}