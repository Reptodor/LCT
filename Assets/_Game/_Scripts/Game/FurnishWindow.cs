using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FurnishRoomRows
{
    public readonly string RoomId;
    public readonly string Title;
    public readonly GameObject[] Rows;

    public FurnishRoomRows(string roomId, string title, GameObject[] rows)
    {
        RoomId = roomId;
        Title = title;
        Rows = rows;
    }
}

public class FurnishWindow : MonoBehaviour
{
    GameObject _root;
    Button _dimButton;
    Button _closeButton;
    TMP_Text _title;
    TMP_Text _coins;
    TMP_Text _status;
    ScrollRect _scroll;
    CanvasGroup _group;
    RectTransform _motion;
    FurnishRoomRows[] _rooms = System.Array.Empty<FurnishRoomRows>();
    bool _bound;
    bool _open;
    bool _busy;

    public System.Action<ShopItem> Purchased;

    public bool IsReady => _root != null;

    public bool IsOpen => _open;

    public void Setup(
        GameObject root,
        Button dimButton,
        Button closeButton,
        TMP_Text title,
        ScrollRect scroll,
        FurnishRoomRows[] rooms,
        CanvasGroup group,
        RectTransform motion,
        TMP_Text coins,
        TMP_Text status)
    {
        _root = root;
        _dimButton = dimButton;
        _closeButton = closeButton;
        _title = title;
        _coins = coins;
        _status = status;
        _scroll = scroll;
        _rooms = rooms ?? System.Array.Empty<FurnishRoomRows>();
        _group = group;
        _motion = motion;
        Bind();
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    void Bind()
    {
        if (_bound || _closeButton == null)
        {
            return;
        }

        _bound = true;
        if (_dimButton != null)
        {
            _dimButton.onClick.AddListener(Close);
        }

        _closeButton.onClick.AddListener(Close);
    }

    public void Buy(string itemId)
    {
        if (!ShopCatalog.TryFind(itemId, out ShopItem item) || item.Category != ShopCategory.Optional)
        {
            SetStatus("Этого товара нет");
            return;
        }

        if (!GameSession.IsReady)
        {
            return;
        }

        if (ShopCheckout.Owns(GameSession.State, item.Id))
        {
            SetStatus("«" + item.Title + "» уже куплено");
            RefreshShop();
            return;
        }

        if (!ShopCheckout.CanPay(GameSession.State, item))
        {
            SetStatus("Не хватает монет на «" + item.Title + "»");
            RefreshShop();
            return;
        }

        if (!LivingFurnish.Place(item.Id))
        {
            SetStatus("Не вышло поставить «" + item.Title + "»");
            return;
        }

        if (!ShopCheckout.TryPay(GameSession.State, item))
        {
            SetStatus("Не хватает монет на «" + item.Title + "»");
            RefreshShop();
            return;
        }

        GameSession.Persist();
        SetStatus("Куплено: " + item.Title + "  ·  −" + item.Cost + " монет");
        RefreshShop();
        if (Purchased != null)
        {
            Purchased(item);
        }
    }

    public void Open(string roomId)
    {
        if (_root == null || _busy)
        {
            return;
        }

        ShowRoom(roomId);
        SetStatus("Выбери мебель");
        RefreshShop();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        if (_scroll != null)
        {
            _scroll.verticalNormalizedPosition = 1f;
        }

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
        _open = true;
        RectTransform[] cards = VisibleCards();
        HudSheetMotion.SnapHidden(_group, _motion, cards);
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

        float total = HudSheetMotion.CardTotal(cards.Length);
        time = 0f;
        while (time < total)
        {
            time += Time.unscaledDeltaTime;
            HudSheetMotion.PlaceCards(cards, time);
            yield return null;
        }

        HudSheetMotion.PlaceCards(cards, total);
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

        _open = false;
        _busy = false;
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    RectTransform[] VisibleCards()
    {
        var list = new List<RectTransform>();
        for (int i = 0; i < _rooms.Length; i++)
        {
            if (_rooms[i] == null)
            {
                continue;
            }

            RectTransform[] part = HudSheetMotion.Visuals(_rooms[i].Rows);
            for (int r = 0; r < part.Length; r++)
            {
                list.Add(part[r]);
            }
        }

        return list.ToArray();
    }

    void ShowRoom(string roomId)
    {
        int match = -1;
        for (int i = 0; i < _rooms.Length; i++)
        {
            if (_rooms[i] != null && _rooms[i].RoomId == roomId)
            {
                match = i;
                break;
            }
        }

        if (match < 0 && _rooms.Length > 0)
        {
            match = 0;
        }

        for (int i = 0; i < _rooms.Length; i++)
        {
            bool on = i == match;
            GameObject[] rows = _rooms[i] != null ? _rooms[i].Rows : null;
            if (rows == null)
            {
                continue;
            }

            for (int r = 0; r < rows.Length; r++)
            {
                if (rows[r] != null)
                {
                    rows[r].SetActive(on);
                }
            }
        }

        if (_title != null && match >= 0 && _rooms[match] != null)
        {
            _title.text = _rooms[match].Title;
        }
    }

    void RefreshShop()
    {
        int coins = GameSession.IsReady ? GameSession.State.coins : 0;
        if (_coins != null)
        {
            _coins.text = "На счету: " + coins;
        }

        for (int i = 0; i < _rooms.Length; i++)
        {
            GameObject[] rows = _rooms[i] != null ? _rooms[i].Rows : null;
            if (rows == null)
            {
                continue;
            }

            for (int r = 0; r < rows.Length; r++)
            {
                GameObject row = rows[r];
                if (row == null || !ShopCatalog.TryFind(row.name, out ShopItem item))
                {
                    continue;
                }

                Transform buttonTransform = row.transform.Find("Visual/Info/BuyButton");
                if (buttonTransform == null)
                {
                    continue;
                }

                var button = buttonTransform.GetComponent<Button>();
                var label = buttonTransform.GetComponentInChildren<TMP_Text>();
                bool owned = GameSession.IsReady && ShopCheckout.Owns(GameSession.State, item.Id);
                if (label != null)
                {
                    label.text = owned ? "Куплено" : "Купить";
                }

                if (button != null)
                {
                    button.interactable = !owned && coins >= item.Cost;
                }
            }
        }
    }

    void SetStatus(string text)
    {
        if (_status != null)
        {
            _status.text = text;
        }
    }
}
