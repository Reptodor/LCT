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
    [SerializeField] LevelSelectWindow _levelSelectPrefab;
    [SerializeField] SnackShopView _snackShop;
    [SerializeField] FurnishWindow _furnish;

    LevelSelectWindow _levelSelect;

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
        LevelSelectWindow window = EnsureLevelSelect();
        if (window == null)
        {
            SetFeedback("Не вышло открыть подработку");
            return;
        }

        window.Open();
    }

    LevelSelectWindow EnsureLevelSelect()
    {
        if (_levelSelect != null)
        {
            return _levelSelect;
        }

        _levelSelect = FindFirstObjectByType<LevelSelectWindow>(FindObjectsInactive.Include);
        if (_levelSelect != null)
        {
            return _levelSelect;
        }

        if (_levelSelectPrefab == null)
        {
            return null;
        }

        Canvas canvas = FindHudCanvas();
        if (canvas == null)
        {
            return null;
        }

        _levelSelect = Instantiate(_levelSelectPrefab, canvas.transform);
        return _levelSelect;
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
            var image = copy.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.55f, 0.36f, 0.20f, 1f);
            }

            button = copy.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
        }

        var label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = "Расстановка";
            label.enableAutoSizing = true;
            label.fontSizeMin = 16f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnWardrobe);
    }

    void OnWardrobe()
    {
        EnsureFurnish();
        if (_furnish == null || !_furnish.IsReady)
        {
            SetFeedback("Не вышло открыть расстановку");
            return;
        }

        if (_furnish.IsOpen)
        {
            _furnish.Close();
            return;
        }

        _furnish.Open(CurrentRoomId());
    }

    static string CurrentRoomId()
    {
        var walk = FindFirstObjectByType<PetWalk>();
        if (walk == null)
        {
            return "";
        }

        return walk.CurrentRoomId;
    }

    void EnsureFurnish()
    {
        Canvas canvas = FindHudCanvas();
        if (_furnish == null)
        {
            _furnish = GetComponent<FurnishWindow>();
        }

        if (_furnish == null)
        {
            _furnish = gameObject.AddComponent<FurnishWindow>();
        }

        if (_furnish != null && !_furnish.IsReady && canvas != null)
        {
            FurnishOverlayFactory.Build(canvas.transform, _furnish);
        }

        if (_furnish != null)
        {
            _furnish.ItemChosen = OnFurnishChosen;
        }
    }

    void OnFurnishChosen(string itemId)
    {
        if (_furnish != null && _furnish.IsOpen)
        {
            _furnish.Close();
        }

        if (!LivingFurnish.Place(itemId))
        {
            SetFeedback("Не вышло поставить");
            return;
        }

        SetFeedback("Купили");
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
            _coins.color = new Color(0.98f, 0.86f, 0.32f, 1f);
            TintCaption(_coins, new Color(1f, 0.96f, 0.86f, 1f));
        }

        if (_hunger != null)
        {
            _hunger.text = state.hunger.ToString();
            _hunger.color = new Color(1f, 0.64f, 0.38f, 1f);
            TintCaption(_hunger, new Color(1f, 0.93f, 0.84f, 1f));
        }
    }

    static void TintCaption(TMP_Text value, Color color)
    {
        if (value.transform.parent == null)
        {
            return;
        }

        var caption = value.transform.parent.Find("Caption");
        if (caption == null)
        {
            return;
        }

        var label = caption.GetComponent<TMP_Text>();
        if (label != null)
        {
            label.color = color;
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
