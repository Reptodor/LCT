using System;
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

    bool _uiBound;
    Action<FoodItem> _onBought;

    public bool IsReady => _root != null && _buyButtons != null && _buyButtons.Length == FoodCatalog.All.Length;

    public void Bind(Action<FoodItem> onBought)
    {
        _onBought = onBought;
    }

    public void Setup(GameObject root, Button dimButton, Button closeButton, TMP_Text coins, TMP_Text status, Button[] buyButtons)
    {
        _root = root;
        _dimButton = dimButton;
        _closeButton = closeButton;
        _coins = coins;
        _status = status;
        _buyButtons = buyButtons;
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
        if (_root != null)
        {
            _root.SetActive(true);
        }

        if (_status != null)
        {
            _status.text = "Выбери еду. Дороже — сытнее.";
        }

        RefreshAffordability();
    }

    public void Close()
    {
        Hide();
    }

    void OnBuy(int index)
    {
        if (index < 0 || index >= FoodCatalog.All.Length)
        {
            return;
        }

        FoodItem food = FoodCatalog.All[index];
        if (!PetActions.TryBuyFood(GameSession.State, food.Cost, food.Hunger))
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
            _status.text = "Куплено: " + food.Title + "  ·  +" + food.Hunger + " сытости";
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
            _coins.text = "Монеты: " + coins;
        }

        if (_buyButtons == null)
        {
            return;
        }

        for (int i = 0; i < _buyButtons.Length && i < FoodCatalog.All.Length; i++)
        {
            if (_buyButtons[i] != null)
            {
                _buyButtons[i].interactable = coins >= FoodCatalog.All[i].Cost;
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
