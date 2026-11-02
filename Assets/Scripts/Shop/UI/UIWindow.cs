using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>Window opened through <see cref="WindowManager"/>. Refreshes itself in OnEnable.</summary>
    public class UIWindow : MonoBehaviour
    {
        [SerializeField] protected WindowManager windowManager;
        [SerializeField] private Button closeButton;

        protected virtual void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(() => windowManager.CloseCurrent());
        }
    }
}
