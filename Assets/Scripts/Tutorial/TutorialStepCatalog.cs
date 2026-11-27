using System.Collections.Generic;
using SproutOfDebt.Core;
using UnityEngine;

namespace SproutOfDebt.Tutorial
{
    /// <summary>
    /// Exact dialogue, hints, moods, highlights and hint positions for all tutorial steps (Figma: Onboarding page).
    /// </summary>
    public static class TutorialStepCatalog
    {
        private static readonly Dictionary<TutorialStep, TutorialStepData> Steps = new Dictionary<TutorialStep, TutorialStepData>
        {
            // 1. Intro Moneylender
            [TutorialStep.IntroMoneylender1] = new TutorialStepData(
                TutorialStep.IntroMoneylender1,
                CharacterType.Moneylender,
                CharacterMood.Neutral,
                "So you're the new owner of the old lab. The keys are yours now, little imp. And so is the debt.",
                "",
                showAction: false,
                showNext: true,
                HighlightTarget.None),

            [TutorialStep.IntroMoneylender2] = new TutorialStepData(
                TutorialStep.IntroMoneylender2,
                CharacterType.Moneylender,
                CharacterMood.Stern,
                "You owe me {total} coins. This bar shows how much you've paid back. No deadline, but I'll be counting.",
                "",
                showAction: false,
                showNext: true,
                HighlightTarget.DebtBar),

            [TutorialStep.IntroMoneylender3] = new TutorialStepData(
                TutorialStep.IntroMoneylender3,
                CharacterType.Moneylender,
                CharacterMood.Neutral,
                "Here's a pack of seeds to get you started. Grow, brew, sell, and bring the coins to me.",
                "",
                showAction: false,
                showNext: true,
                HighlightTarget.None),

            // 2. Garden loop
            [TutorialStep.GardenRake] = new TutorialStepData(
                TutorialStep.GardenRake,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "First, the soil. It needs loosening before I plant anything.",
                "Hold the left mouse button on the rake",
                showAction: true,
                showNext: false,
                HighlightTarget.Rake,
                hintTopLeft: new Vector2(440f, 170f)),

            [TutorialStep.GardenTill] = new TutorialStepData(
                TutorialStep.GardenTill,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "Now over an empty cell, and give it a good scratch.",
                "Drag the rake onto the highlighted cell",
                showAction: true,
                showNext: false,
                HighlightTarget.Plot1,
                hintTopLeft: new Vector2(836f, 112f)),

            [TutorialStep.GardenOpenInventory] = new TutorialStepData(
                TutorialStep.GardenOpenInventory,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "The soil is ready. My seeds are in the bag.",
                "Click the bag or press Tab",
                showAction: true,
                showNext: false,
                HighlightTarget.InventoryButton,
                hintTopLeft: new Vector2(440f, 170f)),

            [TutorialStep.GardenPickSeeds] = new TutorialStepData(
                TutorialStep.GardenPickSeeds,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "Lavender seeds, just what I need.",
                "Hold the left mouse button on the seeds",
                showAction: true,
                showNext: false,
                HighlightTarget.LavenderSeedsSlot,
                hintTopLeft: new Vector2(40f, 170f)),

            [TutorialStep.GardenSow] = new TutorialStepData(
                TutorialStep.GardenSow,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "Into the loosened cell they go.",
                "Drag the seeds onto the highlighted cell",
                showAction: true,
                showNext: false,
                HighlightTarget.Plot1,
                hintTopLeft: new Vector2(836f, 112f)),

            [TutorialStep.GardenWateringCan] = new TutorialStepData(
                TutorialStep.GardenWateringCan,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "The drop above the cell means the seeds are thirsty.",
                "Hold the left mouse button on the watering can",
                showAction: true,
                showNext: false,
                HighlightTarget.WateringCan,
                secondaryHighlight: HighlightTarget.PlotStatus1,
                hintTopLeft: new Vector2(440f, 170f)),

            [TutorialStep.GardenWater] = new TutorialStepData(
                TutorialStep.GardenWater,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "A good splash, and they're all set.",
                "Drag the watering can onto the highlighted cell",
                showAction: true,
                showNext: false,
                HighlightTarget.Plot1,
                hintTopLeft: new Vector2(32f, 116f)),

            [TutorialStep.GardenGrowthTimer] = new TutorialStepData(
                TutorialStep.GardenGrowthTimer,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "Plants need time to grow. The timer above shows how long my lavender still needs. It keeps counting even when I'm away.",
                "",
                showAction: false,
                showNext: true,
                HighlightTarget.GrowthTimer,
                hintTopLeft: new Vector2(440f, 170f)),

            [TutorialStep.GardenHarvestFirst] = new TutorialStepData(
                TutorialStep.GardenHarvestFirst,
                CharacterType.Imp,
                CharacterMood.Happy,
                "It's ripe! The basket above means it's ready to pick.",
                "Click the plant to harvest it",
                showAction: true,
                showNext: false,
                HighlightTarget.RipePlant1,
                hintTopLeft: new Vector2(440f, 112f)),

            [TutorialStep.GardenHarvestRest] = new TutorialStepData(
                TutorialStep.GardenHarvestRest,
                CharacterType.Imp,
                CharacterMood.Happy,
                "While I was busy, the bed gave me one more lavender and a moonpetal. Each plant gives three!",
                "Click each ripe plant to harvest it",
                showAction: true,
                showNext: false,
                HighlightTarget.RestOfBedRipe,
                hintTopLeft: new Vector2(32f, 116f)),

            [TutorialStep.GardenGoToLab] = new TutorialStepData(
                TutorialStep.GardenGoToLab,
                CharacterType.Imp,
                CharacterMood.Happy,
                "My basket is full. Time to brew!",
                "Click Laboratory or press 2",
                showAction: true,
                showNext: false,
                HighlightTarget.NavLaboratory,
                hintTopLeft: new Vector2(440f, 720f)),

            // 3. Laboratory loop
            [TutorialStep.LabOpenRecipe] = new TutorialStepData(
                TutorialStep.LabOpenRecipe,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "Which potion can I make? My recipe book knows.",
                "Click the recipe book or press B",
                showAction: true,
                showNext: false,
                HighlightTarget.RecipeBook,
                hintTopLeft: new Vector2(32f, 112f)),

            [TutorialStep.LabClickBrew] = new TutorialStepData(
                TutorialStep.LabClickBrew,
                CharacterType.Imp,
                CharacterMood.Happy,
                "Calming Potion: two lavender and one moonpetal. I have just enough!",
                "Click Brew",
                showAction: true,
                showNext: false,
                HighlightTarget.BrewButton,
                hintTopLeft: new Vector2(860f, 790f)),

            [TutorialStep.LabLightFire] = new TutorialStepData(
                TutorialStep.LabLightFire,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "The steps are on the left, I'll follow them one by one. First, the fire.",
                "Drag the candle under the cauldron",
                showAction: true,
                showNext: false,
                HighlightTarget.CandleAndCauldron,
                secondaryHighlight: HighlightTarget.BrewingSteps,
                hintTopLeft: new Vector2(860f, 110f)),

            [TutorialStep.LabOpenInventory] = new TutorialStepData(
                TutorialStep.LabOpenInventory,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "Now the ingredients. They are all in my bag.",
                "Click the bag or press Tab",
                showAction: true,
                showNext: false,
                HighlightTarget.InventoryButton,
                // Not in Figma yet: same place as the garden's "open the inventory" step.
                hintTopLeft: new Vector2(440f, 170f)),

            [TutorialStep.LabPickIngredient] = new TutorialStepData(
                TutorialStep.LabPickIngredient,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "This is the one the recipe asks for next.",
                "Hold the left mouse button on the highlighted ingredient",
                showAction: true,
                showNext: false,
                HighlightTarget.RequiredIngredientSlot,
                // Not in Figma yet: left of the open inventory, like the garden's "pick the seeds" step.
                hintTopLeft: new Vector2(40f, 170f)),

            [TutorialStep.LabAddIngredients] = new TutorialStepData(
                TutorialStep.LabAddIngredients,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "Two lavender and one moonpetal go into the bowl, one at a time.",
                "Drag the ingredient onto the cauldron",
                showAction: true,
                showNext: false,
                HighlightTarget.CauldronBowl,
                hintTopLeft: new Vector2(32f, 740f)),

            [TutorialStep.LabStir] = new TutorialStepData(
                TutorialStep.LabStir,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "Now a good stir until the ring is full.",
                "Hold the spoon and move it over the cauldron",
                showAction: true,
                showNext: false,
                HighlightTarget.SpoonAndBowl,
                hintTopLeft: new Vector2(32f, 740f)),

            [TutorialStep.LabPotionReady] = new TutorialStepData(
                TutorialStep.LabPotionReady,
                CharacterType.Imp,
                CharacterMood.Happy,
                "My very first potion! It went straight into my bag.",
                "",
                showAction: false,
                showNext: true,
                HighlightTarget.InventoryButton,
                hintTopLeft: new Vector2(32f, 740f)),

            [TutorialStep.LabGoToCounter] = new TutorialStepData(
                TutorialStep.LabGoToCounter,
                CharacterType.Imp,
                CharacterMood.Happy,
                "The counter is open now. Time to find my first customer!",
                "Click Counter or press 3",
                showAction: true,
                showNext: false,
                HighlightTarget.NavCounter,
                hintTopLeft: new Vector2(440f, 720f)),

            // 4. Counter loop
            [TutorialStep.CounterCustomerRequest] = new TutorialStepData(
                TutorialStep.CounterCustomerRequest,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "A customer! The panel shows what they want, how many I have and how much they'll pay.",
                "",
                showAction: false,
                showNext: true,
                HighlightTarget.OrderPanel,
                hintTopLeft: new Vector2(300f, 112f)),

            [TutorialStep.CounterSellPotion] = new TutorialStepData(
                TutorialStep.CounterSellPotion,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "One Calming Potion, and I have exactly one.",
                "Click Sell",
                showAction: true,
                showNext: false,
                HighlightTarget.SellButton,
                hintTopLeft: new Vector2(300f, 112f)),

            [TutorialStep.CounterPotionSold] = new TutorialStepData(
                TutorialStep.CounterPotionSold,
                CharacterType.Imp,
                CharacterMood.Happy,
                "{coins} coins! Coins buy new seeds and pay off my debt.",
                "",
                showAction: false,
                showNext: true,
                HighlightTarget.CoinCounter,
                hintTopLeft: new Vector2(32f, 300f)),

            [TutorialStep.CounterClickDebtBar] = new TutorialStepData(
                TutorialStep.CounterClickDebtBar,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "Paying off the debt is my main goal. Every payment brings me closer to owning the lab.",
                "Click the debt bar",
                showAction: true,
                showNext: false,
                HighlightTarget.DebtBar,
                hintTopLeft: new Vector2(500f, 140f)),

            [TutorialStep.CounterPayDebt] = new TutorialStepData(
                TutorialStep.CounterPayDebt,
                CharacterType.Imp,
                CharacterMood.Neutral,
                "The Treasury. I pick how much to pay, and it goes straight to the moneylender. All {coins} coins it is!",
                "Set the amount and click Pay",
                showAction: true,
                showNext: false,
                HighlightTarget.TreasuryPayment,
                secondaryHighlight: HighlightTarget.TreasuryAmount,
                hintTopLeft: new Vector2(440f, 840f),
                cutoutArea: HighlightTarget.TreasuryWindow),

            [TutorialStep.CounterMoneylenderFarewell] = new TutorialStepData(
                TutorialStep.CounterMoneylenderFarewell,
                CharacterType.Moneylender,
                CharacterMood.Pleased,
                "{repaid} down, {remaining} to go. A fine start, imp. Keep the potions coming, and I'll keep counting.",
                "",
                showAction: false,
                showNext: true,
                HighlightTarget.None),
        };

        public static bool TryGet(TutorialStep step, out TutorialStepData data)
        {
            return Steps.TryGetValue(step, out data);
        }
    }
}
