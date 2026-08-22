using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingView : MonoBehaviour
{
    [SerializeField] TMP_Text _title;
    [SerializeField] TMP_Text _status;
    [SerializeField] Slider _progress;

    public void SetTitle(string title)
    {
        if (_title != null)
        {
            _title.text = title;
        }
    }

    public void SetStatus(string status)
    {
        if (_status != null)
        {
            _status.text = status;
        }
    }

    public IEnumerator FillTo(float target, float duration)
    {
        if (_progress == null)
        {
            yield break;
        }

        float end = Mathf.Clamp01(target);
        float start = _progress.value;
        if (duration <= 0f)
        {
            _progress.value = end;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _progress.value = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        _progress.value = end;
    }

    public void ShowError(string message)
    {
        SetStatus(message);
        if (_progress != null)
        {
            _progress.value = 1f;
        }
    }
}
