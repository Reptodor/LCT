public static class GameSession
{
    public static GameState State { get; private set; }
    public static SaveService Saves { get; private set; }
    public static string ProfileId { get; private set; }
    public static bool IsReady => State != null;

    public static void Initialize(SaveService saves)
    {
        Saves = saves;
        State = saves.LoadOrCreateDefault();
        ProfileId = null;
    }

    public static void BindProfile(string directory, string profileId, string legacyOwnerId = null)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new System.ArgumentException("Profile id is required.", nameof(profileId));
        }

        Persist();
        string normalized = profileId.Trim().ToLowerInvariant();
        SaveService.AdoptLegacySave(directory, normalized, legacyOwnerId);
        ProfileId = normalized;
        Saves = SaveService.ForProfile(directory, normalized);
        State = Saves.LoadOrCreateDefault();
    }

    public static void Persist()
    {
        if (State == null || Saves == null)
        {
            return;
        }

        Saves.Save(State);
    }

    public static void Unload()
    {
        Persist();
        State = null;
        Saves = null;
        ProfileId = null;
    }

    public static void ResetForTests()
    {
        State = null;
        Saves = null;
        ProfileId = null;
    }
}
