using SproutOfDebt.Core;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class IngredientJar : LaboratoryTool
    {
        [SerializeField] private IngredientData ingredient;
        [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.4f);

        public override bool CanPickUp => base.CanPickUp && GetCount() > 0;

        private void Start()
        {
            if (GameServices.Inventory != null)
            {
                GameServices.Inventory.ItemChanged += OnItemChanged;
            }
            RefreshColor();
        }

        private void OnDestroy()
        {
            if (GameServices.Inventory != null)
            {
                GameServices.Inventory.ItemChanged -= OnItemChanged;
            }
        }

        public override bool UseOn(Collider2D target, float deltaTime)
        {
            return target.TryGetComponent(out Bowl _) && Station.AddIngredient(ingredient);
        }

        private int GetCount()
        {
            if (ingredient == null || GameServices.Inventory == null)
            {
                return 0;
            }
            return GameServices.Inventory.GetCount(ingredient.id);
        }

        private void OnItemChanged(string itemId, int count)
        {
            if (ingredient != null && itemId == ingredient.id)
            {
                RefreshColor();
            }
        }

        private void RefreshColor()
        {
            SpriteRenderer.color = GetCount() > 0 ? Color.white : emptyColor;
        }
    }
}
