public static class GameSession
{
    public static GameState State { get; private set; }
    public static SaveService Saves { get; private set; }
    public static bool IsReady => State != null;

    public static void Initialize(SaveService saves)
    {
        Saves = saves;
        State = saves.LoadOrCreateDefault();
    }

    public static void Persist()
    {
        if (State == null || Saves == null)
        {
            return;
        }

        Saves.Save(State);
    }

    public static void ResetForTests()
    {
        State = null;
        Saves = null;
    }
}
