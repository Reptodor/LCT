using NUnit.Framework;

public class SavingsBankTests
{
    [Test]
    public void Deposit_MovesCoinsFromBalanceIntoTheGoal()
    {
        GameState state = GameState.CreateDefault();
        state.coins = 100;
        SavingsBank.Select(state, "bike");

        int paid = SavingsBank.Deposit(state, "bike", 500, 25);

        Assert.AreEqual(25, paid);
        Assert.AreEqual(75, state.coins);
        Assert.AreEqual(25, SavingsBank.Saved(state, "bike"));
        Assert.AreEqual("bike", state.savingsGoalId);
    }

    [Test]
    public void Deposit_StopsAtTheTarget()
    {
        GameState state = GameState.CreateDefault();
        state.coins = 100;
        SavingsBank.Select(state, "headphones");

        int paid = SavingsBank.Deposit(state, "headphones", 30, 25);
        int extra = SavingsBank.Deposit(state, "headphones", 30, 25);

        Assert.AreEqual(25, paid);
        Assert.AreEqual(5, extra);
        Assert.AreEqual(30, SavingsBank.Saved(state, "headphones"));
        Assert.AreEqual(70, state.coins);
    }

    [Test]
    public void Deposit_FailsWhenBalanceIsShort()
    {
        GameState state = GameState.CreateDefault();
        state.coins = 10;
        SavingsBank.Select(state, "trip");

        int paid = SavingsBank.Deposit(state, "trip", 800, 50);

        Assert.AreEqual(0, paid);
        Assert.AreEqual(10, state.coins);
        Assert.AreEqual(0, SavingsBank.Saved(state, "trip"));
    }

    [Test]
    public void Select_RemembersTheChosenGoal()
    {
        GameState state = GameState.CreateDefault();

        SavingsBank.Select(state, "trip");

        Assert.AreEqual("trip", state.savingsGoalId);
        Assert.AreEqual(0, SavingsBank.Saved(state, "trip"));
    }

    [Test]
    public void Deposit_FailsUntilTheGoalIsConfirmed()
    {
        GameState state = GameState.CreateDefault();
        state.coins = 100;

        int paid = SavingsBank.Deposit(state, "bike", 500, 25);

        Assert.AreEqual(0, paid);
        Assert.AreEqual(100, state.coins);
    }

    [Test]
    public void CanSwitchGoal_OnlyAfterTheConfirmedGoalIsFilled()
    {
        GameState state = GameState.CreateDefault();
        SavingsBank.Select(state, "bike");

        Assert.IsFalse(SavingsBank.CanSwitchGoal(state, 500));

        SavingsBank.Deposit(state, "bike", 500, 500);

        Assert.IsTrue(SavingsBank.CanSwitchGoal(state, 500));
    }
}
