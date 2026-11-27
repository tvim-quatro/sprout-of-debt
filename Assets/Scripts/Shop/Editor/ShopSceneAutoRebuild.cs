using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SproutOfDebt.Shop.Editor
{
    /// <summary>
    /// After scripts reload, rebuilds the Shop Counter scene once if it was built before the Figma Counter layout
    /// (no Order panel wired to the controller). Skipped while playing or when an open scene has unsaved changes;
    /// the scenes that were open are reopened afterwards.
    /// </summary>
    [InitializeOnLoad]
    internal static class ShopSceneAutoRebuild
    {
        private const string LayoutMarker = "orderPanel: {fileID: ";
        private const string EmptyMarker = "orderPanel: {fileID: 0}";

        static ShopSceneAutoRebuild()
        {
            EditorApplication.delayCall += RebuildIfOutdated;
        }

        private static bool IsOutdated()
        {
            string path = ShopSceneBuilder.ScenePath;
            if (!File.Exists(path)) return false;
            string text = File.ReadAllText(path);
            return !text.Contains(LayoutMarker) || text.Contains(EmptyMarker);
        }

        private static void RebuildIfOutdated()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (!IsOutdated()) return;

            var openPaths = new List<string>();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    Debug.LogWarning("[ShopSceneAutoRebuild] The Shop Counter scene uses the old layout, but '" + scene.name +
                                     "' has unsaved changes. Save it and run Tools/Shop/Build Shop Counter Scene.");
                    return;
                }
                if (!string.IsNullOrEmpty(scene.path)) openPaths.Add(scene.path);
            }

            Debug.Log("[ShopSceneAutoRebuild] Rebuilding the Shop Counter scene with the Figma Counter layout.");
            ShopSceneBuilder.BuildScene();

            if (openPaths.Count == 0 || (openPaths.Count == 1 && openPaths[0] == ShopSceneBuilder.ScenePath)) return;
            EditorSceneManager.OpenScene(openPaths[0], OpenSceneMode.Single);
            for (int i = 1; i < openPaths.Count; i++)
            {
                EditorSceneManager.OpenScene(openPaths[i], OpenSceneMode.Additive);
            }
        }
    }
}
