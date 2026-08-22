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
