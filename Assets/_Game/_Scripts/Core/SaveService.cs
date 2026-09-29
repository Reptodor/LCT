using System;
using System.IO;
using UnityEngine;

public sealed class SaveService
{
    public const string FileName = "monetok-save.json";
    public const string ProfilesFolder = "profiles";

    readonly string _filePath;

    public SaveService(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Save directory is required.", nameof(directory));
        }

        _filePath = Path.Combine(directory, FileName);
    }

    public static SaveService CreateDefault()
    {
        return new SaveService(Application.persistentDataPath);
    }

    public static SaveService ForProfile(string directory, string profileId)
    {
        return new SaveService(ProfileDirectory(directory, profileId));
    }

    public static string ProfileDirectory(string directory, string profileId)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Save directory is required.", nameof(directory));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Profile id is required.", nameof(profileId));
        }

        return Path.Combine(directory, ProfilesFolder, ToFileKey(profileId));
    }

    public static string ToFileKey(string profileId)
    {
        string key = (profileId ?? string.Empty).Trim().ToLowerInvariant();
        char[] invalid = Path.GetInvalidFileNameChars();
        var builder = new System.Text.StringBuilder(key.Length);
        for (int i = 0; i < key.Length; i++)
        {
            char c = key[i];
            bool bad = c < 32 || Array.IndexOf(invalid, c) >= 0;
            builder.Append(bad ? '_' : c);
        }

        return builder.Length == 0 ? "profile" : builder.ToString();
    }

    // The old install kept one save for the whole device. The first profile to sign in takes it.
    public static void AdoptLegacySave(string directory, string profileId, string reservedForProfileId = null)
    {
        string profileFile = Path.Combine(ProfileDirectory(directory, profileId), FileName);
        if (File.Exists(profileFile))
        {
            return;
        }

        string legacy = Path.Combine(directory, FileName);
        string marker = Path.Combine(directory, ProfilesFolder, ".legacy-claimed");
        if (!File.Exists(legacy) || File.Exists(marker))
        {
            return;
        }

        if (!string.IsNullOrEmpty(reservedForProfileId)
            && ToFileKey(reservedForProfileId) != ToFileKey(profileId))
        {
            return;
        }

        string profileDirectory = Path.GetDirectoryName(profileFile);
        if (!string.IsNullOrEmpty(profileDirectory))
        {
            Directory.CreateDirectory(profileDirectory);
        }

        File.Copy(legacy, profileFile);
        string markerDirectory = Path.GetDirectoryName(marker);
        if (!string.IsNullOrEmpty(markerDirectory))
        {
            Directory.CreateDirectory(markerDirectory);
        }

        File.WriteAllText(marker, ToFileKey(profileId));
    }

    public GameState LoadOrCreateDefault()
    {
        if (!File.Exists(_filePath))
        {
            return GameState.CreateDefault();
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            GameState state = JsonUtility.FromJson<GameState>(json);
            if (state == null || string.IsNullOrWhiteSpace(state.petName))
            {
                return GameState.CreateDefault();
            }

            if (state.levelProgress == null)
            {
                state.levelProgress = new LevelProgress[0];
            }

            if (state.savings == null)
            {
                state.savings = new SavingsPot[0];
            }

            if (state.savingsGoalId == null)
            {
                state.savingsGoalId = "";
            }

            return state;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Monetok] Save unreadable, using defaults: {ex.Message}");
            return GameState.CreateDefault();
        }
    }

    public void Delete()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }

            string directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory)
                && Directory.Exists(directory)
                && Directory.GetFileSystemEntries(directory).Length == 0)
            {
                Directory.Delete(directory);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Monetok] Failed to delete save: {ex.Message}");
        }
    }

    public void Save(GameState state)
    {
        if (state == null)
        {
            return;
        }

        try
        {
            state.lastSaveUtc = DateTime.UtcNow.ToString("o");
            string directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_filePath, JsonUtility.ToJson(state, true));
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Monetok] Failed to write save: {ex.Message}");
        }
    }
}
