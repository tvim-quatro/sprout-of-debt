using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SproutOfDebt.Laboratory
{
    public class LaboratoryDragController : MonoBehaviour
    {
        private readonly List<Collider2D> hits = new List<Collider2D>();

        private Camera mainCamera;
        private ContactFilter2D filter;
        private LaboratoryTool heldTool;

        private void Awake()
        {
            mainCamera = Camera.main;
            filter = new ContactFilter2D().NoFilter();
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
                return;
            }
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                Drop();
                return;
            }

            heldTool.MoveTo(mousePosition);
            UseOnTargetsAt(mousePosition);
        }

        private void PickUpAt(Vector2 point)
        {
            Physics2D.OverlapPoint(point, filter, hits);
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out LaboratoryTool tool) && tool.CanPickUp)
                {
                    heldTool = tool;
                    heldTool.PickUp();
                    return;
                }
            }
        }

        private void UseOnTargetsAt(Vector2 point)
        {
            Physics2D.OverlapPoint(point, filter, hits);
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
            heldTool.ReturnHome();
            heldTool = null;
        }
    }
}
