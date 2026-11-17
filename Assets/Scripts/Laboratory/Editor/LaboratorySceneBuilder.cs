using System.IO;
using SproutOfDebt.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SproutOfDebt.Laboratory.Editor
{
    /// <summary>
    /// Builds the Laboratory gameplay scene (Assets/Scenes/Laboratory.unity) from the
    /// Laboratory sandbox template, wiring up the camera, world props, brewing stations,
    /// UI canvases, and the shared HUD canvas.
    /// </summary>
    public static class LaboratorySceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Laboratory.unity";
        private const string TemplateScenePath = "Assets/Scenes/Sandbox/Lab_Sandbox.unity";
        private const string HudPrefabPath = "Assets/UI/Prefabs/Canvases/SOD_HUD_Canvas.prefab";
        private const string BuildHudMenu = "Tools/Shop/Build HUD Prefab";

        [MenuItem("Tools/Laboratory/Build Laboratory Scene")]
        public static void BuildScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (!File.Exists(TemplateScenePath))
            {
                Debug.LogError($"[LaboratorySceneBuilder] Template scene not found at: {TemplateScenePath}");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            File.Copy(TemplateScenePath, ScenePath, true);
            AssetDatabase.ImportAsset(ScenePath, ImportAssetOptions.ForceUpdate);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            EnsureCamera();
            EnsureHud();
            RebuildPopups();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            Debug.Log("[LaboratorySceneBuilder] Built Laboratory scene: " + ScenePath);
        }

        private static void EnsureCamera()
        {
            Camera cam = Object.FindAnyObjectByType<Camera>();
            if (cam != null && !cam.TryGetComponent(out FixedAspectCamera _))
            {
                cam.gameObject.AddComponent<FixedAspectCamera>();
            }
        }

        private static void EnsureHud()
        {
            GameObject oldHud = GameObject.Find("SOD_HUD_Canvas");
            if (oldHud != null)
            {
                Object.DestroyImmediate(oldHud);
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath) == null)
            {
                EditorApplication.ExecuteMenuItem(BuildHudMenu);
            }

            GameObject hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            if (hudPrefab != null)
            {
                PrefabUtility.InstantiatePrefab(hudPrefab);
            }
            else
            {
                Debug.LogWarning("[LaboratorySceneBuilder] HUD prefab not found at " + HudPrefabPath);
            }
        }

        private static void RebuildPopups()
        {
            RecipeBookBuilder.Build();
            BrewingGuideBuilder.Build();
            LabFeedbackBuilder.Build();
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == path)
                {
                    return;
                }
            }

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes);
            list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
