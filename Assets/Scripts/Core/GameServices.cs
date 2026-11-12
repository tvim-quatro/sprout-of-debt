using SproutOfDebt.Core.Save;
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

        private static GameProgress progress;

        /// <summary>The player's progress (garden cells, ...). Created empty on first use; the save system will replace it with the loaded one.</summary>
        public static GameProgress Progress
        {
            get => progress ??= new GameProgress();
            set => progress = value;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetServices()
        {
            Wallet = null;
            Inventory = null;
            BrewedPotions = null;
            progress = null;
        }
    }
}
