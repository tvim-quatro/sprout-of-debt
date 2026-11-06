using UnityEngine;

namespace SproutOfDebt.Core
{
    /// <summary>
    /// Global access point for shared game systems. The real implementations (inventory, save system)
    /// register themselves here; until they exist, <see cref="Stubs.StubServicesInstaller"/> fills the gaps.
    /// Values survive scene loads.
    /// </summary>
    public static class GameServices
    {
        public static IWallet Wallet { get; set; }
        public static IInventory Inventory { get; set; }
        public static IBrewedPotionLog BrewedPotions { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetServices()
        {
            Wallet = null;
            Inventory = null;
            BrewedPotions = null;
        }
    }
}
