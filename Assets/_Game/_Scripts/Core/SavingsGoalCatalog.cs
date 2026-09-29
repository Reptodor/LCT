using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SavingsGoals", menuName = "Finashka/Цели накоплений")]
public sealed class SavingsGoalCatalog : ScriptableObject
{
    [SerializeField] private Goal[] _goals =
    {
        new Goal("bike", "Велосипед", 500, 25),
        new Goal("headphones", "Наушники", 300, 20),
        new Goal("trip", "Поездка", 800, 50)
    };

    public int Count => _goals == null ? 0 : _goals.Length;

    public Goal Get(int index)
    {
        return _goals[index];
    }

    public bool TryGet(string id, out Goal goal)
    {
        if (_goals != null && !string.IsNullOrEmpty(id))
        {
            for (int i = 0; i < _goals.Length; i++)
            {
                if (_goals[i].Id == id)
                {
                    goal = _goals[i];
                    return true;
                }
            }
        }

        goal = default;
        return false;
    }

    [Serializable]
    public struct Goal
    {
        [SerializeField] private string _id;
        [SerializeField] private string _title;
        [SerializeField] private int _target;
        [SerializeField] private int _deposit;

        public string Id => _id;

        public string Title => string.IsNullOrEmpty(_title) ? _id : _title;

        public int Target => Mathf.Max(1, _target);

        public int Deposit => Mathf.Max(1, _deposit);

        public Goal(string id, string title, int target, int deposit)
        {
            _id = id;
            _title = title;
            _target = target;
            _deposit = deposit;
        }
    }
}
