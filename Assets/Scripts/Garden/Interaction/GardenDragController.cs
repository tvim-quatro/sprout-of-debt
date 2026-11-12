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
    /// </summary>
    public class GardenDragController : MonoBehaviour
    {
        private readonly List<Collider2D> hits = new List<Collider2D>();

        private Camera mainCamera;
        private ContactFilter2D filter;
        private GardenTool heldTool;

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
                GardenTool tool = FindPickableTool(cursor);
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

        /// <summary>The tool under the point that can be picked up now, or null (also when the cursor is over the UI).</summary>
        private GardenTool FindPickableTool(Vector2 point)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return null;
            }
            GardenTool tool = Find<GardenTool>(point);
            return tool != null && tool.CanPickUp ? tool : null;
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
