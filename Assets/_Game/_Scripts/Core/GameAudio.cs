using System;
using UnityEngine;

public static class GameAudio
{
    public static event Action Changed;

    public static float SoundVolume { get; private set; } = 1f;
    public static float MusicVolume { get; private set; } = 1f;
    public static bool SoundOn => SoundVolume > 0.001f;
    public static bool MusicOn => MusicVolume > 0.001f;

    public static void Reload()
    {
        string profile = ProfileKey();
        SoundVolume = ReadVolume("sound", profile);
        MusicVolume = ReadVolume("music", profile);
        if (Changed != null)
        {
            Changed();
        }
    }

    public static void SetSoundVolume(float volume)
    {
        SoundVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(Key("sound.vol", ProfileKey()), SoundVolume);
        PlayerPrefs.Save();
        if (Changed != null)
        {
            Changed();
        }
    }

    public static void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(Key("music.vol", ProfileKey()), MusicVolume);
        PlayerPrefs.Save();
        if (Changed != null)
        {
            Changed();
        }
    }

    static float ReadVolume(string channel, string profile)
    {
        string volumeKey = Key(channel + ".vol", profile);
        if (PlayerPrefs.HasKey(volumeKey))
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(volumeKey, 1f));
        }

        string legacy = Key(channel, profile);
        if (PlayerPrefs.HasKey(legacy))
        {
            return PlayerPrefs.GetInt(legacy, 1) > 0 ? 1f : 0f;
        }

        return 1f;
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
