using System;
using UnityEngine;

namespace SproutOfDebt.Core.Save
{
    /// <summary>One garden cell: what stage it is in, what grows there and when it was watered.</summary>
    [Serializable]
    public class PlotProgress
    {
        [SerializeField] private PlotState state;
        [Tooltip("Id of the plant growing here, empty when nothing is sown.")]
        [SerializeField] private string plantId;
        [Tooltip("Real time (UTC, Unix milliseconds) when the cell was watered. The plant keeps growing while the game is closed.")]
        [SerializeField] private long growStartUtcMs;

        public PlotState State => state;
        public string PlantId => plantId;
        public long GrowStartUtcMs => growStartUtcMs;

        public void SetState(PlotState newState)
        {
            state = newState;
        }

        public void SetPlant(string id)
        {
            plantId = id;
        }

        public void StartGrowing(long utcMs)
        {
            growStartUtcMs = utcMs;
        }
    }
}
