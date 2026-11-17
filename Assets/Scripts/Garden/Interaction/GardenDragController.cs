using System.Collections.Generic;
using SproutOfDebt.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// Hold LMB on a garden tool to pick it up, move it over the cells to use it, release to put it back.
    /// One drag can work several cells in a row.
    /// Figma cursors: open hand over a tool that can be picked up, closed hand while dragging, the themed arrow otherwise.
    /// Over the interface the UI under the cursor chooses the look (an inventory slot shows the open hand).
    /// </summary>
    public class GardenDragController : MonoBehaviour
    {
        private readonly List<Collider2D> hits = new List<Collider2D>();

        private Camera mainCamera;
        private ContactFilter2D filter;
        private GardenTool heldTool;
        private bool pointerOverUi;

        private void Awake()
        {
            mainCamera = Camera.main;
            filter = ContactFilter2D.noFilter;
        }

        private void OnDisable()
        {
            Drop();
            ShowCursor(CursorState.Default);
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || mainCamera == null)
            {
                return;
            }

            Vector2 cursor = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            if (heldTool == null)
            {
                if (IsPointerOverUi())
                {
                    return;
                }
                GardenTool tool = Find<GardenTool>(cursor);
                tool = tool != null && tool.CanPickUp ? tool : null;
                if (tool != null && mouse.leftButton.wasPressedThisFrame)
                {
                    heldTool = tool;
                    heldTool.PickUp();
                }
                else
                {
                    ShowCursor(tool != null ? CursorState.Hover : CursorState.Default);
                    return;
                }
            }
            if (!mouse.leftButton.isPressed)
            {
                Drop();
                return;
            }

            ShowCursor(CursorState.Grab);
            heldTool.MoveTo(cursor);
            UseOnPlotUnder(heldTool.WorkPoint);
        }

        /// <summary>
        /// Puts a tool into the hand while the mouse button is held (the seed packet dragged out of the inventory).
        /// The drag then goes on as if the player had picked the tool up in the scene.
        /// </summary>
        public void Hold(GardenTool tool)
        {
            Drop();
            heldTool = tool;
            heldTool.PickUp();
            Mouse mouse = Mouse.current;
            if (mouse != null && mainCamera != null)
            {
                heldTool.MoveTo(mainCamera.ScreenToWorldPoint(mouse.position.ReadValue()));
            }
        }

        // Over the UI no tool is picked up. The cursor gets the arrow once when it enters the UI;
        // after that the UI element under it may change it.
        private bool IsPointerOverUi()
        {
            bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (overUi && !pointerOverUi)
            {
                ShowCursor(CursorState.Default);
            }
            pointerOverUi = overUi;
            return overUi;
        }

        private static void ShowCursor(CursorState state)
        {
            GameServices.GameCursor?.Show(state);
        }

        private void UseOnPlotUnder(Vector2 point)
        {
            GardenPlot plot = Find<GardenPlot>(point);
            heldTool.Hover(plot);
            if (plot != null)
            {
                heldTool.UseOn(plot);
            }
        }

        private T Find<T>(Vector2 point) where T : Component
        {
            Physics2D.OverlapPoint(point, filter, hits);
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out T component))
                {
                    return component;
                }
            }
            return null;
        }

        private void Drop()
        {
            if (heldTool == null)
            {
                return;
            }
            heldTool.ReturnHome();
            heldTool = null;
        }
    }
}
