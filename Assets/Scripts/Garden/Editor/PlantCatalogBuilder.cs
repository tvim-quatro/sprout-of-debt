using System.Collections.Generic;
using UnityEditor;

namespace SproutOfDebt.Garden.Editor
{
    /// <summary>Keeps the plant catalog in sync with the plant assets in Data/Plants.</summary>
    public static class PlantCatalogBuilder
    {
        private const string PlantsFolder = "Assets/Data/Plants";
        private const string CatalogPath = PlantsFolder + "/PlantCatalog.asset";

        /// <summary>Loads (or creates) the catalog and fills it with every plant, fastest-growing first.</summary>
        public static PlantCatalog Build()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PlantCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = UnityEngine.ScriptableObject.CreateInstance<PlantCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var plants = new List<PlantDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(PlantDefinition), new[] { PlantsFolder }))
            {
                plants.Add(AssetDatabase.LoadAssetAtPath<PlantDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
            }
            plants.Sort((a, b) => a.GrowSeconds.CompareTo(b.GrowSeconds));

            var so = new SerializedObject(catalog);
            var list = so.FindProperty("plants");
            list.arraySize = plants.Count;
            for (int i = 0; i < plants.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = plants[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return catalog;
        }
    }
}
