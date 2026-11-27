using System.Collections.Generic;
using SproutOfDebt.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SproutOfDebt.Laboratory
{
    public class LaboratoryDragController : MonoBehaviour
    {
        private readonly List<Collider2D> hits = new List<Collider2D>();

        private Camera mainCamera;
        private ContactFilter2D filter;
        private LaboratoryTool heldTool;
        private CursorState cursorState;

        private void Awake()
        {
            mainCamera = Camera.main;
            filter = new ContactFilter2D().NoFilter();
        }

        private void OnDisable()
        {
            Drop();
            SetCursorState(CursorState.Default);
        }

        public void Hold(LaboratoryTool tool)
        {
            Drop();
            heldTool = tool;
            heldTool.PickUp();
            Mouse mouse = Mouse.current;
            if (mouse != null && mainCamera != null)
            {
                heldTool.MoveTo(mainCamera.ScreenToWorldPoint(mouse.position.ReadValue()));
            }
            SetCursorState(CursorState.Grab);
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || mainCamera == null)
            {
                return;
            }

            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());

            if (mouse.leftButton.wasPressedThisFrame)
            {
                PickUpAt(mousePosition);
            }
            if (heldTool == null)
            {
                SetCursorState(FindPickableTool(mousePosition) != null ? CursorState.Hover : CursorState.Default);
                return;
            }
            if (!mouse.leftButton.isPressed)
            {
                Drop();
                return;
            }

            SetCursorState(CursorState.Grab);
            heldTool.MoveTo(mousePosition);
            UseOnTargets();
        }

        private void PickUpAt(Vector2 point)
        {
            LaboratoryTool tool = FindPickableTool(point);
            if (tool != null)
            {
                heldTool = tool;
                heldTool.PickUp();
            }
        }

        private LaboratoryTool FindPickableTool(Vector2 point)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return null;
            }

            Physics2D.OverlapPoint(point, filter, hits);
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out LaboratoryTool tool) && tool.CanPickUp)
                {
                    return tool;
                }
            }
            return null;
        }

        private void UseOnTargets()
        {
            Physics2D.OverlapPoint(heldTool.GetUsePoint(), filter, hits);
            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject != heldTool.gameObject && heldTool.UseOn(hit, Time.deltaTime))
                {
                    Drop();
                    return;
                }
            }
        }

        private void Drop()
        {
            if (heldTool == null)
            {
                return;
            }
            heldTool.ReturnHome();
            heldTool = null;
            SetCursorState(CursorState.Default);
        }

        // One game cursor for the whole game (UI/GameCursorView); the Laboratory only says which look it needs.
        private void SetCursorState(CursorState state)
        {
            if (state == cursorState)
            {
                return;
            }
            cursorState = state;
            GameServices.GameCursor?.Show(state);
        }
    }
}
