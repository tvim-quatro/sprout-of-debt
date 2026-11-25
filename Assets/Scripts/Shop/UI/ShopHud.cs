using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Top bar: clicking the debt bar opens the Treasury, clicking the inventory button (or Tab/I) toggles Inventory,
    /// and clicking the exit button (or Esc when no window is open) returns straight to MainMenu (Figma note 424:6777).
    /// </summary>
    public class ShopHud : MonoBehaviour
    {
        [SerializeField] private WindowManager windowManager;
        [SerializeField] private Button debtBarButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button seedShopButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private UIWindow treasuryWindow;
        [SerializeField] private UIWindow seedShopWindow;
        [SerializeField] private UIWindow inventoryWindow;

        private void Awake()
        {
            if (debtBarButton != null)
                debtBarButton.onClick.AddListener(() => windowManager.Toggle(treasuryWindow));
            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(() => windowManager.Toggle(inventoryWindow));
            if (seedShopButton != null)
                seedShopButton.onClick.AddListener(() => windowManager.Toggle(seedShopWindow));
            if (exitButton != null)
                exitButton.onClick.AddListener(ReturnToMainMenu);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (windowManager != null && inventoryWindow != null &&
                (keyboard.tabKey.wasPressedThisFrame || keyboard.iKey.wasPressedThisFrame))
            {
                windowManager.Toggle(inventoryWindow);
            }
            else if (keyboard.escapeKey.wasPressedThisFrame && (windowManager == null || !windowManager.HasOpenWindow))
            {
                ReturnToMainMenu();
            }
        }

        private void ReturnToMainMenu()
        {
            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
