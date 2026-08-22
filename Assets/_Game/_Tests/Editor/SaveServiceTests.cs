using System.IO;
using NUnit.Framework;
using UnityEngine;

public class SaveServiceTests
{
    string _dir;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "monetok-tests", System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }

    [Test]
    public void LoadOrCreateDefault_ReturnsDefaultsWhenFileMissing()
    {
        var saves = new SaveService(_dir);
        GameState state = saves.LoadOrCreateDefault();

        Assert.AreEqual(AppInfo.Title, state.petName);
        Assert.AreEqual(100, state.coins);
        Assert.AreEqual(80, state.hunger);
    }

    [Test]
    public void SaveThenLoad_RestoresCoins()
    {
        var saves = new SaveService(_dir);
        var state = GameState.CreateDefault();
        state.coins = 130;
        saves.Save(state);

        GameState loaded = new SaveService(_dir).LoadOrCreateDefault();

        Assert.AreEqual(130, loaded.coins);
        Assert.AreEqual(AppInfo.Title, loaded.petName);
        Assert.IsFalse(string.IsNullOrEmpty(loaded.lastSaveUtc));
    }

    [Test]
    public void LoadOrCreateDefault_ReturnsDefaultsWhenJsonCorrupt()
    {
        File.WriteAllText(Path.Combine(_dir, SaveService.FileName), "{not-json");

        var saves = new SaveService(_dir);
        GameState state = saves.LoadOrCreateDefault();

        Assert.AreEqual(100, state.coins);
        Assert.AreEqual(AppInfo.Title, state.petName);
    }
}
