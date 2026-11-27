using UnityEngine;

namespace SproutOfDebt.Core.Stubs
{
    /// <summary>
    /// Registers stub wallet/inventory in <see cref="GameServices"/> when no real implementation is registered,
    /// so a location scene can be played on its own. Remove once the real systems exist.
    /// A new game starts with nothing (GDD): onboarding grants its items, seeds are bought in the Seed Shop.
    /// For testing, use Tools > Debug > Add Test Items / Add 100 Coins in Play mode.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class StubServicesInstaller : MonoBehaviour
    {
        private void Awake()
        {
            if (GameServices.Wallet == null)
            {
                GameServices.Wallet = new StubWallet(0);
            }

            if (GameServices.BrewedPotions == null)
            {
                GameServices.BrewedPotions = new StubBrewedPotionLog();
            }

            if (GameServices.Inventory == null)
            {
                GameServices.Inventory = new StubInventory();
            }
        }
    }
}
