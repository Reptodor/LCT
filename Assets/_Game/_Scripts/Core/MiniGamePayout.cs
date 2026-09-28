using UnityEngine;
using UnityEngine.SceneManagement;

public static class MiniGamePayout
{
    private const string ResourceName = "MiniGameRewards";

    private static MiniGameRewards _rewards;

    public static int GrantForActiveScene()
    {
        if (!GameSession.IsReady)
        {
            return 0;
        }

        int amount = AmountFor(SceneManager.GetActiveScene().name);
        if (!PetActions.TryEarn(GameSession.State, amount))
        {
            return 0;
        }

        GameSession.Persist();
        return amount;
    }

    public static int AmountFor(string sceneName)
    {
        MiniGameRewards rewards = Rewards;
        if (rewards == null)
        {
            return 0;
        }

        return rewards.AmountFor(sceneName);
    }

    private static MiniGameRewards Rewards
    {
        get
        {
            if (_rewards == null)
            {
                _rewards = Resources.Load<MiniGameRewards>(ResourceName);
            }

            return _rewards;
        }
    }
}
