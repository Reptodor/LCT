using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LevelSelectWindow : MonoBehaviour
{
    [SerializeField] private CanvasGroup _rootGroup;
    [SerializeField] private RectTransform _sheet;
    [SerializeField] private Button _dimButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private ScrollRect _scroll;
    [SerializeField] private RectTransform _content;
    [SerializeField] private LevelSelectCard[] _cards;

    [Header("Появление окна")]
    [SerializeField] private float _openDuration = 0.38f;
    [SerializeField] private float _closeDuration = 0.22f;
    [SerializeField] private float _sheetStartScale = 0.86f;
    [SerializeField] private float _sheetStartOffsetY = -56f;

    [Header("Появление карточек")]
    [SerializeField] private float _cardDuration = 0.34f;
    [SerializeField] private float _cardStagger = 0.09f;
    [SerializeField] private float _cardStartScale = 0.64f;
    [SerializeField] private float _cardRise = 42f;
    [SerializeField] private float _pressDuration = 0.16f;

    private bool _entering;
    private bool _bound;
    private Vector2 _sheetRest;

    public bool IsOpen { get; private set; }

    public bool IsEntering => _entering;

    private void Awake()
    {
        if (_sheet != null)
        {
            _sheetRest = _sheet.anchoredPosition;
        }

        Bind();
        CollectCards();
        SnapHidden();
    }

    private void OnDestroy()
    {
        if (_dimButton != null)
        {
            _dimButton.onClick.RemoveListener(Close);
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(Close);
        }
    }

    public void Open()
    {
        if (_entering)
        {
            return;
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        Bind();
        CollectCards();
        StopAllCoroutines();
        StartCoroutine(OpenRoutine());
    }

    public void Close()
    {
        if (_entering || !gameObject.activeInHierarchy)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(CloseRoutine());
    }

    public void Enter(LevelSelectCard card)
    {
        if (_entering || card == null || !card.HasScene)
        {
            return;
        }

        string sceneName = card.SceneName;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("LevelSelectWindow: сцена не добавлена в билд: " + sceneName);
            return;
        }

        _entering = true;
        SetCardsInteractable(false);
        StopAllCoroutines();
        StartCoroutine(EnterRoutine(card, sceneName));
    }

    private void Bind()
    {
        if (_bound)
        {
            return;
        }

        _bound = true;
        if (_dimButton != null)
        {
            _dimButton.onClick.AddListener(Close);
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Close);
        }
    }

    private void CollectCards()
    {
        if (_cards != null && _cards.Length > 0)
        {
            return;
        }

        if (_content != null)
        {
            _cards = _content.GetComponentsInChildren<LevelSelectCard>(true);
        }
    }

    private IEnumerator OpenRoutine()
    {
        IsOpen = true;
        SetInput(false);
        SnapHidden();
        ResetScroll();

        float openLength = Mathf.Max(0.05f, _openDuration);
        float time = 0f;
        while (time < openLength)
        {
            time += Time.unscaledDeltaTime;
            ApplySheet(Mathf.Clamp01(time / openLength));
            yield return null;
        }

        ApplySheet(1f);
        SetInput(true);
        yield return PopCards();
        SetCardsInteractable(true);
    }

    private IEnumerator PopCards()
    {
        if (_cards == null || _cards.Length == 0)
        {
            yield break;
        }

        float stagger = Mathf.Max(0.01f, _cardStagger);
        float duration = Mathf.Max(0.05f, _cardDuration);
        float total = stagger * (_cards.Length - 1) + duration;
        float time = 0f;

        while (time < total)
        {
            time += Time.unscaledDeltaTime;
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] == null)
                {
                    continue;
                }

                float linear = Mathf.Clamp01((time - stagger * i) / duration);
                _cards[i].SetPop(linear, _cardStartScale, _cardRise);
                _cards[i].SetInteractable(linear >= 1f);
            }

            yield return null;
        }

        for (int i = 0; i < _cards.Length; i++)
        {
            if (_cards[i] == null)
            {
                continue;
            }

            _cards[i].SetPop(1f, _cardStartScale, _cardRise);
            _cards[i].SetInteractable(true);
        }
    }

    private IEnumerator CloseRoutine()
    {
        IsOpen = false;
        SetCardsInteractable(false);
        SetInput(false);

        float fromAlpha = _rootGroup != null ? _rootGroup.alpha : 1f;
        float fromScale = _sheet != null ? _sheet.localScale.x : 1f;
        Vector2 fromPos = _sheet != null ? _sheet.anchoredPosition : Vector2.zero;
        float length = Mathf.Max(0.05f, _closeDuration);
        float time = 0f;

        while (time < length)
        {
            time += Time.unscaledDeltaTime;
            float k = LevelSelectEase.InCubic(Mathf.Clamp01(time / length));
            if (_rootGroup != null)
            {
                _rootGroup.alpha = Mathf.Lerp(fromAlpha, 0f, k);
            }

            if (_sheet != null)
            {
                float scale = Mathf.Lerp(fromScale, _sheetStartScale, k);
                _sheet.localScale = new Vector3(scale, scale, 1f);
                _sheet.anchoredPosition = Vector2.Lerp(fromPos, _sheetRest + new Vector2(0f, _sheetStartOffsetY), k);
            }

            yield return null;
        }

        SnapHidden();
        gameObject.SetActive(false);
    }

    private IEnumerator EnterRoutine(LevelSelectCard card, string sceneName)
    {
        yield return card.PlayPress(_pressDuration);
        if (GameSession.IsReady)
        {
            GameSession.Persist();
        }

        SceneManager.LoadScene(sceneName);
    }

    private void ApplySheet(float linear)
    {
        float scaleEase = LevelSelectEase.OutBack(linear);
        float fade = LevelSelectEase.OutCubic(linear);
        if (_rootGroup != null)
        {
            _rootGroup.alpha = fade;
        }

        if (_sheet == null)
        {
            return;
        }

        float scale = Mathf.LerpUnclamped(_sheetStartScale, 1f, scaleEase);
        _sheet.localScale = new Vector3(scale, scale, 1f);
        float y = Mathf.LerpUnclamped(_sheetStartOffsetY, 0f, scaleEase);
        _sheet.anchoredPosition = _sheetRest + new Vector2(0f, y);
    }

    private void SnapHidden()
    {
        if (_rootGroup != null)
        {
            _rootGroup.alpha = 0f;
            _rootGroup.blocksRaycasts = true;
            _rootGroup.interactable = true;
        }

        if (_sheet != null)
        {
            _sheet.localScale = new Vector3(_sheetStartScale, _sheetStartScale, 1f);
            _sheet.anchoredPosition = _sheetRest + new Vector2(0f, _sheetStartOffsetY);
        }

        if (_cards == null)
        {
            return;
        }

        for (int i = 0; i < _cards.Length; i++)
        {
            if (_cards[i] == null)
            {
                continue;
            }

            _cards[i].SetPop(0f, _cardStartScale, _cardRise);
            _cards[i].SetInteractable(false);
        }
    }

    private void ResetScroll()
    {
        if (_content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }

        Canvas.ForceUpdateCanvases();
        if (_scroll != null)
        {
            _scroll.StopMovement();
            _scroll.verticalNormalizedPosition = 1f;
        }
    }

    private void SetCardsInteractable(bool interactable)
    {
        if (_cards == null)
        {
            return;
        }

        for (int i = 0; i < _cards.Length; i++)
        {
            if (_cards[i] != null)
            {
                _cards[i].SetInteractable(interactable);
            }
        }
    }

    private void SetInput(bool enabled)
    {
        if (_dimButton != null)
        {
            _dimButton.interactable = enabled;
        }

        if (_closeButton != null)
        {
            _closeButton.interactable = enabled;
        }

        if (_scroll != null)
        {
            _scroll.enabled = enabled;
        }
    }
}
