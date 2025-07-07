using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Enums;
using System.IO;

namespace SwordFighterServer
{
    public class SkillDatabase
    {
        private static JObject _skillData;

        private const string JsonPath = @"C:\Program Files\Unity\Project\Sword-Fighter\Assets\ExportedSkill.json";
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
    }
}
