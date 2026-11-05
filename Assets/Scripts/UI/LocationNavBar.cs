using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SproutOfDebt.UI
{
    /// <summary>
    /// Persistent navigation bar for moving between game locations (Garden, Laboratory, ShopCounter)
    /// and returning to the Main Menu.
    /// </summary>
    public class LocationNavBar : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button gardenButton;
        [SerializeField] private Button laboratoryButton;
        [SerializeField] private Button counterButton;
        [SerializeField] private Button menuButton;

        [Header("Scene Names")]
        [SerializeField] private string gardenSceneName = "Garden";
        [SerializeField] private string laboratorySceneName = "Laboratory";
        [SerializeField] private string counterSceneName = "ShopCounter";
        [SerializeField] private string menuSceneName = "MainMenu";

        public Button GardenButton => gardenButton;
        public Button LaboratoryButton => laboratoryButton;
        public Button CounterButton => counterButton;
        public Button MenuButton => menuButton;

        private void Start()
        {
            HighlightCurrentLocation();
        }

        private void HighlightCurrentLocation()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (gardenButton != null)
            {
                gardenButton.interactable = !string.Equals(currentScene, gardenSceneName, StringComparison.OrdinalIgnoreCase);
            }

            if (laboratoryButton != null)
            {
                laboratoryButton.interactable = !string.Equals(currentScene, laboratorySceneName, StringComparison.OrdinalIgnoreCase);
            }

            if (counterButton != null)
            {
                counterButton.interactable = !string.Equals(currentScene, counterSceneName, StringComparison.OrdinalIgnoreCase);
            }
        }

        public void OnGardenClicked()
        {
            LoadScene(gardenSceneName);
        }

        public void OnLaboratoryClicked()
        {
            LoadScene(laboratorySceneName);
        }

        public void OnCounterClicked()
        {
            LoadScene(counterSceneName);
        }

        public void OnMenuClicked()
        {
            LoadScene(menuSceneName);
        }

        private void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[LocationNavBar] Target scene name is empty.");
                return;
            }

            Debug.Log($"[LocationNavBar] Transitioning to scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }
}
