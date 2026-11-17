using System;
using UnityEngine;

namespace SproutOfDebt.Core.Save
{
    /// <summary>
    /// One garden cell: what stage it is in, what grows there, how much water it has got and when it was watered.
    /// Unfinished watering is kept too, so all progress survives leaving the location.
    /// </summary>
    [Serializable]
    public class PlotProgress
    {
        [SerializeField] private PlotState state;
        [Tooltip("Id of the plant growing here, empty when nothing is sown.")]
        [SerializeField] private string plantId;
        [Tooltip("How much of the needed water a sown cell has got, 0..1 (the plant starts growing at 1).")]
        [SerializeField] private float waterProgress;
        [Tooltip("Real time (UTC, Unix milliseconds) when the cell was watered. The plant keeps growing while the game is closed.")]
        [SerializeField] private long growStartUtcMs;

        public PlotState State => state;
        public string PlantId => plantId;
        public float WaterProgress => waterProgress;
        public long GrowStartUtcMs => growStartUtcMs;

        /// <summary>Moves the cell to a new stage; the watering of the previous stage starts over.</summary>
        public void SetState(PlotState newState)
        {
            state = newState;
            waterProgress = 0f;
        }

        public void SetPlant(string id)
        {
            plantId = id;
        }

        public void SetWaterProgress(float value)
        {
            waterProgress = Mathf.Clamp01(value);
        }

        public void StartGrowing(long utcMs)
        {
            growStartUtcMs = utcMs;
        }
    }
}
