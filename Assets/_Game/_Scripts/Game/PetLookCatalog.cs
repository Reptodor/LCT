using UnityEngine;

[CreateAssetMenu(fileName = "PetLookCatalog", menuName = "Finashka/Внешность питомца")]
public sealed class PetLookCatalog : ScriptableObject
{
    public const string ResourceName = "PetLookCatalog";

    [SerializeField] GameObject _cat;

    public GameObject Cat => _cat;

    public static PetLookCatalog Load()
    {
        return Resources.Load<PetLookCatalog>(ResourceName);
    }
}
