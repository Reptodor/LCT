using UnityEngine;

public static class GameSession
{
    public static GameState State { get; private set; }
    public static SaveService Saves { get; private set; }
    public static string ProfileId { get; private set; }
    public static bool IsReady => State != null;

    public static bool IsLocalSession =>
        ProfileId == AuthService.GuestProfileId || ProfileId == AuthService.DemoProfileId;

    public static bool IsDemo => ProfileId == AuthService.DemoProfileId;

    public static void Initialize(SaveService saves)
    {
        Saves = saves;
        State = saves.LoadOrCreateDefault();
        ProfileId = null;
        GameAudio.Reload();
    }

    public static void BindProfile(string directory, string profileId, string legacyOwnerId = null)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new System.ArgumentException("Profile id is required.", nameof(profileId));
        }

        Persist();
        string normalized = profileId.Trim().ToLowerInvariant();
        bool local = normalized == AuthService.GuestProfileId || normalized == AuthService.DemoProfileId;
        if (!local)
        {
            SaveService.AdoptLegacySave(directory, normalized, legacyOwnerId);
        }

        ProfileId = normalized;
        Saves = SaveService.ForProfile(directory, normalized);
        State = Saves.LoadOrCreateDefault();
        GameAudio.Reload();
    }

    public static void Persist()
    {
        if (State == null || Saves == null)
        {
            return;
        }

        Saves.Save(State);
    }

    public static void ResetProgress()
    {
        if (Saves == null)
        {
            return;
        }

        string shopKey = ShopSavingsKey();
        State = GameState.CreateDefault();
        Persist();
        PlayerPrefs.DeleteKey(shopKey);
        PlayerPrefs.Save();
    }

    public static string ShopSavingsKey()
    {
        if (string.IsNullOrEmpty(ProfileId))
        {
            return "TotalSaved";
        }

        return "TotalSaved_" + SaveService.ToFileKey(ProfileId);
    }

    public static void Unload()
    {
        Persist();
        State = null;
        Saves = null;
        ProfileId = null;
        GameAudio.Reload();
    }

    public static void Discard()
    {
        string shopKey = ShopSavingsKey();
        if (Saves != null)
        {
            Saves.Delete();
        }

        PlayerPrefs.DeleteKey(shopKey);
        PlayerPrefs.Save();
        State = null;
        Saves = null;
        ProfileId = null;
        GameAudio.Reload();
    }

    public static void ResetForTests()
    {
        State = null;
        Saves = null;
        ProfileId = null;
    }
}
