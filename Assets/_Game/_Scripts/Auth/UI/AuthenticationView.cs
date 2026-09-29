using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AuthenticationView : MonoBehaviour
{
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

    private IAuthService _auth;
    private TMP_Text _status;

    private const string GameSceneName = "Game";

    private void Awake()
    {
        _auth = _auth ?? new AuthService();
        // Навешиваем обработчики
        _goLoginButton?.onClick.AddListener(ShowLogin);
        _goRegisterButton?.onClick.AddListener(ShowRegister);
        _regSubmitButton?.onClick.AddListener(OnRegisterClicked);
        _loginSubmitButton?.onClick.AddListener(OnLoginClicked);

        // Начальный экран
        ShowChoice();

        // Проверка устройства — только лог в дебаге, UI не пропускаем
        if (_auth.IsDeviceRegistered())
        {
            // Требование: пока ничего не пропускать, только писать в Debug
            Debug.Log("[Auth] Игрок уже зарегистрирован — UI не пропускаем по условиям задачи");
        }
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
        EnterGame();
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
        EnterGame();
    }

    private void EnterGame()
    {
        string profileId = _auth.ActiveProfileId;
        if (!string.IsNullOrEmpty(profileId))
        {
            GameSession.BindProfile(Application.persistentDataPath, profileId, _auth.LegacySaveOwner);
        }

        SceneManager.LoadScene(GameSceneName);
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
        _status.color = new Color(1f, 0.82f, 0.45f, 1f);
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
}
