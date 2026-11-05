using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class SpoonTool : LaboratoryTool
    {
        public override bool UseOn(Collider2D target, float deltaTime)
        {
            if (target.TryGetComponent(out Bowl _))
            {
                Station.Stir(deltaTime);
            }
            return false;
        }
    }
}
