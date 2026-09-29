using System;
using UnityEngine;

public static class Hunger
{
    const string ResourceName = "HungerConfig";

    static HungerConfig _config;

    public static HungerConfig Config
    {
        get
        {
            if (_config == null)
            {
                _config = Resources.Load<HungerConfig>(ResourceName);
            }

            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<HungerConfig>();
            }

            return _config;
        }
    }

    public static bool CatchUp(GameState state)
    {
        if (state == null)
        {
            return false;
        }

        DateTime now = DateTime.UtcNow;
        float interval = Config.IntervalSeconds;
        bool changed = false;
        if (!TryDue(state.nextHungerUtc, out DateTime anchor) || anchor > now)
        {
            anchor = now;
            state.nextHungerUtc = now.ToString("o");
            changed = true;
        }

        double elapsed = (now - anchor).TotalSeconds;
        if (elapsed < interval)
        {
            return changed;
        }

        int steps = Math.Max(1, (int)(elapsed / interval));
        state.hunger = Mathf.Max(0, state.hunger - steps * Config.Amount);
        state.nextHungerUtc = anchor.AddSeconds(interval * steps).ToString("o");
        return true;
    }

    static bool TryDue(string text, out DateTime due)
    {
        return DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.RoundtripKind, out due);
    }
}
