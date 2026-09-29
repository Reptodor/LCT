using System;
using System.Collections.Generic;
using UnityEngine;

public static class FurnitureWear
{
    const string ResourceName = "FurnitureWearConfig";
    const string RepairResourceName = "FurnitureRepairConfig";

    static FurnitureWearConfig _config;
    static FurnitureRepairConfig _repair;

    public static FurnitureWearConfig Config
    {
        get
        {
            if (_config == null)
            {
                _config = Resources.Load<FurnitureWearConfig>(ResourceName);
            }

            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<FurnitureWearConfig>();
            }

            return _config;
        }
    }

    public static FurnitureRepairConfig RepairConfig
    {
        get
        {
            if (_repair == null)
            {
                _repair = Resources.Load<FurnitureRepairConfig>(RepairResourceName);
            }

            if (_repair == null)
            {
                _repair = ScriptableObject.CreateInstance<FurnitureRepairConfig>();
            }

            return _repair;
        }
    }

    public static float StepSeconds => RepairConfig.BreakSeconds / Config.MaxHp;

    public static void Ensure(GameState state, string id)
    {
        if (state == null || string.IsNullOrEmpty(id) || Find(state, id) != null)
        {
            return;
        }

        Add(state, id, Config.MaxHp, DateTime.UtcNow.ToString("o"));
    }

    public static int Hp(GameState state, string id)
    {
        FurnitureCondition condition = Find(state, id);
        return condition == null ? Config.MaxHp : Mathf.Max(0, condition.hp);
    }

    public static bool IsBroken(GameState state, string id)
    {
        FurnitureCondition condition = Find(state, id);
        return condition != null && condition.hp <= 0;
    }

    public static float Ratio(GameState state, string id)
    {
        return Mathf.Clamp01(Hp(state, id) / (float)Config.MaxHp);
    }

    public static bool CatchUp(GameState state, out string[] broke)
    {
        broke = null;
        if (state == null || state.ownedItems == null || state.ownedItems.Length == 0)
        {
            return false;
        }

        bool changed = false;
        var broken = new List<string>();
        DateTime now = DateTime.UtcNow;
        float interval = StepSeconds;
        for (int i = 0; i < state.ownedItems.Length; i++)
        {
            string id = state.ownedItems[i];
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            FurnitureCondition condition = Find(state, id);
            if (condition == null)
            {
                Add(state, id, Config.MaxHp, now.ToString("o"));
                changed = true;
                continue;
            }

            if (!TryDue(condition.anchorUtc, out DateTime anchor) || anchor > now)
            {
                anchor = now;
                condition.anchorUtc = now.ToString("o");
                changed = true;
            }

            double elapsed = (now - anchor).TotalSeconds;
            if (elapsed < interval)
            {
                continue;
            }

            int steps = Math.Max(1, (int)(elapsed / interval));
            if (condition.hp > 0)
            {
                int next = condition.hp - steps;
                if (next <= 0)
                {
                    broken.Add(id);
                    SpendBreakJoy(state, id);
                }

                condition.hp = Mathf.Max(0, next);
            }

            condition.anchorUtc = anchor.AddSeconds(interval * steps).ToString("o");
            changed = true;
        }

        if (broken.Count > 0)
        {
            broke = broken.ToArray();
        }

        return changed;
    }

    public static int Cost(GameState state, string id)
    {
        int hp = Hp(state, id);
        int max = Config.MaxHp;
        int maxCost = RepairConfig.MaxCost;
        if (hp >= max || maxCost <= 0)
        {
            return 0;
        }

        if (hp <= 0)
        {
            return maxCost;
        }

        int share = Mathf.CeilToInt(maxCost * ((max - hp) / (float)max));
        return Mathf.Clamp(share, 1, Mathf.Max(1, maxCost - 1));
    }

    public static bool TryRepair(GameState state, string id, out int joy)
    {
        joy = 0;
        FurnitureCondition condition = Find(state, id);
        int cost = Cost(state, id);
        if (state == null || condition == null || condition.hp >= Config.MaxHp || state.coins < cost)
        {
            return false;
        }

        bool broken = condition.hp <= 0;
        state.coins -= cost;
        condition.hp = Config.MaxHp;
        condition.anchorUtc = DateTime.UtcNow.ToString("o");
        if (!broken || !ShopCatalog.TryFind(id, out ShopItem item))
        {
            return true;
        }

        PetActions.AddJoy(state, item.Effect);
        joy = item.Effect;
        return true;
    }

    static void SpendBreakJoy(GameState state, string id)
    {
        if (ShopCatalog.TryFind(id, out ShopItem item))
        {
            PetActions.SpendJoy(state, item.Effect);
        }
    }

    static FurnitureCondition Find(GameState state, string id)
    {
        if (state == null || state.furniture == null || string.IsNullOrEmpty(id))
        {
            return null;
        }

        for (int i = 0; i < state.furniture.Length; i++)
        {
            FurnitureCondition condition = state.furniture[i];
            if (condition != null && condition.id == id)
            {
                return condition;
            }
        }

        return null;
    }

    static void Add(GameState state, string id, int hp, string anchor)
    {
        if (state.furniture == null)
        {
            state.furniture = new FurnitureCondition[0];
        }

        var next = new FurnitureCondition[state.furniture.Length + 1];
        for (int i = 0; i < state.furniture.Length; i++)
        {
            next[i] = state.furniture[i];
        }

        next[state.furniture.Length] = new FurnitureCondition
        {
            id = id,
            hp = hp,
            anchorUtc = anchor
        };
        state.furniture = next;
    }

    static bool TryDue(string text, out DateTime due)
    {
        return DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.RoundtripKind, out due);
    }
}
