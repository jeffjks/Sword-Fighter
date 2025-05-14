#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using Shared.Enums;

namespace Editors
{
    public class PlayerStateScriptMaker : EditorWindow
    {
        private const string scriptPath = "Assets/Scripts/Player/PlayerState/";

        private Vector2 _scrollPos;

        [MenuItem("Tools/Enums/Player State Script Maker")]
        public static void ShowWindow()
        {
            GetWindow<PlayerStateScriptMaker>("Player State Script Maker");
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

            WritePlayerStateCodes();
            WritePlayerStateInitializerCode();
        }
        
        private void WritePlayerStateCodes()
        {
            foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
            {
                var stateName = state.ToString();
                var path = scriptPath + stateName + "State.cs";
                if (File.Exists(path) == false)
                {
                    using (var writer = new StreamWriter(path))
                    {
                        writer.WriteLine($"// This is Auto Generated Code by{this}");
                        writer.WriteLine("using Shared.Enums;");
                        writer.WriteLine("");
                        WritePlayerStateClass(writer, stateName);
                        writer.Close();
                    }
                    Debug.Log($"{path}  Create Complete");
                }
            }
        }

        private void WritePlayerStateClass(StreamWriter writer, string enumName)
        {
            writer.WriteLine($"public class {enumName}State : PlayerStateBase");
            writer.WriteLine("{");
            writer.WriteLine($"    public {enumName}State(PlayerManager manager) : base(manager) {{ }}");
            writer.WriteLine($"    public override PlayerState Type => PlayerState.{enumName};");
            writer.WriteLine("    public override void Enter() { }");
            writer.WriteLine("    public override void Update() { }");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        private void WritePlayerStateInitializerCode()
        {
            var path = scriptPath + "PlayerStateInitializer.cs";
            using (var writer = new StreamWriter(path))
            {
                WritePlayerStateInitializerClass(writer);
            }
            Debug.Log($"{path}  Create Complete");
        }

        private void WritePlayerStateInitializerClass(StreamWriter writer)
        {
            writer.WriteLine($"// This is Auto Generated Code by{this}. Do not modify the code.");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using UnityEngine;");
            writer.WriteLine("using Shared.Enums;");
            writer.WriteLine("");
            writer.WriteLine("public class PlayerStateInitializer : MonoBehaviour");
            writer.WriteLine("{");
            writer.WriteLine("    public static Dictionary<PlayerState, PlayerStateBase> GetPlayerStateDictionary(PlayerManager manager)");
            writer.WriteLine("    {");
            writer.WriteLine("        var playerStates = new Dictionary<PlayerState, PlayerStateBase>");
            writer.WriteLine("        {");
            foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
            {
                var stateName = state.ToString();
                writer.WriteLine($"            {{ PlayerState.{stateName}, new {stateName}State(manager) }},");
            }
            writer.WriteLine("        };");
            writer.WriteLine("");
            writer.WriteLine("        return playerStates;");
            writer.WriteLine("    }");
            writer.WriteLine("}");
            writer.WriteLine("");
            writer.Close();
        }
    }
}
#endif