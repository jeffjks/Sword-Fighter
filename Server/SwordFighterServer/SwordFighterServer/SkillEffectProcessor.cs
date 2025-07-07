using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json.Linq;
using Shared.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace SwordFighterServer
{
    public delegate void SkillEffectHandler(Player caster, SkillInput skillInput, JToken effectToken);

    public static class SkillEffectProcessor
    {
        private static readonly Dictionary<SkillEffect, SkillEffectHandler> _handlers = new Dictionary<SkillEffect, SkillEffectHandler>();

        private const string SkillEffectTypeName = "SkillEffectType";

        static SkillEffectProcessor()
        {
            _handlers[SkillEffect.Block] = ApplyBlock;
            _handlers[SkillEffect.Damage] = ApplyDamage;
        }

        public static void Apply(Player caster, SkillInput skillInput, JToken effectToken)
        {
            var effectType = (SkillEffect) effectToken[SkillEffectTypeName].Value<int>();

            if (effectType == SkillEffect.None)
            {
                return;
            }

            if (_handlers.TryGetValue(effectType, out var handler))
            {
                try
                {
                    handler(caster, skillInput, effectToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                Console.WriteLine($"[Error] Unhandled Skill Effect: {effectType}");
            }
        }

        private static void ApplyBlock(Player caster, SkillInput skillInput, JToken effectToken)
        {
            caster.characterStatus.IsBlocking = true;
            caster.SetBlockState(true);
        }

        private static void ApplyDamage(Player caster, SkillInput skillInput, JToken effectToken)
        {
            const long Delay = 500;
            var targetTimestamp = skillInput.Timestamp + Delay;

            var damageCenterType = (DamageCenterType)effectToken["damageCenterType"].Value<int>();
            var damage = (int)effectToken["damage"];
            var radius = (float)effectToken["radius"];
            var angle = (float)effectToken["angle"];

            caster.AddSchedule(() => caster.PlayerAttack(targetTimestamp, damageCenterType, damage, radius, angle), Delay);
        }
    }
}
