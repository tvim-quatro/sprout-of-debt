using System;
using System.Collections.Generic;
using UnityEngine;

namespace SproutOfDebt.Core.Save
{
    /// <summary>State of every garden cell, by cell number (0 = top-left, numbered left to right, top row first).</summary>
    [Serializable]
    public class GardenProgress
    {
        [SerializeField] private List<PlotProgress> plots = new List<PlotProgress>();

        /// <summary>Returns the cell's state, creating an empty cell the first time it is asked for.</summary>
        public PlotProgress GetPlot(int index)
        {
            while (plots.Count <= index)
            {
                plots.Add(new PlotProgress());
            }
            return plots[index];
        }
    }
}
