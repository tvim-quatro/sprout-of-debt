using SproutOfDebt.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SproutOfDebt.Save
{
    /// <summary>
    /// Counts the time the player spends in the game (the Main Menu does not count) into <see cref="Core.Save.GameProgress.PlaySeconds"/>.
    /// According to GDD, timers run only while the game is open: plant growth follows this clock, so it stands still
    /// while the game is closed and carries on from the same moment after Continue.
    /// </summary>
    public class PlayClock : MonoBehaviour
    {
        private const string MainMenuScene = "MainMenu";
        [SerializeField, Min(0f)] private float maxStepSeconds = 5f;

        private void Update()
        {
            if (SceneManager.GetActiveScene().name == MainMenuScene)
            {
                return;
            }
            // A long stall (the window was frozen or dragged) is not play time.
            GameServices.Progress.AddPlaySeconds(Mathf.Min(Time.unscaledDeltaTime, maxStepSeconds));
        }
    }
}
