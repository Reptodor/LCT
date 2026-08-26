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

        return TryEarn(state, WorkReward);
    }

    public static bool TryEarn(GameState state, int amount)
    {
        if (state == null || amount <= 0)
        {
            return false;
        }

        state.coins += amount;
        return true;
    }

    public static bool TrySnack(GameState state)
    {
        return TryBuyFood(state, SnackCost, SnackHunger);
    }

    public static bool TryBuyFood(GameState state, int cost, int hunger)
    {
        if (state == null || cost <= 0 || hunger <= 0)
        {
            return false;
        }

        if (state.coins < cost)
        {
            return false;
        }

        state.coins -= cost;
        state.hunger = Mathf.Min(HungerMax, state.hunger + hunger);
        return true;
    }
}
