using UnityEngine;

namespace SproutOfDebt.Save
{
    /// <summary>Creates the save service once per play session, before the first scene loads, so every scene (the Main Menu too) can reach it.</summary>
    public static class SaveBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateService()
        {
            if (Object.FindAnyObjectByType<SaveService>() != null)
            {
                return;
            }

            GameObject host = new GameObject("[SaveService]");
            Object.DontDestroyOnLoad(host);
            host.AddComponent<SaveService>();
            host.AddComponent<PlayClock>();
        }
    }
}
