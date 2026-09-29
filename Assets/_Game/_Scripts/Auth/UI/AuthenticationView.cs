using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AuthenticationView : MonoBehaviour
{
    static readonly Color Lawn = new Color(0.18f, 0.42f, 0.26f, 1f);
    static readonly Color Gold = new Color(0.96f, 0.78f, 0.28f, 1f);
    static readonly Color Ink = new Color(0.14f, 0.16f, 0.08f, 1f);
    static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Hint = new Color(0.42f, 0.48f, 0.36f, 1f);

    [Header("Panels")]
    [SerializeField] private GameObject _choicePanel;
    [SerializeField] private GameObject _registerPanel;
    [SerializeField] private GameObject _loginPanel;

    [Header("Register UI")]
    [SerializeField] private TMP_InputField _regEmailInput;
    [SerializeField] private TMP_InputField _regPasswordInput;
    [SerializeField] private Button _regSubmitButton;

    [Header("Login UI")]
    [SerializeField] private TMP_InputField _loginEmailInput;
    [SerializeField] private TMP_InputField _loginPasswordInput;
    [SerializeField] private Button _loginSubmitButton;

    [Header("Choice UI")]
    [SerializeField] private Button _goLoginButton;
    [SerializeField] private Button _goRegisterButton;
    [SerializeField] private Button _goGuestButton;
    [SerializeField] private Button _goDemoButton;

    private IAuthService _auth;
    private TMP_Text _status;


    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        _auth = _auth ?? new AuthService();
        EnsureLocalButtons();
        ApplyLook();
        _goLoginButton?.onClick.AddListener(ShowLogin);
        _goRegisterButton?.onClick.AddListener(ShowRegister);
        _goGuestButton?.onClick.AddListener(OnGuestClicked);
        _goDemoButton?.onClick.AddListener(OnDemoClicked);
        _regSubmitButton?.onClick.AddListener(OnRegisterClicked);
        _loginSubmitButton?.onClick.AddListener(OnLoginClicked);

        if (TryContinueActiveSession())
        {
            return;
        }

        ShowChoice();
    }

    public void SetupAuthService(IAuthService service)
    {
        _auth = service ?? new AuthService();
    }

    public void ShowChoice()
    {
        SetPanel(_choicePanel, true);
        SetPanel(_registerPanel, false);
        SetPanel(_loginPanel, false);
        ClearStatus();
    }

    private void ShowRegister()
    {
        SetPanel(_choicePanel, false);
        SetPanel(_registerPanel, true);
        SetPanel(_loginPanel, false);
        ClearStatus();
    }

    private void ShowLogin()
    {
        SetPanel(_choicePanel, false);
        SetPanel(_registerPanel, false);
        SetPanel(_loginPanel, true);
        ClearStatus();
    }

    private void SetPanel(GameObject go, bool visible)
    {
        if (go != null)
        {
            go.SetActive(visible);
        }
    }

    private void OnRegisterClicked()
    {
        string email = _regEmailInput != null ? _regEmailInput.text : string.Empty;
        string password = _regPasswordInput != null ? _regPasswordInput.text : string.Empty;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowStatus("Введи почту и пароль");
            return;
        }

        if (!_auth.RegisterUser(email, password))
        {
            ShowStatus("Такой профиль уже есть. Войди в него.");
            return;
        }

        _auth.SaveDeviceId(DeviceIdentifier.Get());
        Debug.Log("[Auth] Регистрация завершена, можно продолжать игру");
        Enter(true);
    }

    private void OnGuestClicked()
    {
        _auth.BeginLocalSession(false);
        Debug.Log("[Auth] Гостевой вход: прогресс сохранится, пока не выйдете из профиля");
        Enter(false);
    }

    private void OnDemoClicked()
    {
        _auth.BeginLocalSession(true);
        Debug.Log("[Auth] Демо-вход: таймер монет можно пропускать");
        Enter(false);
    }

    private void OnLoginClicked()
    {
        string email = _loginEmailInput != null ? _loginEmailInput.text : string.Empty;
        string password = _loginPasswordInput != null ? _loginPasswordInput.text : string.Empty;
        if (!_auth.LoginUser(email, password))
        {
            ShowStatus("Неверный email или пароль");
            return;
        }

        Debug.Log("[Auth] Вход выполнен, можно продолжать игру");
        Enter(false);
    }

    private bool TryContinueActiveSession()
    {
        if (!_auth.HasActiveSession())
        {
            return false;
        }

        Debug.Log("[Auth] Сессия сохранена, входим без пароля: " + _auth.ActiveProfileId);
        Enter(false);
        return true;
    }

    private void Enter(bool createPet)
    {
        string profileId = _auth.ActiveProfileId;
        if (!string.IsNullOrEmpty(profileId))
        {
            GameSession.BindProfile(Application.persistentDataPath, profileId, _auth.LegacySaveOwner);
        }

        bool needsPet = createPet || !GameSession.IsReady || !GameSession.State.petLookSet;
        string scene = needsPet ? BootController.PetCustomizeSceneName : BootController.GameSceneName;
        SceneManager.LoadScene(scene);
    }

    private void EnsureLocalButtons()
    {
        if (_choicePanel == null)
        {
            return;
        }

        float step = ChoiceStep();
        float registerY = _goRegisterButton != null
            ? _goRegisterButton.GetComponent<RectTransform>().anchoredPosition.y
            : -125f;
        if (_goGuestButton == null)
        {
            _goGuestButton = CloneChoiceButton("Button (Guest)", "Войти как гость", registerY + step);
        }

        if (_goDemoButton == null)
        {
            _goDemoButton = CloneChoiceButton("Button (Demo)", "Демо", registerY + step * 2f);
        }
    }

    private float ChoiceStep()
    {
        if (_goLoginButton == null || _goRegisterButton == null)
        {
            return -125f;
        }

        float loginY = _goLoginButton.GetComponent<RectTransform>().anchoredPosition.y;
        float registerY = _goRegisterButton.GetComponent<RectTransform>().anchoredPosition.y;
        float step = registerY - loginY;
        return Mathf.Abs(step) < 1f ? -125f : step;
    }

    private Button CloneChoiceButton(string name, string label, float y)
    {
        Transform existing = _choicePanel.transform.Find(name);
        if (existing != null)
        {
            TMP_Text existingLabel = existing.GetComponentInChildren<TMP_Text>();
            if (existingLabel != null)
            {
                existingLabel.text = label;
            }

            return existing.GetComponent<Button>();
        }

        Button source = _goRegisterButton != null ? _goRegisterButton : _goLoginButton;
        if (source == null)
        {
            return null;
        }

        GameObject copy = Instantiate(source.gameObject, _choicePanel.transform);
        copy.name = name;
        RectTransform rect = copy.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
        Button button = copy.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }

        TMP_Text text = copy.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = label;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
        }

        return button;
    }

    private void ClearStatus()
    {
        if (_status != null)
        {
            _status.text = string.Empty;
        }
    }

    private void ShowStatus(string message)
    {
        TMP_Text label = EnsureStatus();
        if (label != null)
        {
            label.text = message;
        }
    }

    private TMP_Text EnsureStatus()
    {
        if (_status != null)
        {
            return _status;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            return null;
        }

        Transform existing = canvas.transform.Find("AuthStatus");
        if (existing != null)
        {
            _status = existing.GetComponent<TMP_Text>();
            return _status;
        }

        var go = new GameObject("AuthStatus", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 0.08f);
        rect.anchorMax = new Vector2(0.92f, 0.2f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        _status = go.AddComponent<TextMeshProUGUI>();
        _status.alignment = TextAlignmentOptions.Center;
        _status.fontSize = 28f;
        _status.fontStyle = FontStyles.Bold;
        _status.color = Cream;
        _status.raycastTarget = false;
        TMP_Text sample = _loginEmailInput != null ? _loginEmailInput.textComponent : null;
        if (sample == null && _regEmailInput != null)
        {
            sample = _regEmailInput.textComponent;
        }

        if (sample != null)
        {
            _status.font = sample.font;
        }

        return _status;
    }

    private void ApplyLook()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Lawn;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            PaintBackdrop(canvas.transform);
            EnsureTitle(canvas.transform);
        }

        PaintPanel(_choicePanel);
        PaintPanel(_registerPanel);
        PaintPanel(_loginPanel);
        PaintButtons(_choicePanel);
        PaintButtons(_registerPanel);
        PaintButtons(_loginPanel);
        PaintButton(_goLoginButton, "Войти");
        PaintButton(_goRegisterButton, "Зарегистрироваться");
        PaintButton(_goGuestButton, "Войти как гость");
        PaintButton(_goDemoButton, "Демо");
        PaintButton(_loginSubmitButton, "Войти");
        PaintButton(_regSubmitButton, "Создать");
        PaintField(_loginEmailInput, "Почта");
        PaintField(_loginPasswordInput, "Пароль");
        PaintField(_regEmailInput, "Почта");
        PaintField(_regPasswordInput, "Пароль");
    }

    static void PaintBackdrop(Transform canvas)
    {
        if (canvas.Find("Lawn") != null)
        {
            return;
        }

        var go = new GameObject("Lawn", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvas, false);
        go.transform.SetAsFirstSibling();
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var image = go.GetComponent<Image>();
        image.color = Lawn;
        image.raycastTarget = false;
    }

    void EnsureTitle(Transform canvas)
    {
        if (canvas.Find("AuthTitle") != null)
        {
            return;
        }

        var go = new GameObject("AuthTitle", typeof(RectTransform));
        go.transform.SetParent(canvas, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 0.78f);
        rect.anchorMax = new Vector2(0.92f, 0.92f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var title = go.AddComponent<TextMeshProUGUI>();
        title.text = AppInfo.Title;
        title.fontSize = 72f;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Cream;
        title.raycastTarget = false;
        TMP_Text sample = SampleFont();
        if (sample != null)
        {
            title.font = sample.font;
        }
    }

    static void PaintPanel(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        var image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.08f, 0.2f, 0.12f, 0.28f);
        }
    }

    static void PaintButtons(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            PaintButton(buttons[i], null);
        }
    }

    static void PaintButton(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        var image = button.targetGraphic as Image;
        if (image == null)
        {
            image = button.GetComponent<Image>();
        }

        if (image != null)
        {
            image.color = Gold;
            button.targetGraphic = image;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 0.75f, 1f);
        colors.pressedColor = new Color(0.82f, 0.66f, 0.16f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        button.colors = colors;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(label))
        {
            text.text = label;
        }

        text.color = Ink;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;
    }

    static void PaintField(TMP_InputField field, string placeholder)
    {
        if (field == null)
        {
            return;
        }

        var image = field.targetGraphic as Image;
        if (image == null)
        {
            image = field.GetComponent<Image>();
        }

        if (image != null)
        {
            image.color = Color.white;
            field.targetGraphic = image;
        }

        ColorBlock colors = field.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = Color.white;
        colors.selectedColor = Color.white;
        field.colors = colors;

        if (field.textComponent != null)
        {
            field.textComponent.color = Ink;
            field.textComponent.raycastTarget = false;
        }

        var hint = field.placeholder as TMP_Text;
        if (hint == null)
        {
            return;
        }

        hint.text = placeholder;
        hint.color = Hint;
        hint.fontStyle = FontStyles.Italic;
        hint.raycastTarget = false;
    }

    TMP_Text SampleFont()
    {
        if (_loginEmailInput != null && _loginEmailInput.textComponent != null)
        {
            return _loginEmailInput.textComponent;
        }

        if (_regEmailInput != null)
        {
            return _regEmailInput.textComponent;
        }

        return _goLoginButton != null ? _goLoginButton.GetComponentInChildren<TMP_Text>(true) : null;
    }
}
