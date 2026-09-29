using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    [SerializeField] SavingsWindow _savingsPrefab;
    [SerializeField] SnackShopView _snackShop;
    [SerializeField] FurnishWindow _furnish;

    LevelSelectWindow _levelSelect;
    SavingsWindow _savings;
    SettingsWindow _settings;
    AllowanceWindow _allowance;
    TMP_Text _allowanceTimer;
    Image _hungerFill;
    Image _joyFill;
    FurnitureRepairWindow _repair;
    bool _armTap;
    Vector2 _tapPos;

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
        EnsureSavingsButton();
        EnsureSettingsButton();
        HideRoomSwitch();
        EnsureAllowanceTimer();
        EnsureSkipAllowanceButton();
        HideHungerReadout();
        EnsureNeedBars();
        bool needsSave = false;
        if (GameSession.IsReady)
        {
            needsSave |= Allowance.EnsureSchedule(GameSession.State);
            needsSave |= Hunger.CatchUp(GameSession.State);
        }

        if (needsSave)
        {
            GameSession.Persist();
        }

        Refresh();
        OpenSavingsIfNeeded();
    }

    void Update()
    {
        TickAllowance();
        TickHunger();
        TickFurniture();
        PollFurnitureTap();
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

    void EnsureSavingsButton()
    {
        if (_snackButton == null)
        {
            return;
        }

        Transform bar = _snackButton.transform.parent;
        Transform existing = bar.Find("SavingsButton");
        Button button;
        if (existing != null)
        {
            button = existing.GetComponent<Button>();
        }
        else
        {
            GameObject copy = Object.Instantiate(_snackButton.gameObject, bar);
            copy.name = "SavingsButton";
            Image image = copy.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.16f, 0.42f, 0.30f, 1f);
            }

            button = copy.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = "Копилка";
            label.enableAutoSizing = true;
            label.fontSizeMin = 16f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnSavings);
    }

    void EnsureSettingsButton()
    {
        GameObject top = GameObject.Find("TopBar");
        Transform parent = top != null ? top.transform : FindHudCanvas() != null ? FindHudCanvas().transform : null;
        if (parent == null)
        {
            return;
        }

        Transform leftover = parent.Find("LogoutButton");
        if (leftover != null)
        {
            Destroy(leftover.gameObject);
        }

        Transform existing = parent.Find("SettingsButton");
        Button button;
        if (existing != null)
        {
            button = existing.GetComponent<Button>();
        }
        else
        {
            var go = new GameObject("SettingsButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-8f, -8f);
            rect.sizeDelta = new Vector2(196f, 64f);
            go.GetComponent<LayoutElement>().ignoreLayout = true;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.18f, 0.32f, 0.22f, 1f);
            button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 4f);
            labelRect.offsetMax = new Vector2(-8f, -4f);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = "Настройки";
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = 16f;
            label.fontSizeMax = 28f;
            label.color = new Color(1f, 0.96f, 0.88f, 1f);
            label.raycastTarget = false;
            if (_petName != null)
            {
                label.font = _petName.font;
            }
        }

        if (button == null)
        {
            return;
        }

        button.transform.SetAsLastSibling();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnSettings);
    }

    void OnSettings()
    {
        SettingsWindow window = EnsureSettings();
        if (window == null || !window.IsReady)
        {
            SetFeedback("Не вышло открыть настройки");
            return;
        }

        if (window.IsOpen)
        {
            window.Close();
            return;
        }

        window.Open();
    }

    SettingsWindow EnsureSettings()
    {
        if (_settings == null)
        {
            _settings = GetComponent<SettingsWindow>();
        }

        if (_settings == null)
        {
            _settings = gameObject.AddComponent<SettingsWindow>();
        }

        Canvas canvas = FindHudCanvas();
        if (_settings != null && !_settings.IsReady && canvas != null)
        {
            TMP_FontAsset font = _petName != null ? _petName.font : null;
            SettingsOverlayFactory.Build(canvas.transform, _settings, font);
        }

        if (_settings != null)
        {
            _settings.ResetDone = OnProfileReset;
            _settings.LogoutRequested = OnLogout;
        }

        return _settings;
    }

    void OnProfileReset()
    {
        GameSession.ResetProgress();
        SceneManager.LoadScene(BootController.PetCustomizeSceneName);
    }

    void OnLogout()
    {
        if (GameSession.IsLocalSession)
        {
            GameSession.Discard();
        }
        else
        {
            GameSession.Unload();
        }

        new AuthService().Logout();
        SceneManager.LoadScene(BootController.ProfileSetupSceneName);
    }

    void OpenSavingsIfNeeded()
    {
        if (!GameSession.IsReady || SavingsBank.HasConfirmedGoal(GameSession.State))
        {
            return;
        }

        OnSavings();
    }

    void OnSavings()
    {
        SavingsWindow window = EnsureSavings();
        if (window == null)
        {
            SetFeedback("Не вышло открыть копилку");
            return;
        }

        window.BalanceChanged -= OnSavingsChanged;
        window.BalanceChanged += OnSavingsChanged;
        window.Open();
    }

    void OnSavingsChanged()
    {
        Refresh();
    }

    SavingsWindow EnsureSavings()
    {
        if (_savings != null)
        {
            return _savings;
        }

        _savings = FindFirstObjectByType<SavingsWindow>(FindObjectsInactive.Include);
        if (_savings != null)
        {
            return _savings;
        }

        if (_savingsPrefab == null)
        {
            return null;
        }

        Canvas canvas = FindHudCanvas();
        if (canvas == null)
        {
            return null;
        }

        _savings = Instantiate(_savingsPrefab, canvas.transform);
        return _savings;
    }

    void OnWardrobe()
    {
        string roomId = CurrentRoomId();
        if (!RoomUnlocks.IsOpen(roomId))
        {
            if (_furnish != null && _furnish.IsOpen)
            {
                _furnish.Close();
            }

            SetFeedback(RoomUnlocks.FurnishBlockedText(roomId));
            return;
        }

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
            _furnish.Purchased = OnFurnitureBought;
        }
    }

    void OnFurnitureBought(ShopItem item)
    {
        SetFeedback(item.Title + ": −" + item.Cost + " монет, +" + item.Effect + " радости");
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

    void OnFoodBought(ShopItem food)
    {
        GameSession.Persist();
        SetFeedback(food.Title + ": −" + food.Cost + " монет, +" + food.Effect + " сытости");
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
            _petName.text = string.IsNullOrWhiteSpace(state.petName) ? AppInfo.Title : state.petName;
        }

        if (_coins != null)
        {
            _coins.text = state.coins.ToString();
            _coins.color = new Color(0.98f, 0.86f, 0.32f, 1f);
            TintCaption(_coins, new Color(1f, 0.96f, 0.86f, 1f));
        }

        if (_allowanceTimer != null)
        {
            _allowanceTimer.text = Allowance.FormatRemaining(Allowance.RemainingSeconds(state));
        }

        ApplyNeedBars(state);
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

    void EnsureAllowanceTimer()
    {
        if (_coins == null)
        {
            return;
        }

        Transform parent = _coins.transform.parent;
        if (parent == null)
        {
            return;
        }

        Transform existing = parent.Find("AllowanceTimer");
        if (existing != null)
        {
            _allowanceTimer = existing.GetComponent<TMP_Text>();
            return;
        }

        var coinsLayout = _coins.GetComponent<LayoutElement>();
        if (coinsLayout != null)
        {
            coinsLayout.minHeight = 64f;
            coinsLayout.preferredHeight = 64f;
        }

        Transform caption = parent.Find("Caption");
        if (caption != null)
        {
            var captionLayout = caption.GetComponent<LayoutElement>();
            if (captionLayout != null)
            {
                captionLayout.minHeight = 36f;
                captionLayout.preferredHeight = 36f;
            }
        }

        var go = new GameObject("AllowanceTimer", typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.transform.SetSiblingIndex(_coins.transform.GetSiblingIndex() + 1);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 36f;
        layout.preferredHeight = 36f;
        var label = go.AddComponent<TextMeshProUGUI>();
        label.font = _coins.font;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.enableAutoSizing = true;
        label.fontSizeMin = 18f;
        label.fontSizeMax = 28f;
        label.color = new Color(1f, 0.96f, 0.86f, 1f);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        label.text = "";
        _allowanceTimer = label;
    }

    void HideHungerReadout()
    {
        if (_hunger == null)
        {
            return;
        }

        Transform card = _hunger.transform;
        while (card != null && card.name != "HungerCard")
        {
            card = card.parent;
        }

        if (card != null)
        {
            card.gameObject.SetActive(false);
        }
    }

    void EnsureNeedBars()
    {
        if (_petName == null)
        {
            return;
        }

        Transform parent = _petName.transform.parent;
        if (parent == null)
        {
            return;
        }

        Transform existing = parent.Find("NeedBars");
        if (existing != null)
        {
            _hungerFill = existing.Find("HungerBar/Track/Fill")?.GetComponent<Image>();
            _joyFill = existing.Find("JoyBar/Track/Fill")?.GetComponent<Image>();
            return;
        }

        var root = new GameObject("NeedBars", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        root.transform.SetParent(parent, false);
        root.transform.SetSiblingIndex(_petName.transform.GetSiblingIndex() + 1);
        var ignored = root.GetComponent<LayoutElement>();
        ignored.ignoreLayout = true;

        RectTransform nameRect = _petName.rectTransform;
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = nameRect.anchorMin;
        rect.anchorMax = nameRect.anchorMax;
        rect.pivot = new Vector2(nameRect.pivot.x, 1f);
        float nameBottom = nameRect.anchoredPosition.y - nameRect.sizeDelta.y * nameRect.pivot.y;
        float width = Mathf.Min(760f, nameRect.sizeDelta.x);
        const float stackHeight = 72f;
        rect.anchoredPosition = new Vector2(nameRect.anchoredPosition.x, nameBottom - 12f);
        rect.sizeDelta = new Vector2(width, stackHeight);

        var column = root.GetComponent<VerticalLayoutGroup>();
        column.spacing = 8f;
        column.padding = new RectOffset(0, 0, 0, 0);
        column.childAlignment = TextAnchor.UpperCenter;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;

        _hungerFill = NeedBar(root.transform, "HungerBar", "голод", new Color(1f, 0.58f, 0.28f, 1f));
        _joyFill = NeedBar(root.transform, "JoyBar", "радость", new Color(0.98f, 0.55f, 0.75f, 1f));

        Transform stats = parent.Find("Stats");
        if (stats == null)
        {
            return;
        }

        var statsRect = stats.GetComponent<RectTransform>();
        float barsBottom = rect.anchoredPosition.y - stackHeight;
        float statsTop = statsRect.anchoredPosition.y + statsRect.sizeDelta.y * (1f - statsRect.pivot.y);
        float overlap = statsTop - (barsBottom - 16f);
        if (overlap > 0f)
        {
            statsRect.anchoredPosition -= new Vector2(0f, overlap);
        }
    }

    Image NeedBar(Transform parent, string name, string caption, Color fillColor)
    {
        var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        var rowLayout = row.GetComponent<LayoutElement>();
        rowLayout.minHeight = 32f;
        rowLayout.preferredHeight = 32f;
        var rowGroup = row.GetComponent<HorizontalLayoutGroup>();
        rowGroup.spacing = 12f;
        rowGroup.childAlignment = TextAnchor.MiddleCenter;
        rowGroup.childControlWidth = true;
        rowGroup.childControlHeight = true;
        rowGroup.childForceExpandWidth = false;
        rowGroup.childForceExpandHeight = true;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(LayoutElement));
        labelGo.transform.SetParent(row.transform, false);
        var labelLayout = labelGo.GetComponent<LayoutElement>();
        labelLayout.minWidth = 148f;
        labelLayout.preferredWidth = 148f;
        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = caption;
        label.font = _petName.font;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.MidlineRight;
        label.fontSize = 24f;
        label.color = fillColor;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;

        var trackGo = new GameObject("Track", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        trackGo.transform.SetParent(row.transform, false);
        var trackLayout = trackGo.GetComponent<LayoutElement>();
        trackLayout.flexibleWidth = 1f;
        trackLayout.minHeight = 22f;
        trackLayout.preferredHeight = 22f;
        var track = trackGo.GetComponent<Image>();
        track.sprite = SolidSprite();
        track.type = Image.Type.Simple;
        track.color = new Color(0.08f, 0.14f, 0.08f, 0.82f);
        track.raycastTarget = false;

        var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(trackGo.transform, false);
        var fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(3f, 3f);
        fillRect.offsetMax = new Vector2(-3f, -3f);
        var fill = fillGo.GetComponent<Image>();
        fill.sprite = SolidSprite();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.color = fillColor;
        fill.raycastTarget = false;
        fill.fillAmount = 0f;
        return fill;
    }

    void ApplyNeedBars(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (_hungerFill != null)
        {
            _hungerFill.fillAmount = Mathf.Clamp01(state.hunger / (float)PetActions.HungerMax);
        }

        if (_joyFill != null)
        {
            _joyFill.fillAmount = Mathf.Clamp01(state.joy / (float)PetActions.JoyMax);
        }
    }

    void TickHunger()
    {
        if (!GameSession.IsReady || !Hunger.CatchUp(GameSession.State))
        {
            return;
        }

        GameSession.Persist();
        ApplyNeedBars(GameSession.State);
    }

    void TickFurniture()
    {
        if (!GameSession.IsReady || !FurnitureWear.CatchUp(GameSession.State, out string[] broke))
        {
            return;
        }

        GameSession.Persist();
        ApplyNeedBars(GameSession.State);
        if (broke != null)
        {
            for (int i = 0; i < broke.Length; i++)
            {
                LivingFurnish.SetBroken(broke[i], true);
                if (ShopCatalog.TryFind(broke[i], out ShopItem item))
                {
                    SetFeedback("Сломалось: " + item.Title);
                }
            }
        }

        if (_repair != null && _repair.IsOpen)
        {
            _repair.Refresh();
        }
    }

    void PollFurnitureTap()
    {
        if (!GameSession.IsReady || !ReadTap(out Vector2 position) || HitsUi(position) || Camera.main == null)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit[] hits = Physics.RaycastAll(ray, 80f);
        int closest = -1;
        for (int i = 0; i < hits.Length; i++)
        {
            if (IsHouseShell(hits[i].collider))
            {
                continue;
            }

            if (closest < 0 || hits[i].distance < hits[closest].distance)
            {
                closest = i;
            }
        }

        if (closest < 0)
        {
            return;
        }

        FurniturePick pick = hits[closest].collider.GetComponentInParent<FurniturePick>();
        if (pick == null || string.IsNullOrEmpty(pick.ItemId))
        {
            return;
        }

        if (!ShopCheckout.Owns(GameSession.State, pick.ItemId))
        {
            return;
        }

        OpenRepair(pick.ItemId);
    }

    static bool IsHouseShell(Collider collider)
    {
        return collider is MeshCollider
            && collider.gameObject.name == "House"
            && collider.transform.parent == null;
    }

    bool ReadTap(out Vector2 position)
    {
        position = default;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                _armTap = true;
                _tapPos = mouse.position.ReadValue();
            }
            else if (_armTap && mouse.leftButton.wasReleasedThisFrame)
            {
                position = mouse.position.ReadValue();
                _armTap = false;
                return (position - _tapPos).sqrMagnitude <= 48f * 48f;
            }
        }

        Touchscreen touch = Touchscreen.current;
        if (touch == null)
        {
            return false;
        }

        UnityEngine.InputSystem.Controls.TouchControl press = touch.primaryTouch;
        if (press.press.wasPressedThisFrame)
        {
            _armTap = true;
            _tapPos = press.position.ReadValue();
        }
        else if (_armTap && press.press.wasReleasedThisFrame)
        {
            position = press.position.ReadValue();
            _armTap = false;
            return (position - _tapPos).sqrMagnitude <= 48f * 48f;
        }

        return false;
    }

    static bool HitsUi(Vector2 screenPos)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };
        var hits = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        for (int i = 0; i < hits.Count; i++)
        {
            if (hits[i].gameObject.GetComponentInParent<Selectable>() != null)
            {
                return true;
            }
        }

        return false;
    }

    void OpenRepair(string itemId)
    {
        FurnitureRepairWindow window = EnsureRepair();
        if (window == null || !window.IsReady)
        {
            SetFeedback("Не вышло открыть мебель");
            return;
        }

        window.Open(itemId);
    }

    FurnitureRepairWindow EnsureRepair()
    {
        if (_repair == null)
        {
            _repair = GetComponent<FurnitureRepairWindow>();
        }

        if (_repair == null)
        {
            _repair = gameObject.AddComponent<FurnitureRepairWindow>();
        }

        Canvas canvas = FindHudCanvas();
        if (_repair != null && !_repair.IsReady && canvas != null)
        {
            TMP_FontAsset font = _petName != null ? _petName.font : null;
            FurnitureRepairOverlayFactory.Build(canvas.transform, _repair, font);
        }

        if (_repair != null)
        {
            _repair.Repaired = OnFurnitureRepaired;
            _repair.RepairFailed = OnFurnitureRepairFailed;
        }

        return _repair;
    }

    void OnFurnitureRepaired(string title, int joy, int cost)
    {
        string price = cost > 0 ? ", −" + cost + " монет" : "";
        SetFeedback(joy > 0
            ? "Починено: " + title + price + ", +" + joy + " радости"
            : "Починено: " + title + price);
        Refresh();
    }

    void OnFurnitureRepairFailed()
    {
        SetFeedback("Не хватает монет");
    }

    void EnsureSkipAllowanceButton()
    {
        if (_allowanceTimer == null)
        {
            return;
        }

        Transform parent = _allowanceTimer.transform.parent;
        if (parent == null)
        {
            return;
        }

        Transform existing = parent.Find("SkipAllowanceButton");
        if (!GameSession.IsDemo)
        {
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            return;
        }

        Button button;
        if (existing != null)
        {
            button = existing.GetComponent<Button>();
        }
        else
        {
            var go = new GameObject("SkipAllowanceButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.transform.SetAsLastSibling();
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -8f);
            rect.sizeDelta = new Vector2(0f, 48f);
            var layout = go.GetComponent<LayoutElement>();
            layout.ignoreLayout = true;
            var image = go.GetComponent<Image>();
            image.sprite = SolidSprite();
            image.type = Image.Type.Simple;
            image.color = new Color(0.72f, 0.48f, 0.12f, 1f);
            button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(6f, 2f);
            labelRect.offsetMax = new Vector2(-6f, -2f);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = "Пропустить";
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.enableAutoSizing = true;
            label.fontSizeMin = 14f;
            label.fontSizeMax = 24f;
            label.color = new Color(0.16f, 0.1f, 0.04f, 1f);
            label.raycastTarget = false;
            label.font = _allowanceTimer.font;
        }

        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnSkipAllowance);
    }

    static Sprite _solidSprite;

    static Sprite SolidSprite()
    {
        if (_solidSprite == null)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            _solidSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        return _solidSprite;
    }

    void OnSkipAllowance()
    {
        if (!GameSession.IsDemo || !GameSession.IsReady)
        {
            return;
        }

        if (_allowance != null && _allowance.IsOpen)
        {
            return;
        }

        Allowance.FinishWait(GameSession.State);
        GameSession.Persist();
        TickAllowance();
    }

    void TickAllowance()
    {
        if (!GameSession.IsReady)
        {
            return;
        }

        GameState state = GameSession.State;
        if (Allowance.EnsureSchedule(state))
        {
            GameSession.Persist();
        }

        float left = Allowance.RemainingSeconds(state);
        if (_allowanceTimer != null)
        {
            _allowanceTimer.text = Allowance.FormatRemaining(left);
        }

        if (left > 0f || (_allowance != null && _allowance.IsOpen))
        {
            return;
        }

        AllowanceWindow window = EnsureAllowance();
        if (window == null)
        {
            return;
        }

        window.Confirmed = OnAllowanceConfirmed;
        window.Open(Allowance.Config.Amount);
    }

    void OnAllowanceConfirmed()
    {
        if (!GameSession.IsReady)
        {
            return;
        }

        Allowance.Grant(GameSession.State);
        GameSession.Persist();
        Refresh();
    }

    AllowanceWindow EnsureAllowance()
    {
        if (_allowance == null)
        {
            _allowance = GetComponent<AllowanceWindow>();
        }

        if (_allowance == null)
        {
            _allowance = gameObject.AddComponent<AllowanceWindow>();
        }

        Canvas canvas = FindHudCanvas();
        if (_allowance != null && !_allowance.IsReady && canvas != null)
        {
            TMP_FontAsset font = _coins != null ? _coins.font : _petName != null ? _petName.font : null;
            AllowanceOverlayFactory.Build(canvas.transform, _allowance, font);
        }

        return _allowance;
    }

    void SetFeedback(string text)
    {
        if (_feedback != null)
        {
            _feedback.text = text;
        }
    }
}
