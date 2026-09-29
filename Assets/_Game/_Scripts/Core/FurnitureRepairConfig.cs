using UnityEngine;

[CreateAssetMenu(fileName = "FurnitureRepairConfig", menuName = "Finashka/Починка")]
public sealed class FurnitureRepairConfig : ScriptableObject
{
    [SerializeField] private int _maxCost = 20;
    [SerializeField] private float _breakSeconds = 300f;

    public int MaxCost => Mathf.Max(0, _maxCost);

    public float BreakSeconds => Mathf.Max(1f, _breakSeconds);
}
