using UnityEngine;
using UnityEngine.UI;

namespace DrugCrafter.Shop.UI
{
    /// <summary>Top bar: clicking the debt bar opens the Treasury, the seed button opens the Seed Shop.</summary>
    public class ShopHud : MonoBehaviour
    {
        [SerializeField] private WindowManager windowManager;
        [SerializeField] private Button debtBarButton;
        [SerializeField] private Button seedShopButton;
        [SerializeField] private UIWindow treasuryWindow;
        [SerializeField] private UIWindow seedShopWindow;

        private void Awake()
        {
            debtBarButton.onClick.AddListener(() => windowManager.Toggle(treasuryWindow));
            seedShopButton.onClick.AddListener(() => windowManager.Toggle(seedShopWindow));
        }
    }
}
