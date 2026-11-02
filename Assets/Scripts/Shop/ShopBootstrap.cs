using UnityEngine;

namespace SproutOfDebt.Shop
{
    /// <summary>Global access to the shop systems. The treasury survives scene loads.</summary>
    public static class ShopContext
    {
        public static Treasury Treasury { get; set; }
    }

    /// <summary>Creates the <see cref="Treasury"/> the first time a scene with this component loads.</summary>
    [DefaultExecutionOrder(-1000)]
    public class ShopBootstrap : MonoBehaviour
    {
        [SerializeField] private ShopConfig config;

        private void Awake()
        {
            if (ShopContext.Treasury == null && config != null)
            {
                ShopContext.Treasury = new Treasury(config);
            }
        }
    }
}
