using System;
using System.IO;
using UnityEngine;

public sealed class SaveService
{
    public const string FileName = "monetok-save.json";

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

            return state;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Monetok] Save unreadable, using defaults: {ex.Message}");
            return GameState.CreateDefault();
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
