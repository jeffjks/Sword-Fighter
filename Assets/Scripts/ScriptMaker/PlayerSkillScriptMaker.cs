#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using Shared.Enums;

namespace Editors
{
    public class PlayerSkillScriptMaker : EditorWindow
    {
        private const string scriptPath = "Assets/Scripts/Player/PlayerSkill/";

        private Vector2 _scrollPos;

        [MenuItem("Tools/Enums/Player Skill Script Maker")]
        public static void ShowWindow()
        {
            GetWindow<PlayerSkillScriptMaker>("Player Skill Script Maker");
        }

        void OnGUI()
        {
            _scrollPos =
            EditorGUILayout.BeginScrollView(_scrollPos);
            ScriptMakerGUI();
            EditorGUILayout.EndScrollView();
        }

        private void ScriptMakerGUI()
        {
            GuiBuild();
        }

        private void GuiBuild()
        {
            GUILayout.Label("Build", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Build"))
            {
                Build();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                CompilationPipeline.RequestScriptCompilation();
                AssetDatabase.Refresh();
                Debug.Log("Build Done.");
            }
        }

        private void Build()
        {
            Directory.CreateDirectory(scriptPath);

            WritePlayerSkillCodes();
            WritePlayerSkillInitializerCode();
        }
        
        private void WritePlayerSkillCodes()
        {
            foreach (PlayerSkill skill in Enum.GetValues(typeof(PlayerSkill)))
            {
                var skillName = skill.ToString();
                var path = scriptPath + skillName + "Skill.cs";
                if (File.Exists(path) == false)
                {
                    using (var writer = new StreamWriter(path))
                    {
                        writer.WriteLine($"// This is Auto Generated Code by{this}");
                        writer.WriteLine("using Shared.Enums;");
                        writer.WriteLine("");
                        WritePlayerSkillClass(writer, skillName);
                        writer.Close();
                    }
                    Debug.Log($"{path} Create Complete");
                }
            }
        }

        private void WritePlayerSkillClass(StreamWriter writer, string enumName)
        {
            writer.WriteLine($"public class {enumName}Skill : PlayerSkillBase");
            writer.WriteLine("{");
            writer.WriteLine($"    public {enumName}Skill(PlayerManager manager) : base(manager) {{ }}");
            writer.WriteLine($"    public override PlayerSkill Type => PlayerSkill.{enumName};");
            writer.WriteLine("    public override void Enter() { }");
            writer.WriteLine("    public override void Update() { }");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        private void WritePlayerSkillInitializerCode()
        {
            var path = scriptPath + "PlayerSkillInitializer.cs";
            using (var writer = new StreamWriter(path))
            {
                WritePlayerSkillInitializerClass(writer);
            }
            Debug.Log($"{path} Create Complete");
        }

        private void WritePlayerSkillInitializerClass(StreamWriter writer)
        {
            writer.WriteLine($"// This is Auto Generated Code by{this}. Do not modify the code.");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using UnityEngine;");
            writer.WriteLine("using Shared.Enums;");
            writer.WriteLine("");
            writer.WriteLine("public class PlayerSkillInitializer : MonoBehaviour");
            writer.WriteLine("{");
            writer.WriteLine("    public static Dictionary<PlayerSkill, PlayerSkillBase> GetPlayerSkillDictionary(PlayerManager manager)");
            writer.WriteLine("    {");
            writer.WriteLine("        var playerSkills = new Dictionary<PlayerSkill, PlayerSkillBase>");
            writer.WriteLine("        {");
            foreach (PlayerSkill skill in Enum.GetValues(typeof(PlayerSkill)))
            {
                var skillName = skill.ToString();
                writer.WriteLine($"            {{ PlayerSkill.{skillName}, new {skillName}Skill(manager) }},");
            }
            writer.WriteLine("        };");
            writer.WriteLine("");
            writer.WriteLine("        return playerSkills;");
            writer.WriteLine("    }");
            writer.WriteLine("}");
            writer.WriteLine("");
            writer.Close();
        }
    }
}
#endif