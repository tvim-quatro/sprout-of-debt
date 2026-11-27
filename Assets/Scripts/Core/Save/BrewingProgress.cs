using System;
using UnityEngine;

namespace SproutOfDebt.Core.Save
{
    /// <summary>
    /// The brew in progress: which potion, which step it is on and how far that step has got.
    /// It lives in <see cref="GameProgress"/>, so leaving the Laboratory (or the game) does not reset the brew.
    /// </summary>
    [Serializable]
    public class BrewingProgress
    {
        [SerializeField] private bool active;
        [Tooltip("Id of the potion being brewed; its recipe is found by it.")]
        [SerializeField] private string potionId;
        [SerializeField] private int stepIndex;
        [Tooltip("Ingredients already put into the bowl in the current step.")]
        [SerializeField] private int addedCount;
        [Tooltip("Seconds already stirred in the current step.")]
        [SerializeField] private float stirredSeconds;

        public bool Active => active;
        public string PotionId => potionId;
        public int StepIndex => stepIndex;
        public int AddedCount => addedCount;
        public float StirredSeconds => stirredSeconds;

        public void Set(string potion, int step, int added, float stirred)
        {
            active = true;
            potionId = potion;
            stepIndex = step;
            addedCount = added;
            stirredSeconds = stirred;
        }

        public void Clear()
        {
            active = false;
            potionId = null;
            stepIndex = 0;
            addedCount = 0;
            stirredSeconds = 0f;
        }
    }
}
