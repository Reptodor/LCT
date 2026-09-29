using UnityEngine;

[CreateAssetMenu(fileName = "HungerConfig", menuName = "Finashka/Голод")]
public sealed class HungerConfig : ScriptableObject
{
    [SerializeField] private float _intervalSeconds = 60f;
    [SerializeField] private int _amount = 5;

    public float IntervalSeconds => Mathf.Max(1f, _intervalSeconds);

    public int Amount => Mathf.Max(1, _amount);
}
