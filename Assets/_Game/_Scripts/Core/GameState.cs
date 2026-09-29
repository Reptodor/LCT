using System;

[Serializable]
public class LevelProgress
{
    public string scene = "";
    public int nextIndex;
}

[Serializable]
public class GameState
{
    public string petName = AppInfo.Title;
    public int coins = 100;
    public int hunger = 80;
    public string lastSaveUtc = "";
    public LevelProgress[] levelProgress = new LevelProgress[0];

    public static GameState CreateDefault()
    {
        return new GameState
        {
            petName = AppInfo.Title,
            coins = 100,
            hunger = 80,
            lastSaveUtc = "",
            levelProgress = new LevelProgress[0]
        };
    }
}
