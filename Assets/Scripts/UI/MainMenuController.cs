using SproutOfDebt.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SproutOfDebt.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Transition")]
        [Tooltip("Name of the gameplay scene to load when New Game or Continue is clicked")]
        [SerializeField] private string gameSceneName = "ShopCounter";

        [Header("UI References (Optional)")]
        [SerializeField] private GameObject continueButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject quitPanel;
        [SerializeField] private GameObject newGamePanel;

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

            if (quitPanel != null)
            {
                quitPanel.SetActive(false);
            }

            if (newGamePanel != null)
            {
                newGamePanel.SetActive(false);
            }
        }

        public void OnNewGameClicked()
        {
            // According to GDD: with a save, New Game asks to confirm that all progress will be lost; without one it starts right away.
            if (newGamePanel != null && CheckHasSaveFile())
            {
                newGamePanel.SetActive(true);
                return;
            }
            OnConfirmNewGameClicked();
        }

        public void OnConfirmNewGameClicked()
        {
            Debug.Log("[MainMenu] New Game! Loading " + gameSceneName);
            GameServices.Save?.StartNewGame();
            if (!string.IsNullOrEmpty(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }

        public void OnCancelNewGameClicked()
        {
            if (newGamePanel != null)
            {
                newGamePanel.SetActive(false);
            }
        }

        public void OnContinueClicked()
        {
            string location = null;
            if (GameServices.Save == null || !GameServices.Save.TryLoad(out location))
            {
                Debug.LogWarning("[MainMenu] Continue clicked but there is no usable save.");
                if (continueButton != null)
                {
                    continueButton.SetActive(false);
                }
                return;
            }

            string scene = string.IsNullOrEmpty(location) ? gameSceneName : location;
            Debug.Log("[MainMenu] Continue! Loading " + scene);
            SceneManager.LoadScene(scene);
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
            if (quitPanel != null)
            {
                quitPanel.SetActive(true);
                return;
            }
            OnConfirmQuitClicked();
        }

        public void OnCancelQuitClicked()
        {
            if (quitPanel != null)
            {
                quitPanel.SetActive(false);
            }
        }

        public void OnConfirmQuitClicked()
        {
            Debug.Log("[MainMenu] Quit confirmed!");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private bool CheckHasSaveFile()
        {
            return GameServices.Save != null && GameServices.Save.HasSave;
        }
    }
}
