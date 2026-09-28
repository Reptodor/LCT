using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MiniGameRewards", menuName = "Finashka/Награды мини-игр")]
public sealed class MiniGameRewards : ScriptableObject
{
    [SerializeField] private int _defaultCoins = 10;

    [SerializeField] private Entry[] _games =
    {
        new Entry("BudgetEnvelopes", 10),
        new Entry("BudgetWeek", 10),
        new Entry("DreamGoal", 10),
        new Entry("PiggyCatch", 10),
        new Entry("Exchange", 10),
        new Entry("Shop", 10)
    };

    public int DefaultCoins => Mathf.Max(0, _defaultCoins);

    public int AmountFor(string sceneName)
    {
        if (_games != null)
        {
            for (int i = 0; i < _games.Length; i++)
            {
                if (_games[i].Scene == sceneName)
                {
                    return Mathf.Max(0, _games[i].Coins);
                }
            }
        }

        return DefaultCoins;
    }

    [Serializable]
    public struct Entry
    {
        [SerializeField] private string _scene;
        [SerializeField] private int _coins;

        public string Scene => _scene;

        public int Coins => _coins;

        public Entry(string scene, int coins)
        {
            _scene = scene;
            _coins = coins;
        }
    }
}
