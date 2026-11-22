namespace SproutOfDebt.Core
{
    /// <summary>
    /// Sequential steps of the onboarding / first-time tutorial flow (Figma: Onboarding page).
    /// </summary>
    public enum TutorialStep
    {
        // 1. Intro in Garden
        IntroMoneylender1 = 0,
        IntroMoneylender2 = 1,
        IntroMoneylender3 = 2,

        // 2. Garden loop
        GardenRake = 10,
        GardenTill = 11,
        GardenOpenInventory = 12,
        GardenPickSeeds = 13,
        GardenSow = 14,
        GardenWateringCan = 15,
        GardenWater = 16,
        GardenGrowthTimer = 17,
        GardenHarvestFirst = 18,
        GardenHarvestRest = 19,
        GardenGoToLab = 20,

        // 3. Laboratory loop
        LabOpenRecipe = 30,
        LabClickBrew = 31,
        LabLightFire = 32,
        LabAddIngredients = 33,
        LabStir = 34,
        LabPotionReady = 35,
        LabGoToCounter = 36,

        // 4. Counter loop
        CounterCustomerRequest = 40,
        CounterSellPotion = 41,
        CounterPotionSold = 42,
        CounterClickDebtBar = 43,
        CounterPayDebt = 44,
        CounterMoneylenderFarewell = 45,

        // Finished
        Completed = 100
    }
}
