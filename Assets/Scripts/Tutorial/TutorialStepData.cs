using System;
using SproutOfDebt.Core;
using UnityEngine;

namespace SproutOfDebt.Tutorial
{
    public enum CharacterType
    {
        None,
        Moneylender,
        Imp
    }

    public enum CharacterMood
    {
        Neutral,
        Stern,
        Pleased,
        Worried,
        Happy
    }

    public enum HighlightTarget
    {
        None,
        DebtBar,
        Rake,
        Plot1,
        InventoryButton,
        LavenderSeedsSlot,
        WateringCan,
        GrowthTimer,
        RipePlant1,
        RestOfBedRipe,
        NavLaboratory,
        RecipeBook,
        BrewButton,
        CandleAndCauldron,
        CauldronBowl,
        SpoonAndBowl,
        NavCounter,
        OrderPanel,
        SellButton,
        CoinCounter,
        TreasuryPayment,
        PlotStatus1,
        BrewingSteps,
        TreasuryWindow,
        TreasuryAmount
    }

    [Serializable]
    public struct TutorialStepData
    {
        public TutorialStep step;
        public CharacterType character;
        public CharacterMood mood;
        public string thought;
        public string action;
        public bool showAction;
        public bool showNext;
        public HighlightTarget highlight;
        /// <summary>Optional second cutout shown together with <see cref="highlight"/> (Figma steps with two highlights).</summary>
        public HighlightTarget secondaryHighlight;
        /// <summary>
        /// Optional area the dim opens instead of the highlights; the highlights then only get frames inside it
        /// (Figma: Treasury step, the whole window is open and the amount and Pay button are framed).
        /// </summary>
        public HighlightTarget cutoutArea;
        /// <summary>Top-left corner of the imp hint in the 1920x1080 frame, as placed on this step's Figma screen.</summary>
        public Vector2 hintTopLeft;

        public TutorialStepData(
            TutorialStep step,
            CharacterType character,
            CharacterMood mood,
            string thought,
            string action = "",
            bool showAction = true,
            bool showNext = false,
            HighlightTarget highlight = HighlightTarget.None,
            HighlightTarget secondaryHighlight = HighlightTarget.None,
            Vector2 hintTopLeft = default,
            HighlightTarget cutoutArea = HighlightTarget.None)
        {
            this.step = step;
            this.character = character;
            this.mood = mood;
            this.thought = thought;
            this.action = action;
            this.showAction = showAction;
            this.showNext = showNext;
            this.highlight = highlight;
            this.secondaryHighlight = secondaryHighlight;
            this.hintTopLeft = hintTopLeft;
            this.cutoutArea = cutoutArea;
        }
    }
}
