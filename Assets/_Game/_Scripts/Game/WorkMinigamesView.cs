using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorkMinigamesView : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] GameObject _menu;
    [SerializeField] GameObject _play;
    [SerializeField] GameObject _result;
    [SerializeField] Button _dimButton;
    [SerializeField] Button _closeMenuButton;
    [SerializeField] Button _closePlayButton;
    [SerializeField] Button _openNeedWantButton;
    [SerializeField] Button _needButton;
    [SerializeField] Button _wantButton;
    [SerializeField] Button _collectButton;
    [SerializeField] Button _openArButton;
    [SerializeField] TMP_Text _progress;
    [SerializeField] TMP_Text _itemTitle;
    [SerializeField] TMP_Text _itemPrompt;
    [SerializeField] TMP_Text _playFeedback;
    [SerializeField] TMP_Text _resultTitle;
    [SerializeField] TMP_Text _resultScore;
    [SerializeField] TMP_Text _resultCoins;

    NeedWantSession _session;
    bool _busy;
    bool _paid;
    Action<int> _onFinished;

    static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Good = new Color(0.55f, 0.92f, 0.55f, 1f);
    static readonly Color Bad = new Color(1f, 0.62f, 0.42f, 1f);

    bool _uiBound;
    bool _arHooked;

    public bool IsReady => _root != null;

    public void Bind(Action<int> onFinished)
    {
        _onFinished = onFinished;
    }

    public void Setup(
        GameObject root,
        GameObject menu,
        GameObject play,
        GameObject result,
        Button dimButton,
        Button closeMenuButton,
        Button closePlayButton,
        Button openNeedWantButton,
        Button openArButton,
        Button needButton,
        Button wantButton,
        Button collectButton,
        TMP_Text progress,
        TMP_Text itemTitle,
        TMP_Text itemPrompt,
        TMP_Text playFeedback,
        TMP_Text resultTitle,
        TMP_Text resultScore,
        TMP_Text resultCoins)
    {
        _root = root;
        _menu = menu;
        _play = play;
        _result = result;
        _dimButton = dimButton;
        _closeMenuButton = closeMenuButton;
        _closePlayButton = closePlayButton;
        _openNeedWantButton = openNeedWantButton;
        _openArButton = openArButton;
        _needButton = needButton;
        _wantButton = wantButton;
        _collectButton = collectButton;
        _progress = progress;
        _itemTitle = itemTitle;
        _itemPrompt = itemPrompt;
        _playFeedback = playFeedback;
        _resultTitle = resultTitle;
        _resultScore = resultScore;
        _resultCoins = resultCoins;
        BindUi();
    }

    void Awake()
    {
        BindUi();
        Hide();
    }

    void BindUi()
    {
        if (_uiBound || _openNeedWantButton == null)
        {
            return;
        }

        _uiBound = true;

        if (_dimButton != null)
        {
            _dimButton.onClick.AddListener(OnDim);
        }

        if (_closeMenuButton != null)
        {
            _closeMenuButton.onClick.AddListener(Close);
        }

        if (_closePlayButton != null)
        {
            _closePlayButton.onClick.AddListener(BackToMenu);
        }

        if (_openNeedWantButton != null)
        {
            _openNeedWantButton.onClick.AddListener(StartNeedWant);
        }

        if (_needButton != null)
        {
            _needButton.onClick.AddListener(() => OnAnswer(NeedWantKind.Need));
        }

        if (_wantButton != null)
        {
            _wantButton.onClick.AddListener(() => OnAnswer(NeedWantKind.Want));
        }

        if (_collectButton != null)
        {
            _collectButton.onClick.AddListener(Collect);
        }

        EnsureArCard();
        Hide();
    }

    public void Open()
    {
        _paid = false;
        _session = null;
        _busy = false;
        EnsureArCard();
        if (_root != null)
        {
            _root.SetActive(true);
        }

        ShowOnly(_menu);
        if (_menu != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_menu.transform as RectTransform);
        }
    }

    public void Close()
    {
        Hide();
    }

    void OnDim()
    {
        if (_menu != null && _menu.activeSelf)
        {
            Close();
        }
    }

    void BackToMenu()
    {
        if (_busy)
        {
            return;
        }

        _session = null;
        ShowOnly(_menu);
    }

    void StartNeedWant()
    {
        _session = NeedWantSession.Create(Environment.TickCount);
        _paid = false;
        ShowOnly(_play);
        ShowRound();
    }

    void OnAnswer(NeedWantKind kind)
    {
        if (_busy || _session == null || _session.IsFinished)
        {
            return;
        }

        NeedWantItem item = _session.Current;
        bool ok = _session.TryAnswer(kind);
        StartCoroutine(AfterAnswer(ok, item.Why));
    }

    IEnumerator AfterAnswer(bool ok, string why)
    {
        _busy = true;
        SetPlayButtons(false);
        if (_playFeedback != null)
        {
            _playFeedback.text = (ok ? "Верно! " : "Почти. ") + why;
            _playFeedback.color = ok ? Good : Bad;
        }

        yield return new WaitForSeconds(1.35f);

        if (_session != null && _session.IsFinished)
        {
            ShowResult();
        }
        else if (_session != null)
        {
            ShowRound();
        }

        _busy = false;
    }

    void ShowRound()
    {
        SetPlayButtons(true);
        NeedWantItem item = _session.Current;
        if (_progress != null)
        {
            _progress.text = (_session.Index + 1) + " / " + _session.TotalRounds;
        }

        if (_itemTitle != null)
        {
            _itemTitle.text = item.Title;
        }

        if (_itemPrompt != null)
        {
            _itemPrompt.text = item.Prompt;
        }

        if (_playFeedback != null)
        {
            _playFeedback.text = "Это нужно купить или просто хочется?";
            _playFeedback.color = Cream;
        }
    }

    void ShowResult()
    {
        ShowOnly(_result);
        int correct = _session.Correct;
        int total = _session.TotalRounds;
        int coins = _session.Coins;
        if (_resultTitle != null)
        {
            if (correct == total)
            {
                _resultTitle.text = "Супер!";
            }
            else if (correct >= 3)
            {
                _resultTitle.text = "Хорошо вышло";
            }
            else
            {
                _resultTitle.text = "Ещё потренируемся";
            }
        }

        if (_resultScore != null)
        {
            _resultScore.text = "Верно " + correct + " из " + total + "\nНужно — то, без чего трудно жить.\nХочу — приятно, но можно подождать.";
        }

        if (_resultCoins != null)
        {
            _resultCoins.text = coins > 0 ? "+" + coins + " монет" : "В этот раз без монет";
        }
    }

    void Collect()
    {
        int coins = _session != null ? _session.Coins : 0;
        if (!_paid)
        {
            _paid = true;
            if (_onFinished != null)
            {
                _onFinished.Invoke(coins);
            }
        }

        Close();
    }

    void ShowOnly(GameObject page)
    {
        if (_menu != null)
        {
            _menu.SetActive(page == _menu);
        }

        if (_play != null)
        {
            _play.SetActive(page == _play);
        }

        if (_result != null)
        {
            _result.SetActive(page == _result);
        }
    }

    void SetPlayButtons(bool on)
    {
        if (_needButton != null)
        {
            _needButton.interactable = on;
        }

        if (_wantButton != null)
        {
            _wantButton.interactable = on;
        }
    }

    void EnsureArCard()
    {
        if (_openArButton == null && _menu != null)
        {
            Transform found = _menu.transform.Find("ArCard");
            if (found != null)
            {
                _openArButton = found.GetComponent<Button>();
            }
        }

        if (_openArButton == null && _menu != null)
        {
            _openArButton = CreateArCard();
        }

        if (_openArButton != null && !_arHooked)
        {
            _arHooked = true;
            _openArButton.onClick.AddListener(OpenAr);
        }
    }

    Button CreateArCard()
    {
        Transform parent = _menu.transform;
        if (_closeMenuButton != null && _closeMenuButton.transform.parent != null)
        {
            parent = _closeMenuButton.transform.parent;
        }

        var go = new GameObject("ArCard", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(VerticalLayoutGroup));
        go.transform.SetParent(parent, false);
        if (_closeMenuButton != null && _closeMenuButton.transform.parent == parent)
        {
            go.transform.SetSiblingIndex(_closeMenuButton.transform.GetSiblingIndex());
        }

        var image = go.GetComponent<Image>();
        image.color = Cream;
        image.raycastTarget = true;
        if (_openNeedWantButton != null)
        {
            Image sample = _openNeedWantButton.GetComponent<Image>();
            if (sample != null)
            {
                image.sprite = sample.sprite;
                image.type = sample.type;
                image.pixelsPerUnitMultiplier = sample.pixelsPerUnitMultiplier;
            }
        }

        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 156f;
        layout.preferredHeight = 168f;
        layout.flexibleHeight = 0f;
        layout.flexibleWidth = 1f;
        var column = go.GetComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(20, 20, 18, 16);
        column.spacing = 4f;
        column.childAlignment = TextAnchor.MiddleCenter;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;

        TMP_Text sampleText = _menu.GetComponentInChildren<TMP_Text>(true);
        ArLabel(go.transform, "Title", "AR", 32f, FontStyles.Bold, sampleText);
        ArLabel(go.transform, "Subtitle", "Оживи своего персонажа", 24f, FontStyles.Normal, sampleText);
        return button;
    }

    static void ArLabel(Transform parent, string name, string text, float size, FontStyles style, TMP_Text sample)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = size + 16f;
        layout.preferredHeight = size + 20f;
        layout.flexibleHeight = 0f;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (sample != null)
        {
            tmp.font = sample.font;
            tmp.fontSharedMaterial = sample.fontSharedMaterial;
        }
        else if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }

        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = new Color(0.14f, 0.16f, 0.08f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
    }

    void OpenAr()
    {
        SceneManager.LoadScene(BootController.PetArSceneName);
    }

    void Hide()
    {
        _busy = false;
        StopAllCoroutines();
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }
}
