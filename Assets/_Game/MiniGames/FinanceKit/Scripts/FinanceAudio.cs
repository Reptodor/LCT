using LCT.MiniGames.Shop;

namespace LCT.MiniGames.Finance
{
    public static class FinanceAudio
    {
        public static void Coin()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCoinDropSound();
        }

        public static void Banknote()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayBanknoteUseSound();
        }

        public static void Money(int value)
        {
            if (FinanceTheme.IsCoin(value))
                Coin();
            else
                Banknote();
        }

        public static void Good()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayLevelCompleteSound();
        }

        public static void Wrong()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrongSound();
        }

        public static void Victory()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayGameOverSound();
        }
    }
}
