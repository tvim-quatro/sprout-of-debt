using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DrugCrafter.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Transition")]
        [Tooltip("Name of the gameplay scene to load when New Game or Continue is clicked")]
        [SerializeField] private string gameSceneName = "SampleScene";

        [Header("UI References (Optional)")]
        [SerializeField] private GameObject continueButton;
        [SerializeField] private GameObject settingsPanel;

        private void Start()
        {
            // According to GDD: If no save exists, hide Continue button
            if (continueButton != null)
            {
                bool hasSave = CheckHasSaveFile();
                continueButton.SetActive(hasSave);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void OnNewGameClicked()
        {
            Debug.Log("[MainMenu] New Game clicked! Loading " + gameSceneName);
            if (!string.IsNullOrEmpty(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }

        public void OnContinueClicked()
        {
            Debug.Log("[MainMenu] Continue clicked! Loading " + gameSceneName);
            if (!string.IsNullOrEmpty(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }

        public void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
            else
            {
                Debug.Log("[MainMenu] Settings clicked (Settings panel not assigned yet).");
            }
        }

        public void OnCloseSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void OnQuitClicked()
        {
            Debug.Log("[MainMenu] Quit clicked!");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private bool CheckHasSaveFile()
        {
            // Placeholder: Check if a save exists (PlayerPrefs or Save File)
            return PlayerPrefs.HasKey("HasSavedGame");
        }
    }
}
