using System.Collections.Generic;
using UnityEditor;

namespace SproutOfDebt.Garden.Editor
{
    /// <summary>Keeps the plant catalog in sync with the plant assets in Data/Plants.</summary>
    public static class PlantCatalogBuilder
    {
        private const string PlantsFolder = "Assets/Data/Plants";
        private const string CatalogPath = PlantsFolder + "/PlantCatalog.asset";
        private const string SeedIconPath = "Assets/Art/Items/Seeds/icon_seed_";

        /// <summary>
        /// Loads (or creates) the catalog and fills it with every plant, fastest-growing first.
        /// A plant without a seed packet sprite gets its seed icon (icon_seed_&lt;id&gt;).
        /// </summary>
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
            foreach (PlantDefinition plant in plants)
            {
                AssignSeedSprite(plant);
            }

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

        private static void AssignSeedSprite(PlantDefinition plant)
        {
            if (plant.SeedSprite != null)
            {
                return;
            }
            var sprite = AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(SeedIconPath + plant.Id + ".png");
            if (sprite == null)
            {
                UnityEngine.Debug.LogWarning("[PlantCatalogBuilder] No seed icon for " + plant.Id);
                return;
            }
            var so = new SerializedObject(plant);
            so.FindProperty("seedSprite").objectReferenceValue = sprite;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(plant);
        }
    }
}
