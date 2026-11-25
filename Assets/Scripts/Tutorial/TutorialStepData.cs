using System;
using SproutOfDebt.Core;

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
        TreasuryPayment
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

        public TutorialStepData(
            TutorialStep step,
            CharacterType character,
            CharacterMood mood,
            string thought,
            string action = "",
            bool showAction = true,
            bool showNext = false,
            HighlightTarget highlight = HighlightTarget.None)
        {
            this.step = step;
            this.character = character;
            this.mood = mood;
            this.thought = thought;
            this.action = action;
            this.showAction = showAction;
            this.showNext = showNext;
            this.highlight = highlight;
        }
    }
}
