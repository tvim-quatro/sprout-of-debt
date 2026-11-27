using SproutOfDebt.Core;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class LabIngredientHandler : MonoBehaviour, IInventoryItemHandler
    {
        [SerializeField] private IngredientData[] ingredients;
        [SerializeField] private HeldIngredient heldIngredient;
        [SerializeField] private LaboratoryDragController dragController;

        private void OnEnable()
        {
            GameServices.ItemHandler = this;
        }

        private void OnDisable()
        {
            if (ReferenceEquals(GameServices.ItemHandler, this))
            {
                GameServices.ItemHandler = null;
            }
        }

        public bool CanDrag(string itemId)
        {
            return heldIngredient != null && dragController != null && Find(itemId) != null
                && GameServices.Inventory != null && GameServices.Inventory.GetCount(itemId) > 0;
        }

        public void BeginDrag(string itemId)
        {
            if (!CanDrag(itemId))
            {
                return;
            }
            heldIngredient.Fill(Find(itemId));
            dragController.Hold(heldIngredient);
        }

        public string GetItemInfo(string itemId)
        {
            return null;
        }

        private IngredientData Find(string itemId)
        {
            if (ingredients == null || string.IsNullOrEmpty(itemId))
            {
                return null;
            }
            foreach (IngredientData ingredient in ingredients)
            {
                if (ingredient != null && ingredient.id == itemId)
                {
                    return ingredient;
                }
            }
            return null;
        }
    }
}
