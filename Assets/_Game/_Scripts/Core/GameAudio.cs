using System;
using UnityEngine;

public static class GameAudio
{
    public static event Action Changed;

    public static bool SoundOn { get; private set; } = true;
    public static bool MusicOn { get; private set; } = true;

    public static void Reload()
    {
        string profile = ProfileKey();
        SoundOn = PlayerPrefs.GetInt(Key("sound", profile), 1) == 1;
        MusicOn = PlayerPrefs.GetInt(Key("music", profile), 1) == 1;
        if (Changed != null)
        {
            Changed();
        }
    }

    public static void SetSound(bool on)
    {
        SoundOn = on;
        PlayerPrefs.SetInt(Key("sound", ProfileKey()), on ? 1 : 0);
        PlayerPrefs.Save();
        if (Changed != null)
        {
            Changed();
        }
    }

    public static void SetMusic(bool on)
    {
        MusicOn = on;
        PlayerPrefs.SetInt(Key("music", ProfileKey()), on ? 1 : 0);
        PlayerPrefs.Save();
        if (Changed != null)
        {
            Changed();
        }
    }

    public static string ProfileKey()
    {
        if (string.IsNullOrEmpty(GameSession.ProfileId))
        {
            return "device";
        }

        return SaveService.ToFileKey(GameSession.ProfileId);
    }

    static string Key(string channel, string profile)
    {
        return "finashka." + channel + "." + profile;
    }
}
