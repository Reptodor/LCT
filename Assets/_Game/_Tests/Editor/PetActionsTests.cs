using NUnit.Framework;

public class PetActionsTests
{
    [Test]
    public void TryWork_AddsFifteenCoins()
    {
        var state = GameState.CreateDefault();
        int before = state.coins;

        bool ok = PetActions.TryWork(state);

        Assert.IsTrue(ok);
        Assert.AreEqual(before + 15, state.coins);
    }

    [Test]
    public void TryEarn_AddsGivenAmount()
    {
        var state = GameState.CreateDefault();
        int before = state.coins;

        bool ok = PetActions.TryEarn(state, 9);

        Assert.IsTrue(ok);
        Assert.AreEqual(before + 9, state.coins);
    }

    [Test]
    public void TryEarn_FailsWhenAmountIsNotPositive()
    {
        var state = GameState.CreateDefault();
        int before = state.coins;

        Assert.IsFalse(PetActions.TryEarn(state, 0));
        Assert.IsFalse(PetActions.TryEarn(state, -3));
        Assert.AreEqual(before, state.coins);
    }

    [Test]
    public void TryBuyFood_SpendsCostAndFeeds()
    {
        var state = GameState.CreateDefault();
        state.coins = 20;
        state.hunger = 50;

        bool ok = PetActions.TryBuyFood(state, 8, 12);

        Assert.IsTrue(ok);
        Assert.AreEqual(12, state.coins);
        Assert.AreEqual(62, state.hunger);
    }

    [Test]
    public void TryBuyFood_FailsWhenBroke()
    {
        var state = GameState.CreateDefault();
        state.coins = 7;
        state.hunger = 40;

        bool ok = PetActions.TryBuyFood(state, 8, 12);

        Assert.IsFalse(ok);
        Assert.AreEqual(7, state.coins);
        Assert.AreEqual(40, state.hunger);
    }

    [Test]
    public void TrySnack_SpendsTenAndFeedsWhenAffordable()
    {
        var state = GameState.CreateDefault();
        state.coins = 10;
        state.hunger = 80;

        bool ok = PetActions.TrySnack(state);

        Assert.IsTrue(ok);
        Assert.AreEqual(0, state.coins);
        Assert.AreEqual(95, state.hunger);
    }

    [Test]
    public void TrySnack_CapsHungerAt100()
    {
        var state = GameState.CreateDefault();
        state.coins = 50;
        state.hunger = 90;

        PetActions.TrySnack(state);

        Assert.AreEqual(100, state.hunger);
    }

    [Test]
    public void TrySnack_FailsAndDoesNotChangeStateWhenBroke()
    {
        var state = GameState.CreateDefault();
        state.coins = 9;
        state.hunger = 40;

        bool ok = PetActions.TrySnack(state);

        Assert.IsFalse(ok);
        Assert.AreEqual(9, state.coins);
        Assert.AreEqual(40, state.hunger);
    }
}
