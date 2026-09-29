using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface IAuthService
{
    bool HasSavedCredentials();
    UserCredentials GetSavedCredentials();
    bool RegisterUser(string email, string password);
    bool LoginUser(string email, string password);
    void SaveDeviceId(string deviceId);
    bool IsDeviceRegistered();
    string ActiveProfileId { get; }
    string LegacySaveOwner { get; }
    void Logout();
}

public sealed class AuthService : IAuthService
{
    const string KeyEmail = "auth_email";
    const string KeyPassword = "auth_password";
    const string KeyDeviceId = "device_id";
    const string AccountsFileName = "accounts.json";

    readonly string _accountsPath;
    AccountRegistry _registry;

    public AuthService()
        : this(
            Path.Combine(Application.persistentDataPath, AccountsFileName),
            PlayerPrefs.GetString(KeyEmail, string.Empty),
            PlayerPrefs.GetString(KeyPassword, string.Empty))
    {
    }

    public AuthService(string accountsPath)
        : this(accountsPath, string.Empty, string.Empty)
    {
    }

    public AuthService(string accountsPath, string legacyEmail, string legacyPassword)
    {
        if (string.IsNullOrWhiteSpace(accountsPath))
        {
            throw new ArgumentException("Accounts file path is required.", nameof(accountsPath));
        }

        _accountsPath = accountsPath;
        _registry = Load(legacyEmail, legacyPassword);
    }

    public string ActiveProfileId => _registry.activeEmail ?? string.Empty;

    public string LegacySaveOwner => _registry.legacyOwnerEmail ?? string.Empty;

    public bool HasSavedCredentials()
    {
        return _registry.accounts != null && _registry.accounts.Length > 0;
    }

    public UserCredentials GetSavedCredentials()
    {
        StoredAccount account = Find(ActiveProfileId);
        if (account == null)
        {
            return null;
        }

        return new UserCredentials(account.email, account.password);
    }

    public bool RegisterUser(string email, string password)
    {
        email = NormalizeEmail(email);
        password = Sanitize(password);
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("[Auth] Email и пароль должны быть не пустыми");
            return false;
        }

        if (Find(email) != null)
        {
            Debug.LogWarning("[Auth] Такой профиль уже есть");
            return false;
        }

        var list = new List<StoredAccount>();
        if (_registry.accounts != null)
        {
            list.AddRange(_registry.accounts);
        }

        list.Add(new StoredAccount { email = email, password = password });
        _registry.accounts = list.ToArray();
        _registry.activeEmail = email;
        Write(_registry);
        Debug.Log($"[Auth] Регистрация успешна: {email}");
        return true;
    }

    public bool LoginUser(string email, string password)
    {
        email = NormalizeEmail(email);
        password = Sanitize(password);
        StoredAccount account = Find(email);
        if (account == null || account.password != password)
        {
            Debug.LogWarning("[Auth] Ошибка входа: неверные данные");
            return false;
        }

        _registry.activeEmail = account.email;
        Write(_registry);
        Debug.Log($"[Auth] Вход успешен: {email}");
        return true;
    }

    public void Logout()
    {
        _registry.activeEmail = string.Empty;
        Write(_registry);
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

    StoredAccount Find(string email)
    {
        if (string.IsNullOrEmpty(email) || _registry.accounts == null)
        {
            return null;
        }

        for (int i = 0; i < _registry.accounts.Length; i++)
        {
            StoredAccount account = _registry.accounts[i];
            if (account != null && account.email == email)
            {
                return account;
            }
        }

        return null;
    }

    AccountRegistry Load(string legacyEmail, string legacyPassword)
    {
        AccountRegistry registry = new AccountRegistry();
        if (File.Exists(_accountsPath))
        {
            try
            {
                AccountRegistry loaded = JsonUtility.FromJson<AccountRegistry>(File.ReadAllText(_accountsPath));
                if (loaded != null)
                {
                    registry = loaded;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Auth] Accounts unreadable, starting empty: {ex.Message}");
            }
        }

        if (registry.accounts == null)
        {
            registry.accounts = new StoredAccount[0];
        }

        if (registry.activeEmail == null)
        {
            registry.activeEmail = string.Empty;
        }

        if (registry.legacyOwnerEmail == null)
        {
            registry.legacyOwnerEmail = string.Empty;
        }

        if (registry.accounts.Length == 0)
        {
            ImportLegacy(registry, legacyEmail, legacyPassword);
        }

        return registry;
    }

    void ImportLegacy(AccountRegistry registry, string legacyEmail, string legacyPassword)
    {
        string email = NormalizeEmail(legacyEmail);
        string password = Sanitize(legacyPassword);
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return;
        }

        registry.accounts = new[]
        {
            new StoredAccount { email = email, password = password }
        };
        registry.activeEmail = string.Empty;
        if (string.IsNullOrEmpty(registry.legacyOwnerEmail))
        {
            registry.legacyOwnerEmail = email;
        }

        Write(registry);
    }

    void Write(AccountRegistry registry)
    {
        try
        {
            string directory = Path.GetDirectoryName(_accountsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_accountsPath, JsonUtility.ToJson(registry, true));
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Auth] Failed to write accounts: {ex.Message}");
        }
    }

    static string NormalizeEmail(string email)
    {
        return Sanitize(email).ToLowerInvariant();
    }

    static string Sanitize(string value)
    {
        return (value ?? string.Empty).Trim();
    }

}

[Serializable]
public class AccountRegistry
{
    public StoredAccount[] accounts = new StoredAccount[0];
    public string activeEmail = "";
    public string legacyOwnerEmail = "";
}

[Serializable]
public class StoredAccount
{
    public string email = "";
    public string password = "";
}
