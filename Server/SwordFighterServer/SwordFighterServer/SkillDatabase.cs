using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Shared.Enums;
using System.IO;

namespace SwordFighterServer
{
    public struct SkillData
    {
        public string skillName;
        public float duration;
        public float cooldown;
    }

    public class SkillDatabase
    {
        private static JObject _skillData;

        private const string JsonPath = @"C:\Program Files\Unity\Project\Sword-Fighter\Assets\ExportedSkill.json";
        private const string SkillName = "skillName";
        private const string Duration = "duration";
        private const string Cooldown = "cooldown";
        private const string ModuleName = "modules";

        public SkillDatabase()
        {
            string jsonText = File.ReadAllText(JsonPath);
            _skillData = JObject.Parse(jsonText);
        }

        public static void ApplySkillEffect(Player caster, SkillInput skillInput)
        {
            var skill = _skillData[skillInput.playerSkill.ToString()];
            var modules = (JArray) skill[ModuleName];

            foreach (var skillEffectModule in modules)
            {
                SkillEffectProcessor.Apply(caster, skillInput, skillEffectModule);
            }
        }

        public static SkillData GetSkillData(PlayerSkill playerSkill)
        {
            var skill = _skillData[playerSkill.ToString()];
            var skillName = (string)skill[SkillName];
            var duration = (float)skill[Duration];
            var cooldown = (float)skill[Cooldown];

            SkillData skillData = new SkillData { skillName = skillName, duration = duration, cooldown = cooldown };

            return skillData;
        }
    }
}
