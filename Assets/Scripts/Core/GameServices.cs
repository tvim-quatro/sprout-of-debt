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
        /// <summary>The shared mouse cursor (registered by the Control_GameCursor prefab); null on screens without it.</summary>
        public static IGameCursor GameCursor { get; set; }
        /// <summary>What the current location does with inventory items (registered by the location); null where items can't be used.</summary>
        public static IInventoryItemHandler ItemHandler { get; set; }
        /// <summary>Short notifications at the top of the screen (registered by the Panel_Toast prefab); null on screens without it.</summary>
        public static IToastService Toasts { get; set; }
        /// <summary>The "item flies into the inventory" effect (registered by the HUD prefab); null on screens without it.</summary>
        public static IItemFlyService ItemFly { get; set; }
        /// <summary>The single save slot (registered by the save system at startup); null before that.</summary>
        public static ISaveService Save { get; set; }
        /// <summary>Full-screen windows outside the WindowManager (the Recipe Book); null until the first one opens.</summary>
        public static IModalState Modals { get; set; }
        /// <summary>Onboarding and gameplay loop tutorial coordinator; null on screens without it.</summary>
        public static ITutorialService Tutorial { get; set; }
        /// <summary>Shared debt repayment service; null before the shop/HUD bootstrap runs.</summary>
        public static IDebtService Debt { get; set; }

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
            GameCursor = null;
            ItemHandler = null;
            Toasts = null;
            ItemFly = null;
            Save = null;
            Modals = null;
            Tutorial = null;
            Debt = null;
            progress = null;
        }
    }
}
