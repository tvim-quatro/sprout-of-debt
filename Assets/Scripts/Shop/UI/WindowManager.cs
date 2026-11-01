using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DrugCrafter.Shop.UI
{
    /// <summary>
    /// Shows one window at a time on top of the current location behind a dimmer.
    /// Esc or a click on the dimmer closes it and returns the player to the same location.
    /// </summary>
    public class WindowManager : MonoBehaviour
    {
        [SerializeField] private Button dimmer;

        public UIWindow Current { get; private set; }

        private void Awake()
        {
            dimmer.onClick.AddListener(CloseCurrent);
            dimmer.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Current != null && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseCurrent();
            }
        }

        public void Open(UIWindow window)
        {
            if (Current == window) return;
            CloseCurrent();
            Current = window;
            dimmer.gameObject.SetActive(true);
            window.gameObject.SetActive(true);
        }

        public void Toggle(UIWindow window)
        {
            if (Current == window) CloseCurrent();
            else Open(window);
        }

        public void CloseCurrent()
        {
            if (Current == null) return;
            Current.gameObject.SetActive(false);
            Current = null;
            dimmer.gameObject.SetActive(false);
        }
    }
}
