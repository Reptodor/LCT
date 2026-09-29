using System;
using NUnit.Framework;

public class FurnitureWearTests
{
    [Test]
    public void CatchUp_StartsOwnedFurnitureAtFullHealth()
    {
        var state = GameState.CreateDefault();
        state.ownedItems = new[] { "Bed" };

        Assert.IsTrue(FurnitureWear.CatchUp(state, out string[] broke));
        Assert.IsNull(broke);
        Assert.AreEqual(FurnitureWear.Config.MaxHp, FurnitureWear.Hp(state, "Bed"));
        Assert.IsFalse(FurnitureWear.IsBroken(state, "Bed"));
    }

    [Test]
    public void CatchUp_DamagesOnTheConfiguredInterval()
    {
        var state = OwnedBed();
        FurnitureCondition bed = state.furniture[0];
        bed.hp = FurnitureWear.Config.MaxHp;
        bed.anchorUtc = DateTime.UtcNow.AddSeconds(-(FurnitureWear.StepSeconds + 0.5f)).ToString("o");

        Assert.IsTrue(FurnitureWear.CatchUp(state, out string[] broke));
        Assert.IsNull(broke);
        Assert.AreEqual(FurnitureWear.Config.MaxHp - 1, FurnitureWear.Hp(state, "Bed"));
    }

    [Test]
    public void Break_DropsJoyOnce_RepairOfBrokenRaisesItLikeAPurchase()
    {
        var state = OwnedBed();
        state.joy = 20;
        FurnitureCondition bed = state.furniture[0];
        bed.hp = 1;
        bed.anchorUtc = DateTime.UtcNow.AddSeconds(-(FurnitureWear.StepSeconds + 0.5f)).ToString("o");

        FurnitureWear.CatchUp(state, out string[] broke);

        Assert.AreEqual(0, FurnitureWear.Hp(state, "Bed"));
        Assert.IsTrue(FurnitureWear.IsBroken(state, "Bed"));
        Assert.AreEqual(1, broke.Length);
        Assert.AreEqual(16, state.joy);

        bed.anchorUtc = DateTime.UtcNow.AddSeconds(-(FurnitureWear.StepSeconds + 0.5f)).ToString("o");
        FurnitureWear.CatchUp(state, out broke);
        Assert.AreEqual(16, state.joy);

        Assert.IsTrue(FurnitureWear.TryRepair(state, "Bed", out int joy));
        Assert.AreEqual(4, joy);
        Assert.AreEqual(20, state.joy);
        Assert.AreEqual(FurnitureWear.Config.MaxHp, FurnitureWear.Hp(state, "Bed"));
        Assert.IsFalse(FurnitureWear.IsBroken(state, "Bed"));
    }

    [Test]
    public void Repair_OfDamagedFurnitureDoesNotGrantJoy()
    {
        var state = OwnedBed();
        state.joy = 20;
        state.furniture[0].hp = FurnitureWear.Config.MaxHp - 1;

        int coins = state.coins;
        Assert.IsTrue(FurnitureWear.TryRepair(state, "Bed", out int joy));
        Assert.AreEqual(0, joy);
        Assert.Less(state.coins, coins);
        Assert.AreEqual(20, state.joy);
        Assert.AreEqual(FurnitureWear.Config.MaxHp, FurnitureWear.Hp(state, "Bed"));
    }

    [Test]
    public void Cost_IsZeroWhenWholeAndMaxWhenBroken()
    {
        var state = OwnedBed();
        Assert.AreEqual(0, FurnitureWear.Cost(state, "Bed"));

        state.furniture[0].hp = 0;
        Assert.AreEqual(FurnitureWear.RepairConfig.MaxCost, FurnitureWear.Cost(state, "Bed"));
    }

    [Test]
    public void Cost_GrowsAsHealthDrops()
    {
        var state = OwnedBed();
        state.furniture[0].hp = FurnitureWear.Config.MaxHp - 1;
        int light = FurnitureWear.Cost(state, "Bed");
        state.furniture[0].hp = 1;
        int heavy = FurnitureWear.Cost(state, "Bed");

        Assert.Greater(light, 0);
        Assert.Greater(heavy, light);
        Assert.Less(heavy, FurnitureWear.RepairConfig.MaxCost);
    }

    [Test]
    public void TryRepair_RefusesWhenCoinsAreShort()
    {
        var state = OwnedBed();
        state.furniture[0].hp = 0;
        int cost = FurnitureWear.Cost(state, "Bed");
        state.coins = cost - 1;

        Assert.IsFalse(FurnitureWear.TryRepair(state, "Bed", out int joy));
        Assert.AreEqual(0, joy);
        Assert.AreEqual(0, FurnitureWear.Hp(state, "Bed"));
        Assert.AreEqual(cost - 1, state.coins);
    }

    static GameState OwnedBed()
    {
        var state = GameState.CreateDefault();
        state.ownedItems = new[] { "Bed" };
        state.furniture = new[]
        {
            new FurnitureCondition
            {
                id = "Bed",
                hp = FurnitureWear.Config.MaxHp,
                anchorUtc = DateTime.UtcNow.ToString("o")
            }
        };
        return state;
    }
}
