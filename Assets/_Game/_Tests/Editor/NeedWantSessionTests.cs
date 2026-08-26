using NUnit.Framework;

public class NeedWantSessionTests
{
    [Test]
    public void Create_MakesFiveRounds()
    {
        var session = NeedWantSession.Create(1);

        Assert.AreEqual(5, NeedWantSession.RoundCount);
        Assert.AreEqual(NeedWantSession.RoundCount, session.TotalRounds);
        Assert.AreEqual(0, session.Index);
        Assert.IsFalse(session.IsFinished);
    }

    [Test]
    public void Create_SameSeed_SameTitles()
    {
        var a = NeedWantSession.Create(42);
        var b = NeedWantSession.Create(42);

        Assert.AreEqual(a.Current.Title, b.Current.Title);
        for (int i = 0; i < a.TotalRounds; i++)
        {
            Assert.AreEqual(a.Rounds[i].Title, b.Rounds[i].Title);
        }
    }

    [Test]
    public void Create_IncludesNeedsAndWants()
    {
        var session = NeedWantSession.Create(7);
        int needs = 0;
        int wants = 0;
        for (int i = 0; i < session.TotalRounds; i++)
        {
            if (session.Rounds[i].Kind == NeedWantKind.Need)
            {
                needs++;
            }
            else
            {
                wants++;
            }
        }

        Assert.GreaterOrEqual(needs, 2);
        Assert.GreaterOrEqual(wants, 2);
    }

    [Test]
    public void CorrectAnswer_AddsThreeCoinsAndAdvances()
    {
        var item = new NeedWantItem("Хлеб", "Еда.", "Нужно, это еда.", NeedWantKind.Need);
        var session = new NeedWantSession(new[] { item, item });

        bool ok = session.TryAnswer(NeedWantKind.Need);

        Assert.IsTrue(ok);
        Assert.AreEqual(1, session.Correct);
        Assert.AreEqual(3, session.Coins);
        Assert.AreEqual(1, session.Index);
    }

    [Test]
    public void WrongAnswer_NoCoinsButAdvances()
    {
        var item = new NeedWantItem("Конфеты", "Сладкое.", "Это хотелка.", NeedWantKind.Want);
        var session = new NeedWantSession(new[] { item });

        bool ok = session.TryAnswer(NeedWantKind.Need);

        Assert.IsFalse(ok);
        Assert.AreEqual(0, session.Correct);
        Assert.AreEqual(0, session.Coins);
        Assert.IsTrue(session.IsFinished);
    }

    [Test]
    public void TryAnswer_AfterFinish_ReturnsFalse()
    {
        var item = new NeedWantItem("Хлеб", "Еда.", "Нужно.", NeedWantKind.Need);
        var session = new NeedWantSession(new[] { item });
        session.TryAnswer(NeedWantKind.Need);

        bool ok = session.TryAnswer(NeedWantKind.Need);

        Assert.IsFalse(ok);
        Assert.AreEqual(1, session.Correct);
    }
}
