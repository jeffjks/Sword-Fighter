// This is Auto Generated Code by (Editors.PlayerSkillScriptMaker). Do not modify the code.
using System.Collections.Generic;
using UnityEngine;
using Shared.Enums;

public class PlayerSkillInitializer : MonoBehaviour
{
    public static Dictionary<PlayerSkill, PlayerSkillBase> GetPlayerSkillDictionary(PlayerManager manager)
    {
        var playerSkills = new Dictionary<PlayerSkill, PlayerSkillBase>
        {
            { PlayerSkill.None, new NoneSkill(manager) },
            { PlayerSkill.Block, new BlockSkill(manager) },
            { PlayerSkill.Basic, new BasicSkill(manager) },
            { PlayerSkill.Roll, new RollSkill(manager) },
        };

        return playerSkills;
    }
}

