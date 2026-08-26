using System;
using System.Collections.Generic;

public sealed class NeedWantSession
{
    public const int RoundCount = 5;
    public const int CoinsPerCorrect = 3;

    readonly NeedWantItem[] _rounds;

    public NeedWantSession(IReadOnlyList<NeedWantItem> rounds)
    {
        if (rounds == null || rounds.Count == 0)
        {
            throw new ArgumentException("Need at least one round.", nameof(rounds));
        }

        _rounds = new NeedWantItem[rounds.Count];
        for (int i = 0; i < rounds.Count; i++)
        {
            _rounds[i] = rounds[i];
        }
    }

    public IReadOnlyList<NeedWantItem> Rounds => _rounds;
    public int TotalRounds => _rounds.Length;
    public int Index { get; private set; }
    public int Correct { get; private set; }
    public int Coins => Correct * CoinsPerCorrect;
    public bool IsFinished => Index >= _rounds.Length;
    public NeedWantItem Current => _rounds[Index];

    public static NeedWantSession Create(int seed)
    {
        var rng = new Random(seed);
        var needs = new List<NeedWantItem>();
        var wants = new List<NeedWantItem>();
        for (int i = 0; i < NeedWantCatalog.All.Length; i++)
        {
            NeedWantItem item = NeedWantCatalog.All[i];
            if (item.Kind == NeedWantKind.Need)
            {
                needs.Add(item);
            }
            else
            {
                wants.Add(item);
            }
        }

        Shuffle(needs, rng);
        Shuffle(wants, rng);

        int needCount = rng.Next(2, 4);
        int wantCount = RoundCount - needCount;
        var picked = new List<NeedWantItem>(RoundCount);
        for (int i = 0; i < needCount; i++)
        {
            picked.Add(needs[i]);
        }

        for (int i = 0; i < wantCount; i++)
        {
            picked.Add(wants[i]);
        }

        Shuffle(picked, rng);
        return new NeedWantSession(picked);
    }

    public bool TryAnswer(NeedWantKind choice)
    {
        if (IsFinished)
        {
            return false;
        }

        NeedWantItem item = Current;
        bool ok = item.Kind == choice;
        if (ok)
        {
            Correct++;
        }

        Index++;
        return ok;
    }

    static void Shuffle(List<NeedWantItem> list, Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            NeedWantItem tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
