using System.IO;
using NUnit.Framework;

public class MiniGameProgressTests
{
    string _dir;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "monetok-tests", System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        GameSession.ResetForTests();
        GameSession.Initialize(new SaveService(_dir));
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
    public void AdvanceTo_RemembersNextLevelAndIgnoresOlderOnes()
    {
        MiniGameProgress.AdvanceTo("PiggyCatch", 1, 3);
        MiniGameProgress.AdvanceTo("PiggyCatch", 2, 3);
        MiniGameProgress.AdvanceTo("PiggyCatch", 1, 3);

        Assert.AreEqual(2, MiniGameProgress.ResumeIndex("PiggyCatch", 3));
        Assert.AreEqual(0, MiniGameProgress.ResumeIndex("Shop", 6));
    }

    [Test]
    public void AdvanceTo_SurvivesSaveAndLoad()
    {
        MiniGameProgress.AdvanceTo("BudgetEnvelopes", 2, 5);
        GameSession.Persist();
        GameSession.ResetForTests();
        GameSession.Initialize(new SaveService(_dir));

        Assert.AreEqual(2, MiniGameProgress.ResumeIndex("BudgetEnvelopes", 5));
    }

    [Test]
    public void ResumeIndex_ReplaysLastLevelWhenCampaignIsFinished()
    {
        MiniGameProgress.AdvanceTo("Shop", 6, 6);

        Assert.AreEqual(5, MiniGameProgress.ResumeIndex("Shop", 6));
    }

    [Test]
    public void Reset_StartsTheCampaignOver()
    {
        MiniGameProgress.AdvanceTo("Exchange", 4, 8);
        MiniGameProgress.Reset("Exchange");

        Assert.AreEqual(0, MiniGameProgress.ResumeIndex("Exchange", 8));
    }
}
