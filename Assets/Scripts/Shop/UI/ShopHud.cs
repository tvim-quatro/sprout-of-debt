using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>Top bar: clicking the debt bar opens the Treasury, clicking the inventory button (or Tab/I) toggles Inventory.</summary>
    public class ShopHud : MonoBehaviour
    {
        [SerializeField] private WindowManager windowManager;
        [SerializeField] private Button debtBarButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button seedShopButton;
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
        }

        private void Update()
        {
            if (windowManager == null || inventoryWindow == null) return;

            if (Keyboard.current != null &&
                (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame))
            {
                windowManager.Toggle(inventoryWindow);
            }
        }
    }
}
