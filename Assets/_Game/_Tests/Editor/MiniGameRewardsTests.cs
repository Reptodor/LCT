using NUnit.Framework;
using UnityEngine;

public class MiniGameRewardsTests
{
    [Test]
    public void AmountFor_UsesCoinsOfMatchingScene()
    {
        MiniGameRewards rewards = ScriptableObject.CreateInstance<MiniGameRewards>();

        Assert.AreEqual(10, rewards.AmountFor("Shop"));
        Assert.AreEqual(10, rewards.AmountFor("BudgetEnvelopes"));

        Object.DestroyImmediate(rewards);
    }

    [Test]
    public void AmountFor_UsesDefaultWhenSceneIsMissing()
    {
        MiniGameRewards rewards = ScriptableObject.CreateInstance<MiniGameRewards>();

        Assert.AreEqual(rewards.DefaultCoins, rewards.AmountFor("UnknownScene"));

        Object.DestroyImmediate(rewards);
    }
}
