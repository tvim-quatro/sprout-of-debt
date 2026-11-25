using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>Window opened through <see cref="WindowManager"/>. Refreshes itself in OnEnable.</summary>
    public class UIWindow : MonoBehaviour
    {
        [SerializeField] protected WindowManager windowManager;
        [SerializeField] private Button closeButton;

        public bool IsOpen => gameObject.activeInHierarchy;

        protected virtual void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        public virtual void Close()
        {
            if (windowManager != null)
            {
                windowManager.CloseCurrent();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
