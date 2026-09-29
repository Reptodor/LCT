using UnityEngine;

[CreateAssetMenu(fileName = "AllowanceConfig", menuName = "Finashka/Выплата")]
public sealed class AllowanceConfig : ScriptableObject
{
    [SerializeField] private int _amount = 15;
    [SerializeField] private float _intervalSeconds = 60f;

    public int Amount => Mathf.Max(1, _amount);

    public float IntervalSeconds => Mathf.Max(1f, _intervalSeconds);
}
