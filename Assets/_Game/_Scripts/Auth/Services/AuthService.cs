using UnityEngine;

public interface IAuthService
{
    bool HasSavedCredentials();
    UserCredentials GetSavedCredentials();
    bool RegisterUser(string email, string password);
    bool LoginUser(string email, string password);
    void SaveDeviceId(string deviceId);
    bool IsDeviceRegistered();
}

public sealed class AuthService : IAuthService
{
    private const string KeyEmail = "auth_email";
    private const string KeyPassword = "auth_password";
    private const string KeyDeviceId = "device_id";
    
    // Вынесем тримминг и простые проверки в хелперы
    private static string Sanitize(string s) => (s ?? string.Empty).Trim();

    public bool HasSavedCredentials()
    {
        return PlayerPrefs.HasKey(KeyEmail) && PlayerPrefs.HasKey(KeyPassword);
    }

    public UserCredentials GetSavedCredentials()
    {
        if (!HasSavedCredentials())
        {
            return null;
        }

        string email = PlayerPrefs.GetString(KeyEmail, string.Empty);
        string password = PlayerPrefs.GetString(KeyPassword, string.Empty);
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        return new UserCredentials(email, password);
    }

    public bool RegisterUser(string email, string password)
    {
        email = Sanitize(email);
        password = Sanitize(password);
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("[Auth] Email и пароль должны быть не пустыми");
            return false;
        }

        PlayerPrefs.SetString(KeyEmail, email);
        PlayerPrefs.SetString(KeyPassword, password);
        PlayerPrefs.Save();
        Debug.Log($"[Auth] Регистрация успешна: {email}");
        return true;
    }

    public bool LoginUser(string email, string password)
    {
        var saved = GetSavedCredentials();
        if (saved == null)
        {
            Debug.LogWarning("[Auth] Нет зарегистрированного пользователя");
            return false;
        }

        email = Sanitize(email);
        password = Sanitize(password);
        bool ok = saved.Email == email && saved.Password == password;
        if (ok)
        {
            Debug.Log($"[Auth] Вход успешен: {email}");
        }
        else
        {
            Debug.LogWarning("[Auth] Ошибка входа: неверные данные");
        }

        return ok;
    }

    public void SaveDeviceId(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            return;
        }

        PlayerPrefs.SetString(KeyDeviceId, deviceId);
        PlayerPrefs.Save();
    }

    public bool IsDeviceRegistered()
    {
        string saved = PlayerPrefs.GetString(KeyDeviceId, string.Empty);
        string current = DeviceIdentifier.Get();
        bool registered = !string.IsNullOrEmpty(saved) && saved == current && HasSavedCredentials();
        if (registered)
        {
            Debug.Log("[Auth] Игрок уже зарегистрирован (устройство распознано)");
        }
        return registered;
    }
}
