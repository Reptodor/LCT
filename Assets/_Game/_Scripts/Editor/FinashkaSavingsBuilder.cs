using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class FinashkaSavingsBuilder
{
    private const string PrefabPath = "Assets/_Game/_Prefabs/UI/Savings/SavingsWindow.prefab";
    private const string CatalogPath = "Assets/_Game/Resources/SavingsGoals.asset";
    private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    private static readonly Color Dim = new Color(0.04f, 0.08f, 0.03f, 0.78f);
    private static readonly Color Sheet = new Color(0.11f, 0.26f, 0.15f, 0.98f);
    private static readonly Color Card = new Color(0.07f, 0.16f, 0.09f, 1f);
    private static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    private static readonly Color Gold = new Color(0.96f, 0.78f, 0.28f, 1f);
    private static readonly Color Muted = new Color(0.86f, 0.9f, 0.78f, 1f);
    private static readonly Color Track = new Color(0.08f, 0.14f, 0.07f, 0.55f);
    private static readonly Color BarTrack = new Color(0.05f, 0.1f, 0.06f, 1f);

    [MenuItem("Finashka/Create Savings Window Prefab")]
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
        Debug.Log("Savings window prefab: " + PrefabPath);
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Game/_Prefabs/UI"))
        {
            AssetDatabase.CreateFolder("Assets/_Game/_Prefabs", "UI");
        }

        if (!AssetDatabase.IsValidFolder("Assets/_Game/_Prefabs/UI/Savings"))
        {
            AssetDatabase.CreateFolder("Assets/_Game/_Prefabs/UI", "Savings");
        }
    }

    private static GameObject Build()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        SavingsGoalCatalog catalog = AssetDatabase.LoadAssetAtPath<SavingsGoalCatalog>(CatalogPath);

        var root = new GameObject("SavingsWindow", typeof(RectTransform), typeof(CanvasGroup), typeof(SavingsWindow));
        Stretch(root, null);
        CanvasGroup rootGroup = root.GetComponent<CanvasGroup>();
        rootGroup.alpha = 1f;

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
        sheet.anchorMin = Vector2.zero;
        sheet.anchorMax = Vector2.one;
        sheet.offsetMin = Vector2.zero;
        sheet.offsetMax = Vector2.zero;
        Paint(sheetGo, sprite, Sheet, true);

        var column = new GameObject("Column", typeof(RectTransform), typeof(VerticalLayoutGroup));
        Stretch(column, sheet);
        VerticalLayoutGroup columnLayout = column.GetComponent<VerticalLayoutGroup>();
        columnLayout.padding = new RectOffset(28, 28, 28, 22);
        columnLayout.spacing = 16f;
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
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = true;
        TextMeshProUGUI title = Label(header.transform, "Title", "Накопления", 36f, 56f, FontStyles.Bold, Gold, TextAlignmentOptions.MidlineLeft, font);
        title.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
        Button closeButton = ButtonBox(header.transform, "CloseButton", "Закрыть", Track, Cream, sprite, font, 196f, 84f);

        TextMeshProUGUI balance = Label(column.transform, "Balance", "На счету: 0", 24f, 36f, FontStyles.Bold, Cream, TextAlignmentOptions.MidlineLeft, font);
        PrefHeight(balance.gameObject, 52f);

        TextMeshProUGUI goalTitle = Label(column.transform, "GoalTitle", "Выбери цель", 34f, 52f, FontStyles.Bold, Gold, TextAlignmentOptions.Center, font);
        PrefHeight(goalTitle.gameObject, 84f);

        var trackGo = new GameObject("BarTrack", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        trackGo.transform.SetParent(column.transform, false);
        PrefHeight(trackGo, 36f);
        Paint(trackGo, sprite, BarTrack, false);

        var fillGo = new GameObject("BarFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Stretch(fillGo, trackGo.transform);
        Image fill = Paint(fillGo, sprite, Gold, false);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0f;

        TextMeshProUGUI amount = Label(column.transform, "Amount", "0 / 0", 24f, 36f, FontStyles.Bold, Cream, TextAlignmentOptions.Center, font);
        PrefHeight(amount.gameObject, 48f);

        Button deposit = ButtonBox(column.transform, "DepositButton", "Пополнить", Gold, new Color(0.12f, 0.16f, 0.08f, 1f), sprite, font, 0f, 96f);
        LayoutElement depositLayout = deposit.GetComponent<LayoutElement>();
        depositLayout.minWidth = 0f;
        depositLayout.preferredWidth = -1f;
        depositLayout.flexibleWidth = 1f;
        TMP_Text depositLabel = deposit.GetComponentInChildren<TMP_Text>();

        TextMeshProUGUI status = Label(column.transform, "Status", "", 20f, 28f, FontStyles.Normal, Muted, TextAlignmentOptions.Center, font);
        PrefHeight(status.gameObject, 40f);

        TextMeshProUGUI hint = Label(column.transform, "Hint", "Выбери цель и копи на неё.", 22f, 30f, FontStyles.Normal, Muted, TextAlignmentOptions.MidlineLeft, font);
        PrefHeight(hint.gameObject, 48f);

        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
        scrollGo.transform.SetParent(column.transform, false);
        LayoutElement scrollLayout = scrollGo.GetComponent<LayoutElement>();
        scrollLayout.flexibleHeight = 1f;
        scrollLayout.minHeight = 280f;
        scrollLayout.flexibleWidth = 1f;
        Image scrollImage = Paint(scrollGo, sprite, new Color(0f, 0f, 0f, 0f), false);
        scrollImage.raycastTarget = false;
        ScrollRect scroll = scrollGo.GetComponent<ScrollRect>();

        var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        RectTransform viewport = Stretch(viewportGo, scrollGo.transform);
        Paint(viewportGo, sprite, new Color(0f, 0f, 0f, 0.01f), true);

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform content = contentGo.GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.sizeDelta = new Vector2(0f, 0f);
        VerticalLayoutGroup contentLayout = contentGo.GetComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(4, 8, 4, 16);
        contentLayout.spacing = 12f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        ContentSizeFitter fitter = contentGo.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.12f;
        scroll.scrollSensitivity = 28f;

        SavingsGoalButton template = GoalTemplate(root.transform, sprite, font, scroll);

        SerializedObject windowObject = new SerializedObject(root.GetComponent<SavingsWindow>());
        windowObject.FindProperty("_rootGroup").objectReferenceValue = rootGroup;
        windowObject.FindProperty("_sheet").objectReferenceValue = sheet;
        windowObject.FindProperty("_dimButton").objectReferenceValue = dimButton;
        windowObject.FindProperty("_closeButton").objectReferenceValue = closeButton;
        windowObject.FindProperty("_balanceLabel").objectReferenceValue = balance;
        windowObject.FindProperty("_goalTitle").objectReferenceValue = goalTitle;
        windowObject.FindProperty("_amountLabel").objectReferenceValue = amount;
        windowObject.FindProperty("_fill").objectReferenceValue = fill;
        windowObject.FindProperty("_depositButton").objectReferenceValue = deposit;
        windowObject.FindProperty("_depositLabel").objectReferenceValue = depositLabel;
        windowObject.FindProperty("_status").objectReferenceValue = status;
        windowObject.FindProperty("_goalContent").objectReferenceValue = content;
        windowObject.FindProperty("_goalTemplate").objectReferenceValue = template;
        windowObject.FindProperty("_catalog").objectReferenceValue = catalog;
        windowObject.ApplyModifiedPropertiesWithoutUndo();
        return root;
    }

    private static SavingsGoalButton GoalTemplate(Transform parent, Sprite sprite, TMP_FontAsset font, ScrollRect scroll)
    {
        var card = new GameObject("GoalTemplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(SavingsGoalButton), typeof(LevelSelectScrollRelay));
        card.transform.SetParent(parent, false);
        card.SetActive(false);
        LayoutElement layout = card.GetComponent<LayoutElement>();
        layout.minHeight = 112f;
        layout.preferredHeight = 120f;
        layout.flexibleWidth = 1f;
        Image image = Paint(card, sprite, Card, true);
        Button button = card.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ClearNavigation(button);

        TextMeshProUGUI title = Label(card.transform, "Title", "Цель", 26f, 36f, FontStyles.Bold, Gold, TextAlignmentOptions.MidlineLeft, font);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 0.42f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(24f, 0f);
        titleRect.offsetMax = new Vector2(-24f, -8f);

        TextMeshProUGUI progress = Label(card.transform, "Progress", "0 / 0", 20f, 28f, FontStyles.Normal, Cream, TextAlignmentOptions.MidlineLeft, font);
        RectTransform progressRect = progress.rectTransform;
        progressRect.anchorMin = new Vector2(0f, 0f);
        progressRect.anchorMax = new Vector2(1f, 0.48f);
        progressRect.offsetMin = new Vector2(24f, 8f);
        progressRect.offsetMax = new Vector2(-24f, 0f);

        SavingsGoalButton view = card.GetComponent<SavingsGoalButton>();
        SerializedObject viewObject = new SerializedObject(view);
        viewObject.FindProperty("_button").objectReferenceValue = button;
        viewObject.FindProperty("_image").objectReferenceValue = image;
        viewObject.FindProperty("_title").objectReferenceValue = title;
        viewObject.FindProperty("_progress").objectReferenceValue = progress;
        viewObject.ApplyModifiedPropertiesWithoutUndo();

        LevelSelectScrollRelay relay = card.GetComponent<LevelSelectScrollRelay>();
        SerializedObject relayObject = new SerializedObject(relay);
        relayObject.FindProperty("_scroll").objectReferenceValue = scroll;
        relayObject.ApplyModifiedPropertiesWithoutUndo();
        return view;
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
        layout.flexibleWidth = width <= 0f ? 1f : 0f;
        Image image = Paint(go, sprite, fill, true);
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        ClearNavigation(button);
        TextMeshProUGUI caption = Label(go.transform, "Label", text, 22f, 32f, FontStyles.Bold, label, TextAlignmentOptions.Center, font);
        Stretch(caption.gameObject, go.transform);
        return button;
    }

    private static TextMeshProUGUI Label(Transform parent, string name, string text, float min, float max, FontStyles style, Color color, TextAlignmentOptions align, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        LayoutElement layout = go.GetComponent<LayoutElement>();
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

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
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
        layout.flexibleWidth = 1f;
    }

    private static void ClearNavigation(Selectable selectable)
    {
        Navigation navigation = selectable.navigation;
        navigation.mode = Navigation.Mode.None;
        selectable.navigation = navigation;
    }
}
