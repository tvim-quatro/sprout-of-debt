using SproutOfDebt.Core;
using UnityEngine;

namespace SproutOfDebt.Shop
{
    /// <summary>Global access to the shop systems. The treasury survives scene loads.</summary>
    public static class ShopContext
    {
        public static Treasury Treasury { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetContext()
        {
            Treasury = null;
        }
    }

    /// <summary>Creates the <see cref="Treasury"/> the first time a scene with this component loads and registers it on <see cref="GameServices.Debt"/>.</summary>
    [DefaultExecutionOrder(-1000)]
    public class ShopBootstrap : MonoBehaviour
    {
        [SerializeField] private ShopConfig config;

        private void Awake()
        {
            bool hadTreasury = ShopContext.Treasury != null;
            if (!hadTreasury && config != null)
            {
                ShopContext.Treasury = new Treasury(config);
            }
            if (ShopContext.Treasury != null)
            {
                GameServices.Debt = ShopContext.Treasury;
                if (hadTreasury && GameServices.Progress != null)
                {
                    // A loaded save or a new game replaced the progress while the treasury lived on.
                    ShopContext.Treasury.Restore(GameServices.Progress.Debt.Repaid);
                }
            }
        }
    }
}
