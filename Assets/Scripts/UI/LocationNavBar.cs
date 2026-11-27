using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

        [Header("Active State")]
        [Tooltip("Sprite shown on the button of the location the player is in")]
        [SerializeField] private Sprite activeSprite;
        [Tooltip("Label color on the active button")]
        [SerializeField] private Color activeLabelColor = new Color(0.24f, 0.15f, 0.09f);

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

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (SproutOfDebt.Core.GameServices.Modals != null && SproutOfDebt.Core.GameServices.Modals.IsOpen) return;

            if (keyboard.digit1Key.wasPressedThisFrame && gardenButton != null && gardenButton.interactable)
            {
                OnGardenClicked();
            }
            else if (keyboard.digit2Key.wasPressedThisFrame && laboratoryButton != null && laboratoryButton.interactable)
            {
                OnLaboratoryClicked();
            }
            else if (keyboard.digit3Key.wasPressedThisFrame && counterButton != null && counterButton.interactable)
            {
                OnCounterClicked();
            }
        }

        private void HighlightCurrentLocation()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            MarkActiveIfCurrent(gardenButton, gardenSceneName, currentScene);
            MarkActiveIfCurrent(laboratoryButton, laboratorySceneName, currentScene);
            MarkActiveIfCurrent(counterButton, counterSceneName, currentScene);
        }

        // The current location's button can't be clicked and shows the active (highlighted) look.
        // The button's own disabled sprite stays reserved for locations that are still locked.
        private void MarkActiveIfCurrent(Button button, string sceneName, string currentScene)
        {
            if (button == null || !string.Equals(currentScene, sceneName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Button_Nav uses its active sprite as the pressed sprite, so that is the fallback.
            SpriteState state = button.spriteState;
            state.disabledSprite = activeSprite != null ? activeSprite : state.pressedSprite;
            button.spriteState = state;

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.color = activeLabelColor;
            }

            button.interactable = false;
        }

        /// <summary>
        /// Locks or unlocks a location button. Figma "Nav button", locked: the sprite shows the lock,
        /// icon and name are hidden. The current location's button is left alone (it shows the active look).
        /// </summary>
        public void SetLocked(Button button, bool locked)
        {
            if (button == null || button.interactable == !locked || IsCurrentLocation(button)) return;

            button.interactable = !locked;
            foreach (Transform child in button.transform)
            {
                child.gameObject.SetActive(!locked);
            }
        }

        private bool IsCurrentLocation(Button button)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            string sceneName = button == gardenButton ? gardenSceneName
                : button == laboratoryButton ? laboratorySceneName
                : button == counterButton ? counterSceneName : null;
            return sceneName != null && string.Equals(currentScene, sceneName, StringComparison.OrdinalIgnoreCase);
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
