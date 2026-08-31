using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class FinashkaUiBuilder
{
    const string BootPath = "Assets/_Game/_Scenes/Boot.unity";
    const string GamePath = "Assets/_Game/_Scenes/Game.unity";

    static readonly Color Bg = new Color(0.12f, 0.22f, 0.12f, 1f);
    static readonly Color GameBg = new Color(0.12f, 0.22f, 0.12f, 1f);
    static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Gold = new Color(0.86f, 0.58f, 0.08f, 1f);
    static readonly Color Caption = new Color(0.93f, 0.95f, 0.82f, 1f);
    static readonly Color Ink = new Color(0.14f, 0.16f, 0.08f, 1f);
    static readonly Color Outline = new Color(0.07f, 0.12f, 0.05f, 0.92f);
    static readonly Color Work = new Color(0.95f, 0.74f, 0.18f, 1f);
    static readonly Color Snack = new Color(0.7f, 0.32f, 0.14f, 1f);
    static readonly Color Track = new Color(0.08f, 0.14f, 0.07f, 0.45f);
    static readonly Color Fill = new Color(0.86f, 0.58f, 0.08f, 1f);
    static readonly Color Feedback = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Dim = new Color(0.05f, 0.09f, 0.04f, 0.72f);
    static readonly Color Sheet = new Color(1f, 0.97f, 0.88f, 0.97f);
    static readonly Color Need = new Color(0.28f, 0.58f, 0.34f, 1f);
    static readonly Color Want = new Color(0.93f, 0.64f, 0.18f, 1f);

    const string IconsFolder = "Assets/_Game/_Art/Icons";
    const string CoinIconPath = IconsFolder + "/icon-coin.png";
    const string HungerIconPath = IconsFolder + "/icon-hunger.png";
    const string NeedWantCoverPath = IconsFolder + "/cover-need-want.png";
    const string RoomBgPath = "Assets/_Game/_Art/Backgrounds/finashka-jungle-bg.png";

    [MenuItem("Finashka/Rebuild Mobile UI")]
    public static void ApplyAll()
    {
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        EnsureStatIcons();
        BuildBoot();
        BuildGame();
        EditorSceneManager.OpenScene(BootPath);
    }

    static Sprite UiSprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        var inputType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputType != null)
        {
            go.AddComponent(inputType);
        }
        else
        {
            go.AddComponent<StandaloneInputModule>();
        }
    }

    static Canvas SetupCanvas(string name)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        }

        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;
        canvas.additionalShaderChannels =
            AdditionalCanvasShaderChannels.TexCoord1 |
            AdditionalCanvasShaderChannels.Normal |
            AdditionalCanvasShaderChannels.Tangent;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100;
        return canvas;
    }

    static RectTransform Stretch(GameObject go, Transform parent)
    {
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = go.AddComponent<RectTransform>();
        }

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    static Image Sliced(GameObject go, Color color, bool raycast)
    {
        var image = go.GetComponent<Image>();
        if (image == null)
        {
            image = go.AddComponent<Image>();
        }

        image.sprite = UiSprite();
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = raycast;
        return image;
    }

    static LayoutElement PrefHeight(GameObject go, float height)
    {
        var layout = go.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = go.AddComponent<LayoutElement>();
        }

        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleHeight = 0f;
        return layout;
    }

    static GameObject Spacer(Transform parent, float flex)
    {
        var go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var layout = go.GetComponent<LayoutElement>();
        layout.flexibleHeight = flex;
        layout.layoutPriority = 1;
        return go;
    }

    static VerticalLayoutGroup Column(GameObject go, RectOffset padding, float spacing, TextAnchor align)
    {
        var layout = go.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = go.AddComponent<VerticalLayoutGroup>();
        }

        layout.padding = padding;
        layout.spacing = spacing;
        layout.childAlignment = align;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return layout;
    }

    static HorizontalLayoutGroup Row(GameObject go, RectOffset padding, float spacing)
    {
        var layout = go.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = go.AddComponent<HorizontalLayoutGroup>();
        }

        layout.padding = padding;
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        return layout;
    }

    static TextMeshProUGUI Label(Transform parent, string name, string text, float min, float max, FontStyles style, Color color, TextAlignmentOptions align)
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
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        tmp.margin = new Vector4(8f, 2f, 8f, 2f);
        var rt = tmp.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return tmp;
    }

    static void MakeReadable(TextMeshProUGUI tmp, Color fill)
    {
        tmp.color = fill;
        tmp.fontMaterial = tmp.fontSharedMaterial;
        bool darkFill = fill.r + fill.g + fill.b < 1.4f;
        if (darkFill)
        {
            tmp.outlineWidth = 0f;
        }
        else
        {
            tmp.outlineColor = Outline;
            tmp.outlineWidth = 0.18f;
        }
    }

    static Button BigButton(Transform parent, string name, string text, Color color, Color labelColor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var image = Sliced(go, color, true);
        var layout = go.GetComponent<LayoutElement>();
        layout.flexibleWidth = 1f;
        layout.minHeight = 128f;
        layout.preferredHeight = 140f;
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        var nav = button.navigation;
        nav.mode = Navigation.Mode.None;
        button.navigation = nav;
        var label = Label(go.transform, "Label", text, 24f, 44f, FontStyles.Bold, labelColor, TextAlignmentOptions.Center);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;
        var lrt = label.rectTransform;
        lrt.offsetMin = new Vector2(16f, 10f);
        lrt.offsetMax = new Vector2(-16f, -10f);
        return button;
    }

    static Slider ProgressBar(Transform parent, string name, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Slider));
        go.transform.SetParent(parent, false);
        PrefHeight(go, height);
        Sliced(go, Track, false);
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        Stretch(fillArea, go.transform);
        var areaRt = fillArea.GetComponent<RectTransform>();
        areaRt.offsetMin = new Vector2(6f, 5f);
        areaRt.offsetMax = new Vector2(-6f, -5f);
        var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        Stretch(fillGo, fillArea.transform);
        var fillImg = Sliced(fillGo, Fill, false);
        var slider = go.GetComponent<Slider>();
        slider.fillRect = fillGo.GetComponent<RectTransform>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.value = 0f;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.targetGraphic = fillImg;
        return slider;
    }

    public static void BuildBoot()
    {
        EditorSceneManager.OpenScene(BootPath);
        EnsureEventSystem();

        var canvas = SetupCanvas("LoadingCanvas");
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
        }

        Sliced(canvas.gameObject, Bg, false);
        var canvasImage = canvas.GetComponent<Image>();
        SetBackgroundImport(RoomBgPath);
        var room = LoadSprite(RoomBgPath);
        if (canvasImage != null && room != null)
        {
            canvasImage.sprite = room;
            canvasImage.type = Image.Type.Simple;
            canvasImage.preserveAspect = false;
            canvasImage.color = Color.white;
        }

        var cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Bg;
        }

        var safeGo = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
        Stretch(safeGo, canvas.transform);
        Column(safeGo, new RectOffset(72, 72, 64, 80), 24f, TextAnchor.MiddleCenter);

        Spacer(safeGo.transform, 1f);

        var title = Label(safeGo.transform, "Title", AppInfo.Title, 48f, 86f, FontStyles.Bold, Cream, TextAlignmentOptions.Center);
        MakeReadable(title, Cream);
        title.characterSpacing = 4f;
        PrefHeight(title.gameObject, 118f);

        var status = Label(safeGo.transform, "Status", "Запуск…", 24f, 38f, FontStyles.Normal, Caption, TextAlignmentOptions.Center);
        MakeReadable(status, Caption);
        PrefHeight(status.gameObject, 56f);

        var slider = ProgressBar(safeGo.transform, "Progress", 28f);

        Spacer(safeGo.transform, 1f);

        var boot = GameObject.Find("Bootstrap");
        if (boot == null)
        {
            boot = new GameObject("Bootstrap");
        }

        var loading = boot.GetComponent<LoadingView>() ?? boot.AddComponent<LoadingView>();
        var controller = boot.GetComponent<BootController>() ?? boot.AddComponent<BootController>();
        var loadingSo = new SerializedObject(loading);
        loadingSo.FindProperty("_title").objectReferenceValue = title;
        loadingSo.FindProperty("_status").objectReferenceValue = status;
        loadingSo.FindProperty("_progress").objectReferenceValue = slider;
        loadingSo.ApplyModifiedPropertiesWithoutUndo();
        var bootSo = new SerializedObject(controller);
        bootSo.FindProperty("_loading").objectReferenceValue = loading;
        bootSo.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    public static void BuildGame()
    {
        EnsureStatIcons();
        EditorSceneManager.OpenScene(GamePath);
        EnsureEventSystem();

        var canvas = SetupCanvas("HudCanvas");
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
        }

        var cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = GameBg;
        }

        PlaceRoomBackground(cam);

        var safeGo = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
        Stretch(safeGo, canvas.transform);
        Column(safeGo, new RectOffset(40, 40, 28, 32), 12f, TextAnchor.UpperCenter);

        var top = new GameObject("TopBar", typeof(RectTransform));
        top.transform.SetParent(safeGo.transform, false);
        Column(top, new RectOffset(4, 4, 4, 4), 6f, TextAnchor.UpperCenter);
        PrefHeight(top, 280f);

        var title = Label(top.transform, "PetName", AppInfo.Title, 42f, 72f, FontStyles.Bold, Gold, TextAlignmentOptions.Center);
        MakeReadable(title, Gold);
        title.characterSpacing = 6f;
        PrefHeight(title.gameObject, 84f);

        var stats = new GameObject("Stats", typeof(RectTransform));
        stats.transform.SetParent(top.transform, false);
        Row(stats, new RectOffset(0, 0, 0, 0), 20f);
        PrefHeight(stats, 160f);

        var coinsCard = StatCard(stats.transform, "CoinsCard", "Coins", "монеты", "100", LoadSprite(CoinIconPath));
        var hungerCard = StatCard(stats.transform, "HungerCard", "Hunger", "сытость", "80", LoadSprite(HungerIconPath));

        Spacer(safeGo.transform, 1f);

        var feedback = Label(safeGo.transform, "Feedback", "", 22f, 34f, FontStyles.Bold, Feedback, TextAlignmentOptions.Center);
        MakeReadable(feedback, Feedback);
        PrefHeight(feedback.gameObject, 56f);

        var bottom = new GameObject("BottomBar", typeof(RectTransform));
        bottom.transform.SetParent(safeGo.transform, false);
        Row(bottom, new RectOffset(0, 0, 0, 0), 20f);
        PrefHeight(bottom, 148f);

        var work = BigButton(bottom.transform, "WorkButton", "Подработать", Work, Ink);
        var snack = BigButton(bottom.transform, "SnackButton", "Перекус", Snack, Cream);

        var hudGo = GameObject.Find("GameHud");
        if (hudGo == null)
        {
            hudGo = new GameObject("GameHud");
        }

        var hud = hudGo.GetComponent<GameHud>() ?? hudGo.AddComponent<GameHud>();
        var games = hudGo.GetComponent<WorkMinigamesView>() ?? hudGo.AddComponent<WorkMinigamesView>();
        WorkOverlayFactory.Build(canvas.transform, games);
        var so = new SerializedObject(hud);
        so.FindProperty("_petName").objectReferenceValue = title;
        so.FindProperty("_coins").objectReferenceValue = coinsCard;
        so.FindProperty("_hunger").objectReferenceValue = hungerCard;
        so.FindProperty("_feedback").objectReferenceValue = feedback;
        so.FindProperty("_workButton").objectReferenceValue = work;
        so.FindProperty("_snackButton").objectReferenceValue = snack;
        so.FindProperty("_workGames").objectReferenceValue = games;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    static TextMeshProUGUI StatCard(Transform parent, string cardName, string valueName, string caption, string value, Sprite icon)
    {
        var card = new GameObject(cardName, typeof(RectTransform), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
        card.transform.SetParent(parent, false);
        card.GetComponent<LayoutElement>().flexibleWidth = 1f;
        var row = card.GetComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(4, 4, 4, 4);
        row.spacing = 12f;
        row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = true;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconGo.transform.SetParent(card.transform, false);
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.sprite = icon;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
        iconImage.color = Color.white;
        var iconLayout = iconGo.GetComponent<LayoutElement>();
        iconLayout.minWidth = 96f;
        iconLayout.preferredWidth = 108f;
        iconLayout.minHeight = 96f;
        iconLayout.preferredHeight = 108f;
        iconLayout.flexibleWidth = 0f;

        var texts = new GameObject("Texts", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        texts.transform.SetParent(card.transform, false);
        texts.GetComponent<LayoutElement>().flexibleWidth = 1f;
        var vlg = texts.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 0f;
        vlg.childAlignment = TextAnchor.MiddleLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var valueLabel = Label(texts.transform, valueName, value, 56f, 72f, FontStyles.Bold, Ink, TextAlignmentOptions.MidlineLeft);
        MakeReadable(valueLabel, Ink);
        valueLabel.textWrappingMode = TextWrappingModes.NoWrap;
        valueLabel.margin = Vector4.zero;
        PrefHeight(valueLabel.gameObject, 88f);

        var captionLabel = Label(texts.transform, "Caption", caption, 28f, 36f, FontStyles.Bold, Ink, TextAlignmentOptions.MidlineLeft);
        MakeReadable(captionLabel, Ink);
        captionLabel.textWrappingMode = TextWrappingModes.NoWrap;
        captionLabel.margin = Vector4.zero;
        PrefHeight(captionLabel.gameObject, 48f);
        return valueLabel;
    }

    static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void PlaceRoomBackground(Camera cam)
    {
        SetBackgroundImport(RoomBgPath);
        var sprite = LoadSprite(RoomBgPath);
        if (sprite == null)
        {
            return;
        }

        var go = GameObject.Find("RoomBackground");
        if (go == null)
        {
            go = new GameObject("RoomBackground");
        }

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.sortingOrder = -20;
        go.transform.position = new Vector3(0f, 1.15f, 5f);
        go.transform.rotation = Quaternion.identity;

        if (cam == null)
        {
            return;
        }

        float distance = Mathf.Abs(go.transform.position.z - cam.transform.position.z);
        float worldHeight = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float worldWidth = worldHeight * (9f / 16f);
        Vector2 size = sprite.bounds.size;
        if (size.x < 0.01f || size.y < 0.01f)
        {
            return;
        }

        float scale = Mathf.Max(worldWidth / size.x, worldHeight / size.y) * 1.04f;
        go.transform.localScale = new Vector3(scale, scale, 1f);
    }

    static void SetBackgroundImport(string path)
    {
        AssetDatabase.ImportAsset(path);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.alphaIsTransparency = false;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    static void EnsureStatIcons()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Game/_Art"))
        {
            AssetDatabase.CreateFolder("Assets/_Game", "_Art");
        }

        if (!AssetDatabase.IsValidFolder(IconsFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Game/_Art", "Icons");
        }

        AssetDatabase.ImportAsset(CoinIconPath);
        AssetDatabase.ImportAsset(HungerIconPath);
        AssetDatabase.ImportAsset(NeedWantCoverPath);
        SetSpriteImport(CoinIconPath);
        SetSpriteImport(HungerIconPath);
        SetSpriteImport(NeedWantCoverPath);
    }

    static void SetSpriteImport(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    static GameObject StretchPage(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Stretch(go, parent);
        return go;
    }

    static Button SlimButton(Transform parent, string name, string text, Color color, Color labelColor, float height)
    {
        var button = BigButton(parent, name, text, color, labelColor);
        var layout = button.GetComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleHeight = 0f;
        return button;
    }

    static Button CoverGameCard(Transform parent, string name, Sprite cover, string title, string subtitle)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(VerticalLayoutGroup));
        go.transform.SetParent(parent, false);
        var image = Sliced(go, Cream, true);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 420f;
        layout.preferredHeight = 460f;
        layout.flexibleHeight = 0f;
        var column = go.GetComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(20, 20, 20, 20);
        column.spacing = 10f;
        column.childAlignment = TextAnchor.UpperCenter;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        button.colors = colors;

        var coverGo = new GameObject("Cover", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        coverGo.transform.SetParent(go.transform, false);
        var coverImage = coverGo.GetComponent<Image>();
        coverImage.sprite = cover;
        coverImage.preserveAspect = true;
        coverImage.raycastTarget = false;
        coverImage.color = Color.white;
        var coverLayout = coverGo.GetComponent<LayoutElement>();
        coverLayout.minHeight = 220f;
        coverLayout.preferredHeight = 240f;

        var titleLabel = Label(go.transform, "Title", title, 28f, 40f, FontStyles.Bold, Ink, TextAlignmentOptions.Center);
        MakeReadable(titleLabel, Ink);
        PrefHeight(titleLabel.gameObject, 48f);

        var subLabel = Label(go.transform, "Subtitle", subtitle, 20f, 28f, FontStyles.Normal, Ink, TextAlignmentOptions.Center);
        MakeReadable(subLabel, Ink);
        PrefHeight(subLabel.gameObject, 72f);
        return button;
    }

    static GameObject BuildWorkOverlay(Transform canvas)
    {
        var overlay = new GameObject("WorkOverlay", typeof(RectTransform));
        Stretch(overlay, canvas);
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
        Sliced(sheet, Sheet, true);
        var sheetLayout = sheet.GetComponent<LayoutElement>();
        sheetLayout.flexibleHeight = 1f;
        sheetLayout.flexibleWidth = 1f;
        Column(sheet, new RectOffset(28, 28, 28, 24), 12f, TextAnchor.UpperCenter);

        var header = Label(sheet.transform, "WorkTitle", "Подработка", 34f, 52f, FontStyles.Bold, Gold, TextAlignmentOptions.Center);
        MakeReadable(header, Gold);
        header.characterSpacing = 2f;
        PrefHeight(header.gameObject, 60f);

        var pages = new GameObject("Pages", typeof(RectTransform), typeof(LayoutElement));
        pages.transform.SetParent(sheet.transform, false);
        pages.GetComponent<LayoutElement>().flexibleHeight = 1f;

        var menu = StretchPage(pages.transform, "Menu");
        Column(menu, new RectOffset(0, 0, 0, 0), 14f, TextAnchor.UpperCenter);
        var intro = Label(menu.transform, "MenuIntro", "Выбери мини-игру и заработай монеты. Финашка учится: что нужно купить, а что просто хочется.", 20f, 30f, FontStyles.Normal, Ink, TextAlignmentOptions.Center);
        MakeReadable(intro, Ink);
        PrefHeight(intro.gameObject, 96f);
        CoverGameCard(
            menu.transform,
            "NeedWantCard",
            LoadSprite(NeedWantCoverPath),
            "Нужно или хочу?",
            "5 вопросов · до +15 монет");
        var soon = Label(menu.transform, "MenuSoon", "Скоро появятся новые игры", 18f, 26f, FontStyles.Italic, Ink, TextAlignmentOptions.Center);
        MakeReadable(soon, Ink);
        PrefHeight(soon.gameObject, 40f);
        Spacer(menu.transform, 1f);
        SlimButton(menu.transform, "CloseMenuButton", "Закрыть", Snack, Cream, 96f);

        var play = StretchPage(pages.transform, "Play");
        Column(play, new RectOffset(0, 0, 0, 0), 10f, TextAnchor.UpperCenter);
        var progress = Label(play.transform, "PlayProgress", "1 / 5", 22f, 32f, FontStyles.Bold, Gold, TextAlignmentOptions.Center);
        MakeReadable(progress, Gold);
        PrefHeight(progress.gameObject, 40f);

        var itemCard = new GameObject("ItemCard", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(VerticalLayoutGroup));
        itemCard.transform.SetParent(play.transform, false);
        Sliced(itemCard, Cream, false);
        var itemLayout = itemCard.GetComponent<LayoutElement>();
        itemLayout.minHeight = 260f;
        itemLayout.preferredHeight = 300f;
        itemLayout.flexibleHeight = 1f;
        var itemCol = itemCard.GetComponent<VerticalLayoutGroup>();
        itemCol.padding = new RectOffset(20, 20, 24, 20);
        itemCol.spacing = 12f;
        itemCol.childAlignment = TextAnchor.MiddleCenter;
        itemCol.childControlWidth = true;
        itemCol.childControlHeight = true;
        itemCol.childForceExpandWidth = true;
        itemCol.childForceExpandHeight = false;
        var itemTitle = Label(itemCard.transform, "ItemTitle", "Хлеб", 34f, 52f, FontStyles.Bold, Ink, TextAlignmentOptions.Center);
        MakeReadable(itemTitle, Ink);
        PrefHeight(itemTitle.gameObject, 72f);
        var itemPrompt = Label(itemCard.transform, "ItemPrompt", "В магазине лежит свежий хлеб.", 22f, 34f, FontStyles.Normal, Ink, TextAlignmentOptions.Center);
        MakeReadable(itemPrompt, Ink);
        PrefHeight(itemPrompt.gameObject, 120f);

        var playFeedback = Label(play.transform, "PlayFeedback", "Это нужно купить или просто хочется?", 20f, 30f, FontStyles.Bold, Ink, TextAlignmentOptions.Center);
        MakeReadable(playFeedback, Ink);
        PrefHeight(playFeedback.gameObject, 88f);
        SlimButton(play.transform, "NeedButton", "Нужно", Need, Cream, 112f);
        SlimButton(play.transform, "WantButton", "Хочу", Want, Ink, 112f);
        SlimButton(play.transform, "ClosePlayButton", "В меню", Track, Cream, 80f);

        var result = StretchPage(pages.transform, "Result");
        Column(result, new RectOffset(0, 0, 8, 0), 14f, TextAnchor.MiddleCenter);
        Spacer(result.transform, 1f);
        var resultTitle = Label(result.transform, "ResultTitle", "Супер!", 36f, 58f, FontStyles.Bold, Gold, TextAlignmentOptions.Center);
        MakeReadable(resultTitle, Gold);
        PrefHeight(resultTitle.gameObject, 72f);
        var resultScore = Label(result.transform, "ResultScore", "Верно 5 из 5", 22f, 32f, FontStyles.Normal, Ink, TextAlignmentOptions.Center);
        MakeReadable(resultScore, Ink);
        PrefHeight(resultScore.gameObject, 160f);
        var resultCoins = Label(result.transform, "ResultCoins", "+15 монет", 32f, 48f, FontStyles.Bold, Gold, TextAlignmentOptions.Center);
        MakeReadable(resultCoins, Gold);
        PrefHeight(resultCoins.gameObject, 64f);
        Spacer(result.transform, 1f);
        SlimButton(result.transform, "CollectButton", "Забрать монеты", Work, Ink, 120f);

        play.SetActive(false);
        result.SetActive(false);
        return overlay;
    }

    static void WireWorkGames(WorkMinigamesView view, GameObject overlay)
    {
        Transform pages = overlay.transform.Find("SheetSafe/Sheet/Pages");
        var so = new SerializedObject(view);
        so.FindProperty("_root").objectReferenceValue = overlay;
        so.FindProperty("_menu").objectReferenceValue = pages.Find("Menu").gameObject;
        so.FindProperty("_play").objectReferenceValue = pages.Find("Play").gameObject;
        so.FindProperty("_result").objectReferenceValue = pages.Find("Result").gameObject;
        so.FindProperty("_dimButton").objectReferenceValue = overlay.transform.Find("Dim").GetComponent<Button>();
        so.FindProperty("_closeMenuButton").objectReferenceValue = pages.Find("Menu/CloseMenuButton").GetComponent<Button>();
        so.FindProperty("_closePlayButton").objectReferenceValue = pages.Find("Play/ClosePlayButton").GetComponent<Button>();
        so.FindProperty("_openNeedWantButton").objectReferenceValue = pages.Find("Menu/NeedWantCard").GetComponent<Button>();
        so.FindProperty("_needButton").objectReferenceValue = pages.Find("Play/NeedButton").GetComponent<Button>();
        so.FindProperty("_wantButton").objectReferenceValue = pages.Find("Play/WantButton").GetComponent<Button>();
        so.FindProperty("_collectButton").objectReferenceValue = pages.Find("Result/CollectButton").GetComponent<Button>();
        so.FindProperty("_progress").objectReferenceValue = pages.Find("Play/PlayProgress").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_itemTitle").objectReferenceValue = pages.Find("Play/ItemCard/ItemTitle").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_itemPrompt").objectReferenceValue = pages.Find("Play/ItemCard/ItemPrompt").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_playFeedback").objectReferenceValue = pages.Find("Play/PlayFeedback").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_resultTitle").objectReferenceValue = pages.Find("Result/ResultTitle").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_resultScore").objectReferenceValue = pages.Find("Result/ResultScore").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_resultCoins").objectReferenceValue = pages.Find("Result/ResultCoins").GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}

[InitializeOnLoad]
static class FinashkaApplyLargeStats
{
    const string Key = "Finashka.WorkMinigame.v1";

    static FinashkaApplyLargeStats()
    {
        EditorApplication.delayCall += Run;
    }

    static void Run()
    {
        if (SessionState.GetBool(Key, false) || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        SessionState.SetBool(Key, true);
        try
        {
            FinashkaUiBuilder.BuildGame();
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Game/_Scenes/Boot.unity");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Finashka HUD rebuild: " + ex.Message);
        }
    }
}
