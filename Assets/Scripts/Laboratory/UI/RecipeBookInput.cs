using UnityEngine;
using UnityEngine.InputSystem;

namespace SproutOfDebt.Laboratory.UI
{
    public class RecipeBookInput : MonoBehaviour
    {
        [SerializeField] private RecipeBookWindow window;
        [SerializeField] private Collider2D bookCollider;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (window == null)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                window.Toggle();
            }
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                window.Close();
            }
            if (!window.IsOpen && WasBookClicked())
            {
                window.Open();
            }
        }

        private bool WasBookClicked()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || mainCamera == null || bookCollider == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return false;
            }
            Vector2 point = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            return bookCollider.OverlapPoint(point);
        }
    }
}
