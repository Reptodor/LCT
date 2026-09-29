using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class SettingsOverlayFactory
{
    static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Gold = new Color(0.96f, 0.78f, 0.28f, 1f);
    static readonly Color Snack = new Color(0.7f, 0.32f, 0.14f, 1f);
    static readonly Color Dim = new Color(0.04f, 0.08f, 0.03f, 0.78f);
    static readonly Color Sheet = new Color(0.16f, 0.32f, 0.18f, 0.98f);
    static readonly Color Card = new Color(0.16f, 0.38f, 0.26f, 1f);
    static readonly Color Reset = new Color(0.45f, 0.28f, 0.12f, 1f);
    static readonly Color Logout = new Color(0.45f, 0.18f, 0.16f, 1f);

    public static void Build(Transform canvas, SettingsWindow view, TMP_FontAsset font)
    {
        Transform existing = canvas.Find("SettingsOverlay");
        if (existing != null)
        {
            Object.Destroy(existing.gameObject);
        }

        var overlay = new GameObject("SettingsOverlay", typeof(RectTransform), typeof(CanvasGroup));
        Stretch(overlay, canvas);
        var group = overlay.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        overlay.SetActive(false);

        var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image), typeof(Button));
        Stretch(dimGo, overlay.transform);
        var dimImg = dimGo.GetComponent<Image>();
        dimImg.color = Dim;
        dimImg.raycastTarget = true;
        var dimBtn = dimGo.GetComponent<Button>();
        dimBtn.targetGraphic = dimImg;
        dimBtn.transition = Selectable.Transition.None;

        var safe = new GameObject("SheetSafe", typeof(RectTransform), typeof(SafeAreaFitter));
        Stretch(safe, overlay.transform);
        Column(safe, new RectOffset(36, 36, 36, 36), 0f, TextAnchor.MiddleCenter);

        var sheet = new GameObject("Sheet", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        sheet.transform.SetParent(safe.transform, false);
        Paint(sheet, Sheet, true);
        var sheetLayout = sheet.GetComponent<LayoutElement>();
        sheetLayout.preferredHeight = 860f;
        sheetLayout.flexibleHeight = 0f;
        var motion = new GameObject("Motion", typeof(RectTransform));
        Stretch(motion, sheet.transform);
        Column(motion, new RectOffset(28, 28, 24, 24), 16f, TextAnchor.UpperCenter);

        var title = Label(motion.transform, "Title", "Настройки", 32f, 48f, FontStyles.Bold, Gold, TextAlignmentOptions.Center, font);
        PrefHeight(title.gameObject, 64f);
        var hint = Label(motion.transform, "Hint", "Звук и музыка запоминаются для этого профиля.", 18f, 26f, FontStyles.Normal, Cream, TextAlignmentOptions.Center, font);
        PrefHeight(hint.gameObject, 56f);

        Slider sound = VolumeRow(motion.transform, "SoundRow", "Звук", font, out TMP_Text soundLabel);
        Slider music = VolumeRow(motion.transform, "MusicRow", "Музыка", font, out TMP_Text musicLabel);
        Button reset = WideButton(motion.transform, "ResetButton", "Сбросить прогресс", Reset, Cream, font, out TMP_Text resetLabel);
        Button logout = WideButton(motion.transform, "LogoutButton", "Выйти из профиля", Logout, Cream, font, out _);
        Button close = WideButton(motion.transform, "CloseButton", "Закрыть", Snack, Cream, font, out _);

        view.Setup(
            overlay,
            dimBtn,
            close,
            sound,
            music,
            reset,
            logout,
            soundLabel,
            musicLabel,
            resetLabel,
            group,
            motion.GetComponent<RectTransform>());
    }

    static Slider VolumeRow(Transform parent, string name, string caption, TMP_FontAsset font, out TMP_Text label)
    {
        var row = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        Paint(row, Card, false);
        var rowLayout = row.GetComponent<LayoutElement>();
        rowLayout.minHeight = 148f;
        rowLayout.preferredHeight = 148f;
        rowLayout.flexibleWidth = 1f;
        var column = row.GetComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(20, 20, 14, 16);
        column.spacing = 8f;
        column.childAlignment = TextAnchor.UpperCenter;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;

        label = Label(row.transform, "Label", caption, 20f, 30f, FontStyles.Bold, Cream, TextAlignmentOptions.MidlineLeft, font);
        PrefHeight(label.gameObject, 36f);

        var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
        sliderGo.transform.SetParent(row.transform, false);
        var sliderLayout = sliderGo.GetComponent<LayoutElement>();
        sliderLayout.minHeight = 64f;
        sliderLayout.preferredHeight = 64f;
        sliderLayout.flexibleWidth = 1f;

        var track = new GameObject("Background", typeof(RectTransform), typeof(Image));
        track.transform.SetParent(sliderGo.transform, false);
        var trackRect = track.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0f, 0.5f);
        trackRect.anchorMax = new Vector2(1f, 0.5f);
        trackRect.pivot = new Vector2(0.5f, 0.5f);
        trackRect.sizeDelta = new Vector2(0f, 18f);
        Paint(track, new Color(0.08f, 0.16f, 0.1f, 1f), true);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        var fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
        fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
        fillAreaRect.sizeDelta = new Vector2(-28f, 18f);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Paint(fill, Gold, false);

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderGo.transform, false);
        var handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(18f, 0f);
        handleAreaRect.offsetMax = new Vector2(-18f, 0f);

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(44f, 44f);
        Paint(handle, Cream, true);

        var slider = sliderGo.GetComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        return slider;
    }

    static Button WideButton(Transform parent, string name, string text, Color color, Color labelColor, TMP_FontAsset font, out TMP_Text label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        Paint(go, color, true);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 92f;
        layout.preferredHeight = 92f;
        layout.flexibleWidth = 1f;
        var button = go.GetComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        label = Label(go.transform, "Label", text, 22f, 34f, FontStyles.Bold, labelColor, TextAlignmentOptions.Center, font);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        var rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(12f, 8f);
        rect.offsetMax = new Vector2(-12f, -8f);
        return button;
    }

    static TextMeshProUGUI Label(Transform parent, string name, string text, float min, float max, FontStyles style, Color color, TextAlignmentOptions align, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = min;
        tmp.fontSizeMax = max;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        if (font != null)
        {
            tmp.font = font;
        }
        else if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }

        return tmp;
    }

    static void Stretch(GameObject go, Transform parent)
    {
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void Paint(GameObject go, Color color, bool raycast)
    {
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = raycast;
    }

    static void Column(GameObject go, RectOffset padding, float spacing, TextAnchor align)
    {
        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.padding = padding;
        layout.spacing = spacing;
        layout.childAlignment = align;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    static void PrefHeight(GameObject go, float height)
    {
        var layout = go.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleHeight = 0f;
    }
}
