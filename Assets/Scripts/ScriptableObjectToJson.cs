using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Shared.Enums;

public class ScriptableObjectToJson : MonoBehaviour
{
    public SkillMap m_SkillMap;

    private readonly Dictionary<PlayerSkill, ModularSkill> _skillData = new();

    [ContextMenu("Export To JSON")]
    public void Export()
    {
        foreach (var item in m_SkillMap.entries)
        {
            // item.modularSkill.modules.RemoveAll(m => m.SkillEffectType == SkillEffect.None);
            _skillData.Add(item.playerSkill, item.modularSkill);
        }

        var json = JsonConvert.SerializeObject(_skillData,
            Formatting.Indented,
            new JsonSerializerSettings
            {
                ContractResolver = new IgnoreUnityObjectFieldsResolver(),
                TypeNameHandling = TypeNameHandling.None, // SerializeReference 용도
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                PreserveReferencesHandling = PreserveReferencesHandling.None
            });

        var path = Path.Combine(Application.dataPath, "ExportedSkill.json");
        File.WriteAllText(path, json);
        Debug.Log("Skill exported to " + path);
    }
}

public class IgnoreUnityObjectFieldsResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
    {
        var props = base.CreateProperties(type, memberSerialization);

        var ignored = new HashSet<string> { "name", "hideFlags" };

        return props.Where(p => !ignored.Contains(p.PropertyName)).ToList();
    }
}