using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MiniGameExit
{
    const float ButtonLeft = 28f;
    const float ButtonTop = 22f;
    const float ButtonWidth = 220f;
    const float ButtonHeight = 84f;
    const float Gap = 28f;

    static readonly string[] Scenes =
    {
        "Shop",
        "Exchange",
        "BudgetEnvelopes",
        "BudgetWeek",
        "DreamGoal",
        "PiggyCatch"
    };

    static bool _leaving;
    static Sprite _sprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    public static void Return()
    {
        if (_leaving)
        {
            return;
        }

        _leaving = true;
        SceneManager.LoadScene(BootController.GameSceneName);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _leaving = false;
        if (!IsMiniGame(scene.name))
        {
            return;
        }

        Canvas canvas = FindGameCanvas();
        if (canvas == null || canvas.transform.Find("MiniGameExit") != null)
        {
            return;
        }

        float notch = Notch(canvas);
        if (canvas.gameObject.name == "FinanceCanvas")
        {
            InsetPlayfield(canvas.transform.Find("Safe"));
        }
        else
        {
            ReserveTop(canvas.transform as RectTransform, notch + Clearance);
        }

        BuildButton(canvas, notch);
    }

    static bool IsMiniGame(string sceneName)
    {
        for (int i = 0; i < Scenes.Length; i++)
        {
            if (Scenes[i] == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    static float Clearance => ButtonTop + ButtonHeight + Gap;

    static float Notch(Canvas canvas)
    {
        float scale = canvas.scaleFactor > 0.01f ? canvas.scaleFactor : 1f;
        return Mathf.Max(0f, (Screen.height - Screen.safeArea.yMax) / scale);
    }

    static void InsetPlayfield(Transform safe)
    {
        if (safe == null)
        {
            return;
        }

        Inset(safe.Find("Game"));
        Inset(safe.Find("Drag"));
        Inset(safe.Find("Fx"));
    }

    static void Inset(Transform target)
    {
        var rect = target as RectTransform;
        if (rect == null)
        {
            return;
        }

        Vector2 max = rect.offsetMax;
        max.y = -Clearance;
        rect.offsetMax = max;
    }

    static void ReserveTop(RectTransform canvas, float band)
    {
        if (canvas == null)
        {
            return;
        }

        for (int i = 0; i < canvas.childCount; i++)
        {
            var child = canvas.GetChild(i) as RectTransform;
            if (child == null || child.name == "Background" || child.name == "MiniGameExit")
            {
                continue;
            }

            if (child.anchorMax.y < 0.98f || child.anchorMax.y - child.anchorMin.y < 0.05f)
            {
                continue;
            }

            Vector2 max = child.offsetMax;
            max.y = Mathf.Min(max.y, 0f) - band;
            child.offsetMax = max;
            if (child.name == "Panel_HUD")
            {
                PullInside(child);
            }
        }
    }

    static void PullInside(RectTransform panel)
    {
        Canvas.ForceUpdateCanvases();
        float top = panel.rect.yMax;
        var corners = new Vector3[4];
        for (int i = 0; i < panel.childCount; i++)
        {
            var child = panel.GetChild(i) as RectTransform;
            if (child == null)
            {
                continue;
            }

            child.GetWorldCorners(corners);
            float childTop = panel.InverseTransformPoint(corners[1]).y;
            float overflow = childTop - top;
            if (overflow > 1f)
            {
                child.anchoredPosition -= new Vector2(0f, overflow + 8f);
            }
        }
    }

    static void BuildButton(Canvas canvas, float notch)
    {
        var host = new GameObject("MiniGameExit", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(Image), typeof(Button));
        host.layer = 5;
        host.transform.SetParent(canvas.transform, false);
        host.transform.SetAsLastSibling();

        var layer = host.GetComponent<Canvas>();
        layer.overrideSorting = true;
        layer.sortingOrder = 400;

        var rect = (RectTransform)host.transform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        float scale = canvas.scaleFactor > 0.01f ? canvas.scaleFactor : 1f;
        float left = Screen.safeArea.xMin / scale + ButtonLeft;
        rect.anchoredPosition = new Vector2(left, -(notch + ButtonTop));
        rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

        var image = host.GetComponent<Image>();
        image.sprite = Sprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(0.12f, 0.34f, 0.2f, 0.96f);
        image.raycastTarget = true;

        var button = host.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(Return);

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.layer = 5;
        labelGo.transform.SetParent(host.transform, false);
        var labelRect = (RectTransform)labelGo.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(12f, 8f);
        labelRect.offsetMax = new Vector2(-12f, -8f);
        var label = labelGo.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "Выход";
        label.fontSize = 40;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(1f, 0.97f, 0.9f, 1f);
        label.raycastTarget = false;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 24;
        label.resizeTextMaxSize = 40;
    }

    static Canvas FindGameCanvas()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas fallback = null;
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (!canvas.isRootCanvas || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                continue;
            }

            if (canvas.gameObject.name == "FinanceCanvas" || canvas.transform.Find("Panel_HUD") != null)
            {
                return canvas;
            }

            fallback = canvas;
        }

        return fallback;
    }

    static Sprite Sprite()
    {
        if (_sprite != null)
        {
            return _sprite;
        }

        const int size = 48;
        const int radius = 16;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        var clear = new Color(1f, 1f, 1f, 0f);
        var solid = Color.white;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x - (size - 1) * 0.5f) - (size * 0.5f - radius), 0f);
                float dy = Mathf.Max(Mathf.Abs(y - (size - 1) * 0.5f) - (size * 0.5f - radius), 0f);
                texture.SetPixel(x, y, dx * dx + dy * dy > radius * radius ? clear : solid);
            }
        }

        texture.Apply();
        _sprite = UnityEngine.Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            1f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        return _sprite;
    }
}
