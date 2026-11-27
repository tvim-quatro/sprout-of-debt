using SproutOfDebt.Core;

namespace SproutOfDebt.Tutorial
{
    /// <summary>
    /// Fills the live numbers into onboarding lines, so they say what really happened instead of fixed figures:
    /// {coins} = the player's coins, {repaid} = debt paid so far, {remaining} = debt left, {total} = the whole debt.
    /// </summary>
    public static class TutorialText
    {
        public static string Format(string line)
        {
            if (string.IsNullOrEmpty(line) || line.IndexOf('{') < 0)
            {
                return line;
            }
            IWallet wallet = GameServices.Wallet;
            IDebtService debt = GameServices.Debt;
            return line
                .Replace("{coins}", wallet != null ? wallet.Coins.ToString() : "0")
                .Replace("{repaid}", debt != null ? debt.Repaid.ToString() : "0")
                .Replace("{remaining}", debt != null ? debt.Remaining.ToString() : "0")
                .Replace("{total}", debt != null ? debt.TotalDebt.ToString() : "0");
        }
    }
}
