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

    GameObject _bottomBar;
    Image _roomButtonImage;
    Sprite _bathIcon;
    Sprite _livingIcon;
    bool _inBath;

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
        EnsureRoomButton();
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

    void EnsureRoomButton()
    {
        _bottomBar = GameObject.Find("BottomBar");
        Canvas canvas = FindHudCanvas();
        if (canvas == null)
        {
            return;
        }

        var existing = canvas.transform.Find("RoomButton");
        GameObject go;
        if (existing != null)
        {
            go = existing.gameObject;
        }
        else
        {
            go = new GameObject("RoomButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas.transform, false);
        }

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(112f, 112f);
        rt.anchoredPosition = new Vector2(-28f, -40f);

        _bathIcon = RoomIcons.Bath();
        _livingIcon = RoomIcons.Living();
        _roomButtonImage = go.GetComponent<Image>();
        _roomButtonImage.sprite = _bathIcon;
        _roomButtonImage.preserveAspect = true;
        _roomButtonImage.raycastTarget = true;

        var button = go.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnRoomToggle);
        button.targetGraphic = _roomButtonImage;
        ApplyRoomUi();
    }

    void OnRoomToggle()
    {
        _inBath = !_inBath;
        PetRoom.Build(Camera.main, _inBath ? RoomKind.Bath : RoomKind.Living);
        ApplyRoomUi();
    }

    void ApplyRoomUi()
    {
        if (_bottomBar == null)
        {
            _bottomBar = GameObject.Find("BottomBar");
        }

        if (_bottomBar != null)
        {
            _bottomBar.SetActive(!_inBath);
        }

        if (_workButton != null)
        {
            _workButton.gameObject.SetActive(!_inBath);
        }

        if (_snackButton != null)
        {
            _snackButton.gameObject.SetActive(!_inBath);
        }

        if (_roomButtonImage != null)
        {
            _roomButtonImage.sprite = _inBath ? _livingIcon : _bathIcon;
        }

        if (_inBath)
        {
            SetFeedback("");
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
