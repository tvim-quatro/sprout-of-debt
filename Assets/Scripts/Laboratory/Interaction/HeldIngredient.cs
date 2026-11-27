using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class HeldIngredient : LaboratoryTool
    {
        private IngredientData ingredient;

        public override bool CanPickUp => false;

        public void Fill(IngredientData data)
        {
            ingredient = data;
            gameObject.SetActive(true);
            if (data != null && data.icon != null)
            {
                SpriteRenderer.sprite = data.icon;
            }
        }

        public override bool UseOn(Collider2D target, float deltaTime)
        {
            return ingredient != null && target.TryGetComponent(out Bowl _) && Station.AddIngredient(ingredient);
        }

        protected override void OnReturnedHome()
        {
            ingredient = null;
            gameObject.SetActive(false);
        }
    }
}
