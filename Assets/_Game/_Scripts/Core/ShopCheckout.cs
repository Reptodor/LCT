using UnityEngine;

public static class ShopCheckout
{
    public static bool Owns(GameState state, string id)
    {
        if (state == null || state.ownedItems == null || string.IsNullOrEmpty(id))
        {
            return false;
        }

        for (int i = 0; i < state.ownedItems.Length; i++)
        {
            if (state.ownedItems[i] == id)
            {
                return true;
            }
        }

        return false;
    }

    public static bool CanPay(GameState state, ShopItem item)
    {
        return state != null && item != null && item.Cost > 0 && state.coins >= item.Cost && !AlreadyOwned(state, item);
    }

    public static bool TryPay(GameState state, ShopItem item)
    {
        if (!CanPay(state, item))
        {
            return false;
        }

        state.coins -= item.Cost;
        if (item.Category == ShopCategory.Optional)
        {
            Remember(state, item.Id);
        }

        return true;
    }

    public static void RestoreFurniture()
    {
        if (!GameSession.IsReady || GameSession.State.ownedItems == null)
        {
            return;
        }

        string[] owned = GameSession.State.ownedItems;
        for (int i = 0; i < owned.Length; i++)
        {
            if (!string.IsNullOrEmpty(owned[i]))
            {
                LivingFurnish.Place(owned[i]);
            }
        }
    }

    static bool AlreadyOwned(GameState state, ShopItem item)
    {
        return item.Category == ShopCategory.Optional && Owns(state, item.Id);
    }

    static void Remember(GameState state, string id)
    {
        if (Owns(state, id))
        {
            return;
        }

        string[] current = state.ownedItems ?? new string[0];
        var next = new string[current.Length + 1];
        for (int i = 0; i < current.Length; i++)
        {
            next[i] = current[i];
        }

        next[current.Length] = id;
        state.ownedItems = next;
    }
}
