using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LevelSelectOpener : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private LevelSelectWindow _window;

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_button != null)
        {
            _button.onClick.AddListener(Open);
        }
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(Open);
        }
    }

    public void Open()
    {
        if (_window != null)
        {
            _window.Open();
        }
    }
}
