using UnityEngine;

public static class SavingsBank
{
    public static int Saved(GameState state, string goalId)
    {
        int index = Find(state, goalId);
        if (index < 0)
        {
            return 0;
        }

        return Mathf.Max(0, state.savings[index].saved);
    }

    public static bool HasConfirmedGoal(GameState state)
    {
        return state != null && !string.IsNullOrEmpty(state.savingsGoalId);
    }

    public static bool IsFilled(GameState state, string goalId, int target)
    {
        return target > 0 && Saved(state, goalId) >= target;
    }

    public static bool CanSwitchGoal(GameState state, int confirmedTarget)
    {
        if (!HasConfirmedGoal(state))
        {
            return true;
        }

        return IsFilled(state, state.savingsGoalId, confirmedTarget);
    }

    public static void Select(GameState state, string goalId)
    {
        if (state == null)
        {
            return;
        }

        state.savingsGoalId = goalId ?? "";
    }

    public static int Deposit(GameState state, string goalId, int target, int step)
    {
        if (state == null || string.IsNullOrEmpty(goalId) || target <= 0 || step <= 0)
        {
            return 0;
        }

        if (state.savingsGoalId != goalId)
        {
            return 0;
        }

        int saved = Saved(state, goalId);
        int room = target - saved;
        if (room <= 0)
        {
            return 0;
        }

        int charge = Mathf.Min(step, room);
        if (state.coins < charge)
        {
            return 0;
        }

        state.coins -= charge;
        Write(state, goalId, saved + charge);
        return charge;
    }

    private static int Find(GameState state, string goalId)
    {
        if (state == null || state.savings == null || string.IsNullOrEmpty(goalId))
        {
            return -1;
        }

        for (int i = 0; i < state.savings.Length; i++)
        {
            SavingsPot pot = state.savings[i];
            if (pot != null && pot.id == goalId)
            {
                return i;
            }
        }

        return -1;
    }

    private static void Write(GameState state, string goalId, int saved)
    {
        int index = Find(state, goalId);
        if (index >= 0)
        {
            state.savings[index].saved = saved;
            return;
        }

        SavingsPot[] pots = state.savings ?? new SavingsPot[0];
        var grown = new SavingsPot[pots.Length + 1];
        for (int i = 0; i < pots.Length; i++)
        {
            grown[i] = pots[i];
        }

        grown[pots.Length] = new SavingsPot
        {
            id = goalId,
            saved = saved
        };
        state.savings = grown;
    }
}
