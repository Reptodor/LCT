using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class FinashkaLevelSelectBuilder
{
    private const string PrefabPath = "Assets/_Game/_Prefabs/UI/LevelSelect/LevelSelectWindow.prefab";
    private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    private static readonly Color Dim = new Color(0.04f, 0.08f, 0.03f, 0.78f);
    private static readonly Color Sheet = new Color(0.11f, 0.26f, 0.15f, 0.98f);
    private static readonly Color Card = new Color(0.07f, 0.16f, 0.09f, 1f);
    private static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    private static readonly Color Gold = new Color(0.96f, 0.78f, 0.28f, 1f);
    private static readonly Color Ink = new Color(0.12f, 0.16f, 0.08f, 1f);
    private static readonly Color Muted = new Color(0.86f, 0.9f, 0.78f, 1f);
    private static readonly Color Track = new Color(0.08f, 0.14f, 0.07f, 0.55f);

    private readonly struct CardSpec
    {
        public readonly string Name;
        public readonly string Title;
        public readonly string Subtitle;
        public readonly string Scene;
        public readonly Color Accent;

        public CardSpec(string name, string title, string subtitle, string scene, Color accent)
        {
            Name = name;
            Title = title;
            Subtitle = subtitle;
            Scene = scene;
            Accent = accent;
        }
    }

    [MenuItem("Finashka/Create Level Select Prefab")]
    public static void CreatePrefab()
    {
        EnsureFolders();
        Scene preview = EditorSceneManager.NewPreviewScene();
        GameObject root = Build();
        SceneManager.MoveGameObjectToScene(root, preview);
        root.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        EditorSceneManager.ClosePreviewScene(preview);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Level select prefab: " + PrefabPath);
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Game/_Prefabs/UI"))
        {
            AssetDatabase.CreateFolder("Assets/_Game/_Prefabs", "UI");
        }

        if (!AssetDatabase.IsValidFolder("Assets/_Game/_Prefabs/UI/LevelSelect"))
        {
            AssetDatabase.CreateFolder("Assets/_Game/_Prefabs/UI", "LevelSelect");
        }
    }

    private static GameObject Build()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var root = new GameObject("LevelSelectWindow", typeof(RectTransform), typeof(CanvasGroup), typeof(LevelSelectWindow));
        RectTransform rootRect = Stretch(root, null);
        var rootGroup = root.GetComponent<CanvasGroup>();
        rootGroup.alpha = 0f;
        rootGroup.blocksRaycasts = true;
        rootGroup.interactable = true;

        var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        Stretch(dimGo, root.transform);
        Image dimImage = Paint(dimGo, sprite, Dim, true);
        Button dimButton = dimGo.GetComponent<Button>();
        dimButton.targetGraphic = dimImage;
        dimButton.transition = Selectable.Transition.None;
        ClearNavigation(dimButton);

        var safe = new GameObject("Safe", typeof(RectTransform), typeof(SafeAreaFitter), typeof(VerticalLayoutGroup));
        Stretch(safe, root.transform);
        VerticalLayoutGroup safeLayout = safe.GetComponent<VerticalLayoutGroup>();
        safeLayout.padding = new RectOffset(28, 28, 20, 24);
        safeLayout.spacing = 0f;
        safeLayout.childAlignment = TextAnchor.MiddleCenter;
        safeLayout.childControlWidth = true;
        safeLayout.childControlHeight = true;
        safeLayout.childForceExpandWidth = true;
        safeLayout.childForceExpandHeight = true;

        var slot = new GameObject("SheetSlot", typeof(RectTransform), typeof(LayoutElement));
        slot.transform.SetParent(safe.transform, false);
        LayoutElement slotLayout = slot.GetComponent<LayoutElement>();
        slotLayout.flexibleWidth = 1f;
        slotLayout.flexibleHeight = 1f;

        var sheetGo = new GameObject("Sheet", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform sheet = sheetGo.GetComponent<RectTransform>();
        sheet.SetParent(slot.transform, false);
        sheet.pivot = new Vector2(0.5f, 0.5f);
        sheet.anchorMin = Vector2.zero;
        sheet.anchorMax = Vector2.one;
        sheet.offsetMin = Vector2.zero;
        sheet.offsetMax = Vector2.zero;
        sheet.localScale = Vector3.one;
        Paint(sheetGo, sprite, Sheet, true);

        var column = new GameObject("Column", typeof(RectTransform), typeof(VerticalLayoutGroup));
        Stretch(column, sheet);
        VerticalLayoutGroup columnLayout = column.GetComponent<VerticalLayoutGroup>();
        columnLayout.padding = new RectOffset(28, 28, 28, 22);
        columnLayout.spacing = 14f;
        columnLayout.childAlignment = TextAnchor.UpperCenter;
        columnLayout.childControlWidth = true;
        columnLayout.childControlHeight = true;
        columnLayout.childForceExpandWidth = true;
        columnLayout.childForceExpandHeight = false;

        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        header.transform.SetParent(column.transform, false);
        PrefHeight(header, 92f);
        HorizontalLayoutGroup headerLayout = header.GetComponent<HorizontalLayoutGroup>();
        headerLayout.spacing = 16f;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = true;
        headerLayout.childForceExpandHeight = true;
        TextMeshProUGUI title = Label(header.transform, "Title", "Мини-игры", 36f, 56f, FontStyles.Bold, Gold, TextAlignmentOptions.MidlineLeft, font);
        title.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
        Button closeButton = ButtonBox(header.transform, "CloseButton", "Закрыть", Track, Cream, sprite, font, 196f, 84f);

        TextMeshProUGUI hint = Label(column.transform, "Hint", "Выбери игру. Если не влезла — листай вниз.", 22f, 32f, FontStyles.Normal, Muted, TextAlignmentOptions.MidlineLeft, font);
        PrefHeight(hint.gameObject, 72f);

        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
        scrollGo.transform.SetParent(column.transform, false);
        LayoutElement scrollLayout = scrollGo.GetComponent<LayoutElement>();
        scrollLayout.flexibleHeight = 1f;
        scrollLayout.minHeight = 480f;
        scrollLayout.flexibleWidth = 1f;
        Image scrollImage = Paint(scrollGo, sprite, new Color(0f, 0f, 0f, 0f), false);
        scrollImage.raycastTarget = false;

        var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        RectTransform viewport = Stretch(viewportGo, scrollGo.transform);
        Image viewportImage = Paint(viewportGo, sprite, new Color(0f, 0f, 0f, 0.01f), true);

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform content = contentGo.GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 0f);
        VerticalLayoutGroup list = contentGo.GetComponent<VerticalLayoutGroup>();
        list.padding = new RectOffset(4, 18, 6, 28);
        list.spacing = 18f;
        list.childAlignment = TextAnchor.UpperCenter;
        list.childControlWidth = true;
        list.childControlHeight = true;
        list.childForceExpandWidth = true;
        list.childForceExpandHeight = false;
        ContentSizeFitter fitter = contentGo.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Scrollbar scrollbar = CreateScrollbar(scrollGo.transform, sprite);
        ScrollRect scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.12f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f;
        scroll.scrollSensitivity = 28f;
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        scroll.verticalScrollbarSpacing = 6f;

        CardSpec[] specs =
        {
            new CardSpec("Card_BudgetEnvelopes", "Конверты бюджета", "Разложи деньги по трём конвертам", "BudgetEnvelopes", new Color(0.36f, 0.72f, 0.42f, 1f)),
            new CardSpec("Card_BudgetWeek", "Неделя на бюджете", "Доживи до воскресенья с запасом", "BudgetWeek", new Color(0.93f, 0.64f, 0.18f, 1f)),
            new CardSpec("Card_DreamGoal", "Путь к мечте", "Откладывай каждую неделю", "DreamGoal", new Color(0.62f, 0.48f, 0.86f, 1f)),
            new CardSpec("Card_PiggyCatch", "Копилка", "Лови монеты и копи на мечту", "PiggyCatch", new Color(0.93f, 0.45f, 0.48f, 1f)),
            new CardSpec("Card_Exchange", "Обмен", "Собери сумму монетами и купюрами", "Exchange", new Color(0.28f, 0.62f, 0.78f, 1f)),
            new CardSpec("Card_Shop", "Магазин", "Покупай и считай сдачу", "Shop", new Color(0.86f, 0.48f, 0.22f, 1f)),
            new CardSpec("Card_AR", "AR", "Оживи своего персонажа", "PetAR", new Color(0.96f, 0.78f, 0.28f, 1f))
        };

        var cards = new LevelSelectCard[specs.Length];
        for (int i = 0; i < specs.Length; i++)
        {
            cards[i] = CreateCard(content, specs[i], i + 1, sprite, font, scroll);
        }

        var window = root.GetComponent<LevelSelectWindow>();
        SerializedObject serialized = new SerializedObject(window);
        serialized.FindProperty("_rootGroup").objectReferenceValue = rootGroup;
        serialized.FindProperty("_sheet").objectReferenceValue = sheet;
        serialized.FindProperty("_dimButton").objectReferenceValue = dimButton;
        serialized.FindProperty("_closeButton").objectReferenceValue = closeButton;
        serialized.FindProperty("_scroll").objectReferenceValue = scroll;
        serialized.FindProperty("_content").objectReferenceValue = content;
        SerializedProperty cardArray = serialized.FindProperty("_cards");
        cardArray.arraySize = cards.Length;
        for (int i = 0; i < cards.Length; i++)
        {
            cardArray.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        rootRect.localScale = Vector3.one;
        return root;
    }

    private static LevelSelectCard CreateCard(Transform parent, CardSpec spec, int index, Sprite sprite, TMP_FontAsset font, ScrollRect scroll)
    {
        var cardGo = new GameObject(spec.Name, typeof(RectTransform), typeof(LayoutElement), typeof(Button), typeof(LevelSelectCard), typeof(LevelSelectScrollRelay));
        cardGo.transform.SetParent(parent, false);
        LayoutElement layout = cardGo.GetComponent<LayoutElement>();
        layout.minHeight = 208f;
        layout.preferredHeight = 228f;
        layout.flexibleWidth = 1f;
        layout.flexibleHeight = 0f;

        var visualGo = new GameObject("Visual", typeof(RectTransform), typeof(CanvasGroup));
        RectTransform visual = Stretch(visualGo, cardGo.transform);
        visual.pivot = new Vector2(0.5f, 0.5f);
        CanvasGroup group = visualGo.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = true;
        group.interactable = true;

        var backGo = new GameObject("Back", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Stretch(backGo, visual);
        Image back = Paint(backGo, sprite, Card, true);

        var accentGo = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform accent = accentGo.GetComponent<RectTransform>();
        accent.SetParent(visual, false);
        accent.anchorMin = new Vector2(0f, 0f);
        accent.anchorMax = new Vector2(0f, 1f);
        accent.pivot = new Vector2(0f, 0.5f);
        accent.anchoredPosition = new Vector2(16f, 0f);
        accent.sizeDelta = new Vector2(14f, -36f);
        Paint(accentGo, sprite, spec.Accent, false);

        var badgeGo = new GameObject("Index", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform badge = badgeGo.GetComponent<RectTransform>();
        badge.SetParent(visual, false);
        badge.anchorMin = new Vector2(0f, 0.5f);
        badge.anchorMax = new Vector2(0f, 0.5f);
        badge.pivot = new Vector2(0f, 0.5f);
        badge.anchoredPosition = new Vector2(46f, 0f);
        badge.sizeDelta = new Vector2(64f, 64f);
        Paint(badgeGo, sprite, spec.Accent, false);
        TextMeshProUGUI indexLabel = Label(badge.transform, "Value", index.ToString(), 26f, 36f, FontStyles.Bold, Ink, TextAlignmentOptions.Center, font);
        indexLabel.outlineWidth = 0f;
        Stretch(indexLabel.gameObject, badge);

        TextMeshProUGUI title = Label(visual, "Title", spec.Title, 30f, 42f, FontStyles.Bold, Gold, TextAlignmentOptions.MidlineLeft, font);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 0.48f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(128f, 0f);
        titleRect.offsetMax = new Vector2(-24f, -18f);

        TextMeshProUGUI subtitle = Label(visual, "Subtitle", spec.Subtitle, 20f, 28f, FontStyles.Normal, Cream, TextAlignmentOptions.TopLeft, font);
        RectTransform subtitleRect = subtitle.rectTransform;
        subtitleRect.anchorMin = new Vector2(0f, 0f);
        subtitleRect.anchorMax = new Vector2(1f, 0.52f);
        subtitleRect.offsetMin = new Vector2(128f, 18f);
        subtitleRect.offsetMax = new Vector2(-24f, -6f);

        Button button = cardGo.GetComponent<Button>();
        button.targetGraphic = back;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.94f, 0.94f, 0.94f, 1f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.85f);
        colors.fadeDuration = 0.08f;
        colors.colorMultiplier = 1f;
        button.colors = colors;
        ClearNavigation(button);

        LevelSelectCard card = cardGo.GetComponent<LevelSelectCard>();
        SerializedObject cardObject = new SerializedObject(card);
        cardObject.FindProperty("_button").objectReferenceValue = button;
        cardObject.FindProperty("_visual").objectReferenceValue = visual;
        cardObject.FindProperty("_group").objectReferenceValue = group;
        cardObject.FindProperty("_title").objectReferenceValue = title;
        cardObject.FindProperty("_subtitle").objectReferenceValue = subtitle;
        cardObject.FindProperty("_sceneName").stringValue = spec.Scene;
        cardObject.ApplyModifiedPropertiesWithoutUndo();

        LevelSelectScrollRelay relay = cardGo.GetComponent<LevelSelectScrollRelay>();
        SerializedObject relayObject = new SerializedObject(relay);
        relayObject.FindProperty("_scroll").objectReferenceValue = scroll;
        relayObject.ApplyModifiedPropertiesWithoutUndo();
        return card;
    }

    private static Scrollbar CreateScrollbar(Transform parent, Sprite sprite)
    {
        var barGo = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
        RectTransform barRect = barGo.GetComponent<RectTransform>();
        barRect.SetParent(parent, false);
        barRect.anchorMin = new Vector2(1f, 0f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(1f, 0.5f);
        barRect.sizeDelta = new Vector2(14f, 0f);
        barRect.anchoredPosition = new Vector2(-4f, 0f);
        Image background = Paint(barGo, sprite, new Color(1f, 1f, 1f, 0.08f), false);

        var sliding = new GameObject("Sliding Area", typeof(RectTransform));
        RectTransform slidingRect = sliding.GetComponent<RectTransform>();
        slidingRect.SetParent(barRect, false);
        slidingRect.anchorMin = Vector2.zero;
        slidingRect.anchorMax = Vector2.one;
        slidingRect.offsetMin = new Vector2(2f, 8f);
        slidingRect.offsetMax = new Vector2(-2f, -8f);

        var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform handleRect = Stretch(handleGo, sliding.transform);
        Image handle = Paint(handleGo, sprite, Gold, true);

        Scrollbar scrollbar = barGo.GetComponent<Scrollbar>();
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handle;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        ColorBlock colors = scrollbar.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        scrollbar.colors = colors;
        background.raycastTarget = true;
        return scrollbar;
    }

    private static Button ButtonBox(Transform parent, string name, string text, Color fill, Color label, Sprite sprite, TMP_FontAsset font, float width, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        LayoutElement layout = go.GetComponent<LayoutElement>();
        layout.minWidth = width;
        layout.preferredWidth = width;
        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleWidth = 0f;
        Image image = Paint(go, sprite, fill, true);
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.fadeDuration = 0.08f;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        button.colors = colors;
        ClearNavigation(button);
        TextMeshProUGUI caption = Label(go.transform, "Label", text, 22f, 32f, FontStyles.Bold, label, TextAlignmentOptions.Center, font);
        Stretch(caption.gameObject, go.transform);
        return button;
    }

    private static TextMeshProUGUI Label(Transform parent, string name, string text, float min, float max, FontStyles style, Color color, TextAlignmentOptions align, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var layout = go.AddComponent<LayoutElement>();
        layout.flexibleWidth = 1f;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null)
        {
            tmp.font = font;
        }

        tmp.text = text;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = min;
        tmp.fontSizeMax = max;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        tmp.outlineColor = new Color(0.05f, 0.1f, 0.04f, 0.9f);
        tmp.outlineWidth = 0.14f;
        tmp.margin = new Vector4(6f, 2f, 6f, 2f);
        return tmp;
    }

    private static RectTransform Stretch(GameObject go, Transform parent)
    {
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        return rect;
    }

    private static Image Paint(GameObject go, Sprite sprite, Color color, bool raycast)
    {
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = raycast;
        return image;
    }

    private static void PrefHeight(GameObject go, float height)
    {
        LayoutElement layout = go.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = go.AddComponent<LayoutElement>();
        }

        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleHeight = 0f;
    }

    private static void ClearNavigation(Selectable selectable)
    {
        Navigation navigation = selectable.navigation;
        navigation.mode = Navigation.Mode.None;
        selectable.navigation = navigation;
    }
}
