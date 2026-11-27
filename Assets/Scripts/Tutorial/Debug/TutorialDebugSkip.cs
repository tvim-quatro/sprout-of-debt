#if UNITY_EDITOR
using SproutOfDebt.Core;
using UnityEditor;
using UnityEngine;

namespace SproutOfDebt.Tutorial
{
    public static class TutorialDebugSkip
    {
        private const string PrefKey = "SproutOfDebt.SkipTutorial";
        private const string MenuPath = "Tools/Tutorial/Skip Tutorial In Editor";

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            EditorPrefs.SetBool(PrefKey, !EditorPrefs.GetBool(PrefKey, false));
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateToggle()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PrefKey, false));
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            if (!EditorPrefs.GetBool(PrefKey, false))
            {
                return;
            }

            var tutorial = GameServices.Progress.Tutorial;
            tutorial.IsCompleted = true;
            tutorial.CurrentStep = TutorialStep.Completed;
            Debug.Log("[Tutorial] Skipped by Tools > Tutorial > Skip Tutorial In Editor");
        }
    }
}
#endif
