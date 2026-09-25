using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void ShowChoice()
    {
        SetPanel(_choicePanel, true);
        SetPanel(_registerPanel, false);
        SetPanel(_loginPanel, false);
    }

    private void ShowRegister()
    {
        SetPanel(_choicePanel, false);
        SetPanel(_registerPanel, true);
        SetPanel(_loginPanel, false);
    }

    private void ShowLogin()
    {
        SetPanel(_choicePanel, false);
        SetPanel(_registerPanel, false);
        SetPanel(_loginPanel, true);
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
        if (_auth.RegisterUser(email, password))
        {
            _auth.SaveDeviceId(DeviceIdentifier.Get());
            // По задаче — просто закрываем регистрацию и можно вернуться к выбору или продолжить.
            Debug.Log("[Auth] Регистрация завершена, можно продолжать игру");
            ShowChoice();
        }
    }

    private void OnLoginClicked()
    {
        string email = _loginEmailInput != null ? _loginEmailInput.text : string.Empty;
        string password = _loginPasswordInput != null ? _loginPasswordInput.text : string.Empty;
        bool ok = _auth.LoginUser(email, password);
        if (ok)
        {
            Debug.Log("[Auth] Вход выполнен, можно продолжать игру");
            ShowChoice();
        }
    }
}
