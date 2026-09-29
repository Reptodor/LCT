using UnityEngine;
using UnityEngine.SceneManagement;

public static class MiniGameProgress
{
    public static int ResumeIndex(int levelCount)
    {
        return ResumeIndex(SceneManager.GetActiveScene().name, levelCount);
    }

    public static int ResumeIndex(string sceneName, int levelCount)
    {
        if (levelCount <= 1)
        {
            return 0;
        }

        int next = Read(sceneName);
        if (next <= 0)
        {
            return 0;
        }

        if (next >= levelCount)
        {
            return levelCount - 1;
        }

        return next;
    }

    public static void AdvanceTo(int nextIndex, int levelCount)
    {
        AdvanceTo(SceneManager.GetActiveScene().name, nextIndex, levelCount);
    }

    public static void AdvanceTo(string sceneName, int nextIndex, int levelCount)
    {
        if (!GameSession.IsReady || levelCount <= 0 || string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        int stored = Read(sceneName);
        int next = Mathf.Clamp(nextIndex, 0, levelCount);
        if (next <= stored)
        {
            return;
        }

        Write(sceneName, next);
        GameSession.Persist();
    }

    public static void ResetActive()
    {
        Reset(SceneManager.GetActiveScene().name);
    }

    public static void Reset(string sceneName)
    {
        if (!GameSession.IsReady || string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        if (Read(sceneName) == 0 && Find(sceneName) < 0)
        {
            return;
        }

        Write(sceneName, 0);
        GameSession.Persist();
    }

    private static int Read(string sceneName)
    {
        int index = Find(sceneName);
        if (index < 0)
        {
            return 0;
        }

        return GameSession.State.levelProgress[index].nextIndex;
    }

    private static int Find(string sceneName)
    {
        if (!GameSession.IsReady || string.IsNullOrEmpty(sceneName))
        {
            return -1;
        }

        LevelProgress[] entries = GameSession.State.levelProgress;
        if (entries == null)
        {
            return -1;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            LevelProgress entry = entries[i];
            if (entry != null && entry.scene == sceneName)
            {
                return i;
            }
        }

        return -1;
    }

    private static void Write(string sceneName, int nextIndex)
    {
        GameState state = GameSession.State;
        int index = Find(sceneName);
        if (index >= 0)
        {
            state.levelProgress[index].nextIndex = nextIndex;
            return;
        }

        LevelProgress[] entries = state.levelProgress ?? new LevelProgress[0];
        var grown = new LevelProgress[entries.Length + 1];
        for (int i = 0; i < entries.Length; i++)
        {
            grown[i] = entries[i];
        }

        grown[entries.Length] = new LevelProgress
        {
            scene = sceneName,
            nextIndex = nextIndex
        };
        state.levelProgress = grown;
    }
}
