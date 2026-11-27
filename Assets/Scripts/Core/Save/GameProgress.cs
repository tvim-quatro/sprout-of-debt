using System;
using UnityEngine;

namespace SproutOfDebt.Core.Save
{
    /// <summary>
    /// Everything the player has achieved, in one place, grouped by game area. It lives outside the scenes, so it
    /// survives location changes, and it is a plain serializable object, so the save system can write it to disk as is.
    /// Each area adds its own group here (Garden now; Economy, Laboratory, ... when they move their state in).
    /// </summary>
    [Serializable]
    public class GameProgress
    {
        [SerializeField] private GardenProgress garden = new GardenProgress();
        [SerializeField] private TutorialProgress tutorial = new TutorialProgress();
        [SerializeField] private DebtProgress debt = new DebtProgress();
        [Tooltip("Seconds the player has spent in the game. Timers (plant growth) run on this clock, so they stop while the game is closed.")]
        [SerializeField] private double playSeconds;

        public GardenProgress Garden => garden;
        public TutorialProgress Tutorial => tutorial;
        public DebtProgress Debt => debt;

        /// <summary>The game's own clock, in seconds of play. It is saved, and it does not run while the game is closed.</summary>
        public double PlaySeconds => playSeconds;

        public void AddPlaySeconds(double seconds)
        {
            if (seconds > 0)
            {
                playSeconds += seconds;
            }
        }
    }
}
