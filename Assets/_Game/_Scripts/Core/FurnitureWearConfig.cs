using UnityEngine;

[CreateAssetMenu(fileName = "FurnitureWearConfig", menuName = "Finashka/Мебель")]
public sealed class FurnitureWearConfig : ScriptableObject
{
    [SerializeField] private int _maxHp = 5;

    public int MaxHp => Mathf.Max(1, _maxHp);
}
