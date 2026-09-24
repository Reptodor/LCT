using UnityEngine;

public static class DeviceIdentifier
{
    public static string Get()
    {
        // Для прототипа используем стандартный идентификатор Unity.
        // На iOS/Android он стабильный для установки, на Editor/Standalone может меняться между сессиями.
        return SystemInfo.deviceUniqueIdentifier ?? string.Empty;
    }
}
