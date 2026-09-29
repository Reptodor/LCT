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
        GameSession.ResetForTests();
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

    [Test]
    public void Load_OldSaveWithoutLevelProgress_KeepsCoins()
    {
        string json = "{\"petName\":\"Финашка\",\"coins\":40,\"hunger\":70,\"lastSaveUtc\":\"x\"}";
        File.WriteAllText(Path.Combine(_dir, SaveService.FileName), json);

        GameState state = new SaveService(_dir).LoadOrCreateDefault();

        Assert.AreEqual(40, state.coins);
        Assert.IsNotNull(state.levelProgress);
        Assert.AreEqual(0, state.levelProgress.Length);
    }

    [Test]
    public void BindProfile_KeepsProgressSeparatePerProfile()
    {
        GameSession.BindProfile(_dir, "a@mail.com");
        GameSession.State.coins = 15;

        GameSession.BindProfile(_dir, "b@mail.com");
        Assert.AreEqual(100, GameSession.State.coins);
        GameSession.State.coins = 80;

        GameSession.BindProfile(_dir, "a@mail.com");
        Assert.AreEqual(15, GameSession.State.coins);

        GameSession.Unload();
        GameSession.BindProfile(_dir, "b@mail.com");
        Assert.AreEqual(80, GameSession.State.coins);
    }

    [Test]
    public void BindProfile_GivesLegacyDeviceSaveOnlyToItsOwner()
    {
        string json = "{\"petName\":\"Финашка\",\"coins\":40,\"hunger\":70,\"lastSaveUtc\":\"x\"}";
        File.WriteAllText(Path.Combine(_dir, SaveService.FileName), json);

        GameSession.BindProfile(_dir, "new@mail.com", "old@mail.com");
        Assert.AreEqual(100, GameSession.State.coins);

        GameSession.BindProfile(_dir, "old@mail.com", "old@mail.com");
        Assert.AreEqual(40, GameSession.State.coins);

        GameSession.BindProfile(_dir, "new@mail.com", "old@mail.com");
        Assert.AreEqual(100, GameSession.State.coins);
    }
}
