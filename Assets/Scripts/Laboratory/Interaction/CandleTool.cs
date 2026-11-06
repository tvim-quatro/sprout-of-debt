using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class CandleTool : LaboratoryTool
    {
        public override bool UseOn(Collider2D target, float deltaTime)
        {
            if (target.TryGetComponent(out Brazier _))
            {
                Station.LightFire();
            }
            return false;
        }
    }
}
