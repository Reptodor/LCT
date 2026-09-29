using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LevelSelectCard : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private RectTransform _visual;
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _subtitle;
    [SerializeField] private string _sceneName;

    private Vector2 _visualRest;
    private bool _restCached;

    public string SceneName => _sceneName;

    public bool HasScene => !string.IsNullOrEmpty(_sceneName);

    public void Present(string title, string subtitle, string sceneName)
    {
        _sceneName = sceneName ?? "";
        if (_title != null)
        {
            _title.text = title;
        }

        if (_subtitle != null)
        {
            _subtitle.text = subtitle;
        }
    }

    private void Awake()
    {
        CacheRest();
        if (_button != null)
        {
            _button.onClick.AddListener(OnClick);
        }

        SetInteractable(false);
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }

    public void SetPop(float linear, float startScale, float rise)
    {
        CacheRest();
        if (_visual == null)
        {
            return;
        }

        float eased = LevelSelectEase.OutBack(linear);
        float fade = LevelSelectEase.OutCubic(linear);
        float scale = Mathf.LerpUnclamped(startScale, 1f, eased);
        _visual.localScale = new Vector3(scale, scale, 1f);
        _visual.anchoredPosition = _visualRest + new Vector2(0f, Mathf.LerpUnclamped(rise, 0f, eased));

        if (_group != null)
        {
            _group.alpha = fade;
            _group.blocksRaycasts = true;
            _group.interactable = true;
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (_button != null)
        {
            _button.interactable = interactable && HasScene;
        }
    }

    public IEnumerator PlayPress(float duration)
    {
        CacheRest();
        if (_visual == null)
        {
            yield break;
        }

        float time = 0f;
        float length = Mathf.Max(0.05f, duration);
        while (time < length)
        {
            time += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(time / length);
            float dip = 1f - Mathf.Sin(k * Mathf.PI) * 0.08f;
            _visual.localScale = new Vector3(dip, dip, 1f);
            yield return null;
        }

        _visual.localScale = Vector3.one;
        _visual.anchoredPosition = _visualRest;
    }

    private void OnClick()
    {
        LevelSelectWindow window = GetComponentInParent<LevelSelectWindow>(true);
        if (window != null)
        {
            window.Enter(this);
        }
    }

    private void CacheRest()
    {
        if (_restCached || _visual == null)
        {
            return;
        }

        _visualRest = _visual.anchoredPosition;
        _restCached = true;
    }
}
