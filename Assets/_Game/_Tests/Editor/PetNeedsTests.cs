using System;
using NUnit.Framework;

public class PetNeedsTests
{
    [Test]
    public void CatchUp_SchedulesHungerWithoutDrainingYet()
    {
        var state = GameState.CreateDefault();
        state.hunger = 40;
        state.nextHungerUtc = "";

        Assert.IsTrue(Hunger.CatchUp(state));
        Assert.AreEqual(40, state.hunger);
        Assert.IsFalse(string.IsNullOrEmpty(state.nextHungerUtc));
    }

    [Test]
    public void CatchUp_WaitsUntilTheInterval()
    {
        var state = GameState.CreateDefault();
        state.hunger = 40;
        state.nextHungerUtc = DateTime.UtcNow.AddSeconds(-1).ToString("o");

        Assert.IsFalse(Hunger.CatchUp(state));
        Assert.AreEqual(40, state.hunger);
    }

    [Test]
    public void CatchUp_DropsAStaleDeadlineAndFollowsTheCurrentInterval()
    {
        var state = GameState.CreateDefault();
        state.hunger = 40;
        state.nextHungerUtc = DateTime.UtcNow.AddMinutes(5).ToString("o");

        Assert.IsTrue(Hunger.CatchUp(state));
        Assert.AreEqual(40, state.hunger);

        state.nextHungerUtc = DateTime.UtcNow.AddSeconds(-(Hunger.Config.IntervalSeconds + 0.5f)).ToString("o");
        Assert.IsTrue(Hunger.CatchUp(state));
        Assert.AreEqual(40 - Hunger.Config.Amount, state.hunger);
    }

    [Test]
    public void CatchUp_DrainsOnceWhenTheIntervalHasPassed()
    {
        var state = GameState.CreateDefault();
        state.hunger = 40;
        state.nextHungerUtc = DateTime.UtcNow.AddSeconds(-(Hunger.Config.IntervalSeconds + 0.5f)).ToString("o");

        Assert.IsTrue(Hunger.CatchUp(state));
        Assert.AreEqual(40 - Hunger.Config.Amount, state.hunger);
    }

    [Test]
    public void CatchUp_DrainsEveryMissedIntervalAndStopsAtZero()
    {
        var state = GameState.CreateDefault();
        state.hunger = 100;
        float interval = Hunger.Config.IntervalSeconds;
        state.nextHungerUtc = DateTime.UtcNow.AddSeconds(-(interval * 2f + 1f)).ToString("o");

        Hunger.CatchUp(state);

        Assert.AreEqual(100 - Hunger.Config.Amount * 2, state.hunger);

        state.hunger = 1;
        state.nextHungerUtc = DateTime.UtcNow.AddSeconds(-(interval + 0.5f)).ToString("o");
        Hunger.CatchUp(state);
        Assert.AreEqual(0, state.hunger);
    }

    [Test]
    public void AddJoy_GrowsFromFurnitureAndCapsAt100()
    {
        var state = GameState.CreateDefault();
        state.joy = 97;

        PetActions.AddJoy(state, 6);

        Assert.AreEqual(100, state.joy);
    }

    [Test]
    public void SpendJoy_GoesDownAndStopsAtZero()
    {
        var state = GameState.CreateDefault();
        state.joy = 4;

        PetActions.SpendJoy(state, 10);

        Assert.AreEqual(0, state.joy);
        PetActions.SpendJoy(state, 0);
        Assert.AreEqual(0, state.joy);
    }

    [Test]
    public void FurniturePurchase_AddsItsJoyAndFoodDoesNot()
    {
        var state = GameState.CreateDefault();
        state.coins = 200;
        state.joy = 10;
        var chair = new ShopItem("LoungeChair", "Кресло", 70, 2, ShopCategory.Optional, "MainRoom", "Гостиная");
        var apple = new ShopItem("apple", "Яблоко", 8, 12, ShopCategory.Required);

        Assert.IsTrue(ShopCheckout.TryPay(state, chair));
        Assert.AreEqual(12, state.joy);
        Assert.IsTrue(ShopCheckout.Owns(state, "LoungeChair"));

        Assert.IsTrue(ShopCheckout.TryPay(state, apple));
        Assert.AreEqual(12, state.joy);
        Assert.AreEqual(92, state.hunger);
    }
}
