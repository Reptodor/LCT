using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnackShopView : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] Button _dimButton;
    [SerializeField] Button _closeButton;
    [SerializeField] TMP_Text _coins;
    [SerializeField] TMP_Text _status;
    [SerializeField] Button[] _buyButtons;
    CanvasGroup _group;
    RectTransform _motion;
    RectTransform[] _cards = System.Array.Empty<RectTransform>();

    bool _uiBound;
    bool _busy;
    Action<ShopItem> _onBought;

    public bool IsReady => _root != null && _buyButtons != null && _buyButtons.Length == ShopCatalog.Food.Length;

    public void Bind(Action<ShopItem> onBought)
    {
        _onBought = onBought;
    }

    public void Setup(
        GameObject root,
        Button dimButton,
        Button closeButton,
        TMP_Text coins,
        TMP_Text status,
        Button[] buyButtons,
        CanvasGroup group,
        RectTransform motion,
        RectTransform[] cards)
    {
        _root = root;
        _dimButton = dimButton;
        _closeButton = closeButton;
        _coins = coins;
        _status = status;
        _buyButtons = buyButtons;
        _group = group;
        _motion = motion;
        _cards = cards ?? System.Array.Empty<RectTransform>();
        BindUi();
    }

    void Awake()
    {
        BindUi();
        Hide();
    }

    void BindUi()
    {
        if (_uiBound || _closeButton == null || _buyButtons == null)
        {
            return;
        }

        _uiBound = true;
        if (_dimButton != null)
        {
            _dimButton.onClick.AddListener(Close);
        }

        _closeButton.onClick.AddListener(Close);
        for (int i = 0; i < _buyButtons.Length; i++)
        {
            int index = i;
            if (_buyButtons[i] != null)
            {
                _buyButtons[i].onClick.AddListener(() => OnBuy(index));
            }
        }

        Hide();
    }

    public void Open()
    {
        if (_root == null || _busy)
        {
            return;
        }

        if (_status != null)
        {
            _status.text = "Выбери еду";
        }

        RefreshAffordability();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        StopAllCoroutines();
        StartCoroutine(OpenRoutine());
    }

    public void Close()
    {
        if (_root == null || !_root.activeSelf)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(CloseRoutine());
    }

    IEnumerator OpenRoutine()
    {
        _busy = true;
        HudSheetMotion.SnapHidden(_group, _motion, _cards);
        float time = 0f;
        while (time < HudSheetMotion.OpenDuration)
        {
            time += Time.unscaledDeltaTime;
            HudSheetMotion.ApplyOpen(Mathf.Clamp01(time / HudSheetMotion.OpenDuration), _group, _motion);
            yield return null;
        }

        HudSheetMotion.ApplyOpen(1f, _group, _motion);
        if (_group != null)
        {
            _group.blocksRaycasts = true;
            _group.interactable = true;
        }

        float total = HudSheetMotion.CardTotal(_cards.Length);
        time = 0f;
        while (time < total)
        {
            time += Time.unscaledDeltaTime;
            HudSheetMotion.PlaceCards(_cards, time);
            yield return null;
        }

        HudSheetMotion.PlaceCards(_cards, total);
        _busy = false;
    }

    IEnumerator CloseRoutine()
    {
        _busy = true;
        float fromAlpha = _group != null ? _group.alpha : 1f;
        float fromScale = _motion != null ? _motion.localScale.x : 1f;
        Vector2 fromPos = _motion != null ? _motion.anchoredPosition : Vector2.zero;
        float time = 0f;
        while (time < HudSheetMotion.CloseDuration)
        {
            time += Time.unscaledDeltaTime;
            HudSheetMotion.ApplyClose(Mathf.Clamp01(time / HudSheetMotion.CloseDuration), _group, _motion, fromAlpha, fromScale, fromPos);
            yield return null;
        }

        _busy = false;
        Hide();
    }

    void OnBuy(int index)
    {
        if (index < 0 || index >= ShopCatalog.Food.Length)
        {
            return;
        }

        ShopItem food = ShopCatalog.Food[index];
        if (!GameSession.IsReady || !ShopCheckout.TryPay(GameSession.State, food))
        {
            if (_status != null)
            {
                _status.text = "Не хватает монет на «" + food.Title + "»";
            }

            RefreshAffordability();
            return;
        }

        if (_status != null)
        {
            _status.text = "Куплено: " + food.Title + "  ·  −" + food.Cost + " монет";
        }

        if (_onBought != null)
        {
            _onBought.Invoke(food);
        }

        RefreshAffordability();
    }

    void RefreshAffordability()
    {
        GameState state = GameSession.State;
        int coins = state != null ? state.coins : 0;
        if (_coins != null)
        {
            _coins.text = "На счету: " + coins;
        }

        if (_buyButtons == null)
        {
            return;
        }

        for (int i = 0; i < _buyButtons.Length && i < ShopCatalog.Food.Length; i++)
        {
            if (_buyButtons[i] != null)
            {
                _buyButtons[i].interactable = coins >= ShopCatalog.Food[i].Cost;
            }
        }
    }

    void Hide()
    {
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }
}
