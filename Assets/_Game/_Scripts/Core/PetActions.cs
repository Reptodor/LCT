using UnityEngine;

public static class PetActions
{
    public const int WorkReward = 15;
    public const int SnackCost = 10;
    public const int SnackHunger = 15;
    public const int HungerMax = 100;

    public static bool TryWork(GameState state)
    {
        if (state == null)
        {
            return false;
        }

        state.coins += WorkReward;
        return true;
    }

    public static bool TrySnack(GameState state)
    {
        if (state == null || state.coins < SnackCost)
        {
            return false;
        }

        state.coins -= SnackCost;
        state.hunger = Mathf.Min(HungerMax, state.hunger + SnackHunger);
        return true;
    }
}
