using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FurnitureRepairWindow : MonoBehaviour
{
    GameObject _root;
    TMP_Text _title;
    Image _fill;
    Button _repairButton;
    CanvasGroup _group;
    string _itemId;
    bool _open;

    public Action<string, int, int> Repaired;
    public Action RepairFailed;

    public bool IsOpen => _open;

    public bool IsReady => _root != null;

    public void Setup(
        GameObject root,
        TMP_Text title,
        Image fill,
        Button repairButton,
        Button closeButton,
        Button dimButton,
        CanvasGroup group)
    {
        _root = root;
        _title = title;
        _fill = fill;
        _repairButton = repairButton;
        _group = group;
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (dimButton != null)
        {
            dimButton.onClick.RemoveAllListeners();
            dimButton.onClick.AddListener(Close);
        }

        if (_repairButton != null)
        {
            _repairButton.onClick.RemoveAllListeners();
            _repairButton.onClick.AddListener(OnRepair);
        }

        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    public void Open(string itemId)
    {
        if (_root == null || string.IsNullOrEmpty(itemId))
        {
            return;
        }

        _itemId = itemId;
        _open = true;
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        if (_group != null)
        {
            _group.alpha = 1f;
            _group.blocksRaycasts = true;
            _group.interactable = true;
        }

        Refresh();
    }

    public void Close()
    {
        _open = false;
        if (_group != null)
        {
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }

        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    public void Refresh()
    {
        if (!GameSession.IsReady || string.IsNullOrEmpty(_itemId))
        {
            return;
        }

        ShopCatalog.TryFind(_itemId, out ShopItem item);
        if (_title != null)
        {
            _title.text = item != null ? item.Title : _itemId;
        }

        float ratio = FurnitureWear.Ratio(GameSession.State, _itemId);
        if (_fill != null)
        {
            _fill.fillAmount = ratio;
            _fill.color = ratio <= 0f
                ? new Color(0.75f, 0.28f, 0.22f, 1f)
                : Color.Lerp(new Color(0.86f, 0.45f, 0.18f, 1f), new Color(0.45f, 0.78f, 0.32f, 1f), ratio);
        }

        int cost = FurnitureWear.Cost(GameSession.State, _itemId);
        bool damaged = FurnitureWear.Hp(GameSession.State, _itemId) < FurnitureWear.Config.MaxHp;
        if (_repairButton != null)
        {
            _repairButton.interactable = damaged;
            TMP_Text label = _repairButton.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = damaged && cost > 0 ? "Починить · " + cost : "Починить";
            }
        }
    }

    void OnRepair()
    {
        if (!GameSession.IsReady || string.IsNullOrEmpty(_itemId))
        {
            return;
        }

        if (FurnitureWear.Hp(GameSession.State, _itemId) >= FurnitureWear.Config.MaxHp)
        {
            return;
        }

        int cost = FurnitureWear.Cost(GameSession.State, _itemId);
        if (!FurnitureWear.TryRepair(GameSession.State, _itemId, out int joy))
        {
            Refresh();
            if (RepairFailed != null)
            {
                RepairFailed();
            }

            return;
        }

        LivingFurnish.SetBroken(_itemId, false);
        GameSession.Persist();
        Refresh();
        string title = _title != null ? _title.text : _itemId;
        if (Repaired != null)
        {
            Repaired(title, joy, cost);
        }
    }
}
