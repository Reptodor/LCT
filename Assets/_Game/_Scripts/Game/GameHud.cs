using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    [SerializeField] TMP_Text _petName;
    [SerializeField] TMP_Text _coins;
    [SerializeField] TMP_Text _hunger;
    [SerializeField] TMP_Text _feedback;
    [SerializeField] Button _workButton;
    [SerializeField] Button _snackButton;
    [SerializeField] WorkMinigamesView _workGames;
    [SerializeField] SnackShopView _snackShop;

    void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        if (!GameSession.IsReady)
        {
            GameSession.Initialize(SaveService.CreateDefault());
        }

        if (_workButton != null)
        {
            _workButton.onClick.AddListener(OnWork);
        }

        if (_snackButton != null)
        {
            _snackButton.onClick.AddListener(OnSnack);
        }

        EnsureWorkGames();
        EnsureSnackShop();
        EnsureWardrobeButton();
        HideRoomSwitch();
        Refresh();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            GameSession.Persist();
        }
    }

    void OnApplicationQuit()
    {
        GameSession.Persist();
    }

    void OnWork()
    {
        EnsureWorkGames();
        if (_workGames == null || !_workGames.IsReady)
        {
            SetFeedback("Не вышло открыть подработку");
            return;
        }

        _workGames.Open();
    }

    void OnMinigameFinished(int coins)
    {
        if (PetActions.TryEarn(GameSession.State, coins))
        {
            GameSession.Persist();
            SetFeedback("Подработал. +" + coins + " монет");
        }
        else
        {
            SetFeedback("В этот раз без монет. Попробуй ещё");
        }

        Refresh();
    }

    void EnsureWardrobeButton()
    {
        if (_snackButton == null)
        {
            return;
        }

        var bar = _snackButton.transform.parent;
        var existing = bar.Find("WardrobeButton");
        Button button;
        if (existing != null)
        {
            button = existing.GetComponent<Button>();
        }
        else
        {
            var copy = Object.Instantiate(_snackButton.gameObject, bar);
            copy.name = "WardrobeButton";
            var label = copy.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = "Шкаф";
            }

            var image = copy.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.55f, 0.36f, 0.20f, 1f);
            }

            button = copy.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnWardrobe);
    }

    void OnWardrobe()
    {
        bool show = !IsWardrobeZoneOn();
        PetRoom.ShowWardrobeZone(show);
        var button = _snackButton.transform.parent.Find("WardrobeButton");
        var image = button != null ? button.GetComponent<Image>() : null;
        if (image != null)
        {
            image.color = show
                ? new Color(0.42f, 0.72f, 0.38f, 1f)
                : new Color(0.55f, 0.36f, 0.20f, 1f);
        }

        SetFeedback(show ? "Нажми на площадку" : "");
    }

    static bool IsWardrobeZoneOn()
    {
        var zone = GameObject.Find("WardrobeZone");
        return zone != null && zone.activeSelf;
    }

    void OnSnack()
    {
        EnsureSnackShop();
        if (_snackShop == null || !_snackShop.IsReady)
        {
            SetFeedback("Не вышло открыть перекус");
            return;
        }

        _snackShop.Open();
    }

    void OnFoodBought(FoodItem food)
    {
        GameSession.Persist();
        SetFeedback(food.Title + ": −" + food.Cost + " монет, +" + food.Hunger + " сытости");
        Refresh();
    }

    void EnsureWorkGames()
    {
        Canvas canvas = FindHudCanvas();
        if (_workGames == null)
        {
            _workGames = GetComponent<WorkMinigamesView>();
        }

        if (_workGames == null)
        {
            _workGames = FindFirstObjectByType<WorkMinigamesView>(FindObjectsInactive.Include);
        }

        if (_workGames == null)
        {
            _workGames = gameObject.AddComponent<WorkMinigamesView>();
        }

        if (_workGames != null && !_workGames.IsReady && canvas != null)
        {
            try
            {
                WorkOverlayFactory.Build(canvas.transform, _workGames);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                SetFeedback("Не вышло открыть подработку");
            }
        }

        if (_workGames != null)
        {
            _workGames.Bind(OnMinigameFinished);
        }
    }

    void EnsureSnackShop()
    {
        Canvas canvas = FindHudCanvas();
        if (_snackShop == null)
        {
            _snackShop = GetComponent<SnackShopView>();
        }

        if (_snackShop == null)
        {
            _snackShop = FindFirstObjectByType<SnackShopView>(FindObjectsInactive.Include);
        }

        if (_snackShop == null)
        {
            _snackShop = gameObject.AddComponent<SnackShopView>();
        }

        if (_snackShop != null && !_snackShop.IsReady && canvas != null)
        {
            try
            {
                SnackOverlayFactory.Build(canvas.transform, _snackShop);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                SetFeedback("Не вышло открыть перекус");
            }
        }

        if (_snackShop != null)
        {
            _snackShop.Bind(OnFoodBought);
        }
    }

    void HideRoomSwitch()
    {
        Canvas canvas = FindHudCanvas();
        if (canvas == null)
        {
            return;
        }

        var existing = canvas.transform.Find("RoomButton");
        if (existing != null)
        {
            existing.gameObject.SetActive(false);
        }
    }

    Canvas FindHudCanvas()
    {
        if (_workButton != null)
        {
            var fromButton = _workButton.GetComponentInParent<Canvas>();
            if (fromButton != null)
            {
                return fromButton;
            }
        }

        var named = GameObject.Find("HudCanvas");
        if (named != null)
        {
            return named.GetComponent<Canvas>();
        }

        return FindFirstObjectByType<Canvas>();
    }

    void Refresh()
    {
        GameState state = GameSession.State;
        if (state == null)
        {
            return;
        }

        if (_petName != null)
        {
            _petName.text = AppInfo.Title;
        }

        if (_coins != null)
        {
            _coins.text = state.coins.ToString();
        }

        if (_hunger != null)
        {
            _hunger.text = state.hunger.ToString();
        }
    }

    void SetFeedback(string text)
    {
        if (_feedback != null)
        {
            _feedback.text = text;
        }
    }
}
