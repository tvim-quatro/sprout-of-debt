using System.IO;
using UnityEditor;
using UnityEngine;

namespace SproutOfDebt.UI.Editor
{
    // Shared paths and helpers for the UI prefab builders.
    internal static class UIPrefabUtils
    {
        internal const string ArtRoot = "Assets/UI/Art";
        internal const string PrefabsRoot = "Assets/UI/Prefabs";
        internal const string FontsRoot = "Assets/UI/Fonts";

        internal static Sprite LoadSprite(string relativePath)
        {
            string fullPath = Path.Combine(ArtRoot, relativePath).Replace("\\", "/");
            return AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
        }

        internal static void SavePrefab(GameObject go, string localPath)
        {
            string unityPath = localPath.Replace("\\", "/");
            PrefabUtility.SaveAsPrefabAsset(go, unityPath);
        }
    }
}
