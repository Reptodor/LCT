using UnityEngine;

public static class RoomUnlocks
{
    const string CatalogResource = "SavingsGoals";

    public static bool IsOpen(string roomId)
    {
        if (!TryGoal(roomId, out SavingsGoalCatalog.Goal goal))
        {
            return true;
        }

        if (!GameSession.IsReady)
        {
            return false;
        }

        return SavingsBank.IsFilled(GameSession.State, goal.Id, goal.Target);
    }

    public static string FurnishBlockedText(string roomId)
    {
        if (TryGoal(roomId, out SavingsGoalCatalog.Goal goal) && !string.IsNullOrEmpty(goal.Title))
        {
            return "«" + goal.Title + "» ещё закрыта. Расставить мебель нельзя, пока не купишь комнату";
        }

        return "Расставить мебель нельзя, пока не купишь комнату";
    }

    static bool TryGoal(string roomId, out SavingsGoalCatalog.Goal goal)
    {
        goal = default;
        if (string.IsNullOrEmpty(roomId))
        {
            return false;
        }

        string key = Canon(roomId);
        if (key == "living")
        {
            return false;
        }

        SavingsGoalCatalog catalog = Resources.Load<SavingsGoalCatalog>(CatalogResource);
        if (catalog == null)
        {
            return false;
        }

        for (int i = 0; i < catalog.Count; i++)
        {
            SavingsGoalCatalog.Goal candidate = catalog.Get(i);
            if (Canon(candidate.Id) == key)
            {
                goal = candidate;
                return true;
            }
        }

        return false;
    }

    static string Canon(string id)
    {
        string key = id.Trim().ToLowerInvariant();
        if (key == "mainroom" || key == "main" || key == "living" || key == "livingroom" || key == "гостиная")
        {
            return "living";
        }

        if (key == "bedroom" || key == "bed")
        {
            return "bedroom";
        }

        if (key == "bathroom" || key == "bath")
        {
            return "bath";
        }

        return key;
    }
}
