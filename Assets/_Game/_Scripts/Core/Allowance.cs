using System;
using UnityEngine;

public static class Allowance
{
    const string ResourceName = "AllowanceConfig";

    static AllowanceConfig _config;

    public static AllowanceConfig Config
    {
        get
        {
            if (_config == null)
            {
                _config = Resources.Load<AllowanceConfig>(ResourceName);
            }

            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<AllowanceConfig>();
            }

            return _config;
        }
    }

    public static bool EnsureSchedule(GameState state)
    {
        if (state == null || HasDue(state))
        {
            return false;
        }

        state.nextAllowanceUtc = DueFromNow();
        return true;
    }

    public static float RemainingSeconds(GameState state)
    {
        if (state == null || !TryDue(state.nextAllowanceUtc, out DateTime due))
        {
            return Config.IntervalSeconds;
        }

        return (float)(due - DateTime.UtcNow).TotalSeconds;
    }

    public static void FinishWait(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.nextAllowanceUtc = DateTime.UtcNow.AddSeconds(-1).ToString("o");
    }

    public static void Grant(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.coins += Config.Amount;
        state.nextAllowanceUtc = DueFromNow();
    }

    public static string FormatRemaining(float seconds)
    {
        int total = Mathf.CeilToInt(Mathf.Max(0f, seconds));
        int hours = total / 3600;
        int minutes = (total % 3600) / 60;
        int secs = total % 60;
        if (hours > 0)
        {
            return hours + ":" + minutes.ToString("00") + ":" + secs.ToString("00");
        }

        return minutes.ToString("00") + ":" + secs.ToString("00");
    }

    static bool HasDue(GameState state)
    {
        return TryDue(state.nextAllowanceUtc, out _);
    }

    static bool TryDue(string text, out DateTime due)
    {
        return DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.RoundtripKind, out due);
    }

    static string DueFromNow()
    {
        return DateTime.UtcNow.AddSeconds(Config.IntervalSeconds).ToString("o");
    }
}
