using System;
using System.Collections;
using TMPro;
using UnityEngine;
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

        Hide();
    }

    public void Open()
    {
        _paid = false;
        _session = null;
        _busy = false;
        if (_root != null)
        {
            _root.SetActive(true);
        }

        ShowOnly(_menu);
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
