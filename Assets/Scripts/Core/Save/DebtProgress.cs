using System;
using UnityEngine;

namespace SproutOfDebt.Core.Save
{
    /// <summary>
    /// Persistent debt repayment state inside <see cref="GameProgress"/>.
    /// </summary>
    [Serializable]
    public class DebtProgress
    {
        [SerializeField] private int repaid;

        public int Repaid
        {
            get => repaid;
            set => repaid = Mathf.Max(0, value);
        }
    }
}
