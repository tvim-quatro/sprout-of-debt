using UnityEditor;
using UnityEngine;

namespace SproutOfDebt.Save.Editor
{
    public static class SaveMenu
    {
        [MenuItem("Tools/Save/Delete Save File")]
        private static void DeleteSaveFile()
        {
            SaveFile.Delete();
            Debug.Log("[Save] Deleted " + SaveFile.Path);
        }

        [MenuItem("Tools/Save/Show Save File")]
        private static void ShowSaveFile()
        {
            if (SaveFile.Exists)
            {
                EditorUtility.RevealInFinder(SaveFile.Path);
            }
            else
            {
                Debug.Log("[Save] No save file at " + SaveFile.Path);
            }
        }
    }
}
