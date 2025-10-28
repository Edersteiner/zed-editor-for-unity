using UnityEngine;
using UnityEditor;
using Microsoft.Unity.VisualStudio.Editor;

namespace UnityZed
{
    public class ZedPreferences
    {
        private readonly IGenerator m_Generator;

        public ZedPreferences(IGenerator generator)
        {
            m_Generator = generator;
        }

        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(GetType().Assembly);

            var style = new GUIStyle
            {
                richText = true,
                margin = new RectOffset(0, 4, 0, 0)
            };

            // PackageInfo.FindForAssembly may return null in some editor contexts (preferences window,
            // Unity domain reload state). Guard against null to avoid NullReferenceException.
            if (package != null)
                GUILayout.Label($"<size=10><color=grey>{package.displayName} v{package.version} enabled</color></size>", style);
            else
                GUILayout.Label("<size=10><color=grey>Zed Editor (package info unavailable)</color></size>", style);
            GUILayout.EndHorizontal();

            // If generator is not available yet (preferences shown before Initialize), show info and skip controls
            if (m_Generator == null)
            {
                EditorGUILayout.LabelField("Generate .csproj files for:");
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Project generation is not available until the code editor is initialized.", MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.LabelField("Generate .csproj files for:");
            EditorGUI.indentLevel++;
            SettingsButton(ProjectGenerationFlag.Embedded, "Embedded packages", "", m_Generator);
            SettingsButton(ProjectGenerationFlag.Local, "Local packages", "", m_Generator);
            SettingsButton(ProjectGenerationFlag.Registry, "Registry packages", "", m_Generator);
            SettingsButton(ProjectGenerationFlag.Git, "Git packages", "", m_Generator);
            SettingsButton(ProjectGenerationFlag.BuiltIn, "Built-in packages", "", m_Generator);
            SettingsButton(ProjectGenerationFlag.LocalTarBall, "Local tarball", "", m_Generator);
            SettingsButton(ProjectGenerationFlag.Unknown, "Packages from unknown sources", "", m_Generator);
            SettingsButton(ProjectGenerationFlag.PlayerAssemblies, "Player projects", "For each player project generate an additional csproj with the name 'project-player.csproj'", m_Generator);
            RegenerateProjectFiles(m_Generator);
            EditorGUI.indentLevel--;
        }

        private static void RegenerateProjectFiles(IGenerator generator)
        {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            rect.width = 252;
            if (GUI.Button(rect, "Regenerate project files"))
            {
                generator.Sync();
            }
        }

        private static void SettingsButton(ProjectGenerationFlag preference, string guiMessage, string toolTip, IGenerator generator)
        {
            var prevValue = generator.AssemblyNameProvider.ProjectGenerationFlag.HasFlag(preference);

            var newValue = EditorGUILayout.Toggle(new GUIContent(guiMessage, toolTip), prevValue);
            if (newValue != prevValue)
                generator.AssemblyNameProvider.ToggleProjectGeneration(preference);
        }
    }
}
