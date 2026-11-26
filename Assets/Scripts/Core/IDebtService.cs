using System;

namespace SproutOfDebt.Core
{
    /// <summary>
    /// Shared debt-repayment service exposed through <see cref="GameServices.Debt"/> so every area
    /// (Laboratory recipe book, Seed Shop, Counter, HUD) reacts to debt payments without referencing Shop internals.
    /// </summary>
    public interface IDebtService
    {
        int Repaid { get; }
        int TotalDebt { get; }
        int Remaining { get; }
        float Progress { get; }
        bool IsPaidOff { get; }

        /// <summary>Raised whenever the repaid amount changes.</summary>
        event Action Changed;

        /// <summary>Raised once when the debt reaches TotalDebt.</summary>
        event Action PaidOff;
    }
}
