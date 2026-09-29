using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class SavingsGoalButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _progress;

    public Button Button => _button;

    public void Set(string title, string progress, bool selected)
    {
        if (_title != null)
        {
            _title.text = title;
        }

        if (_progress != null)
        {
            _progress.text = progress;
        }

        if (_image != null)
        {
            _image.color = selected
                ? new Color(0.22f, 0.46f, 0.28f, 1f)
                : new Color(0.07f, 0.16f, 0.09f, 1f);
        }
    }
}
