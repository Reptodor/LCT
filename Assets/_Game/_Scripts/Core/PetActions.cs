using UnityEngine;

public static class PetActions
{
    public const int WorkReward = 15;
    public const int SnackCost = 10;
    public const int SnackHunger = 15;
    public const int HungerMax = 100;
    public const int JoyMax = 100;

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

    public static void Feed(GameState state, int hunger)
    {
        if (state == null || hunger <= 0)
        {
            return;
        }

        state.hunger = Mathf.Min(HungerMax, state.hunger + hunger);
    }

    public static void AddJoy(GameState state, int amount)
    {
        if (state == null || amount <= 0)
        {
            return;
        }

        state.joy = Mathf.Min(JoyMax, state.joy + amount);
    }

    public static void SpendJoy(GameState state, int amount)
    {
        if (state == null || amount <= 0)
        {
            return;
        }

        state.joy = Mathf.Max(0, state.joy - amount);
    }
}
