using UnityEngine;

[ExecuteAlways]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform _rect;
    Rect _lastSafe;
    Vector2Int _lastScreen;

    void OnEnable()
    {
        Apply();
    }

    void Update()
    {
        Apply();
    }

    public void Apply()
    {
        if (_rect == null)
        {
            _rect = transform as RectTransform;
        }

        if (_rect == null)
        {
            return;
        }

        Rect safe = Screen.safeArea;
        var screen = new Vector2Int(Screen.width, Screen.height);
        if (safe == _lastSafe && screen == _lastScreen)
        {
            return;
        }

        _lastSafe = safe;
        _lastScreen = screen;
        ApplyAnchors(ToAnchorRect(safe, screen));
    }

    public static Rect ToAnchorRect(Rect safeArea, Vector2 screenSize)
    {
        if (screenSize.x <= 0.01f || screenSize.y <= 0.01f)
        {
            return Rect.MinMaxRect(0f, 0f, 1f, 1f);
        }

        float xMin = Mathf.Clamp01(safeArea.xMin / screenSize.x);
        float yMin = Mathf.Clamp01(safeArea.yMin / screenSize.y);
        float xMax = Mathf.Clamp01(safeArea.xMax / screenSize.x);
        float yMax = Mathf.Clamp01(safeArea.yMax / screenSize.y);
        if (xMax <= xMin || yMax <= yMin)
        {
            return Rect.MinMaxRect(0f, 0f, 1f, 1f);
        }

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    void ApplyAnchors(Rect anchors)
    {
        _rect.anchorMin = new Vector2(anchors.xMin, anchors.yMin);
        _rect.anchorMax = new Vector2(anchors.xMax, anchors.yMax);
        _rect.offsetMin = Vector2.zero;
        _rect.offsetMax = Vector2.zero;
    }
}
