using System;
using UnityEngine;

[Serializable]
public class GameState
{
    public string petName = AppInfo.Title;
    public int coins = 100;
    public int hunger = 80;
    public string lastSaveUtc = "";

    public static GameState CreateDefault()
    {
        return new GameState
        {
            petName = AppInfo.Title,
            coins = 100,
            hunger = 80,
            lastSaveUtc = ""
        };
    }
}
