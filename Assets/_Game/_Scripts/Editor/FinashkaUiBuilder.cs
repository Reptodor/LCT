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

    const string IconsFolder = "Assets/_Game/_Art/Icons";
    const string CoinIconPath = IconsFolder + "/icon-coin.png";
    const string HungerIconPath = IconsFolder + "/icon-hunger.png";
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
        tmp.overflowMode = TextOverflowModes.Ellipsis;
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
        label.overflowMode = TextOverflowModes.Ellipsis;
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
        PrefHeight(top, 228f);

        var title = Label(top.transform, "PetName", AppInfo.Title, 42f, 72f, FontStyles.Bold, Gold, TextAlignmentOptions.Center);
        MakeReadable(title, Gold);
        title.characterSpacing = 6f;
        PrefHeight(title.gameObject, 84f);

        var stats = new GameObject("Stats", typeof(RectTransform));
        stats.transform.SetParent(top.transform, false);
        Row(stats, new RectOffset(8, 8, 0, 0), 28f);
        PrefHeight(stats, 108f);

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
        var so = new SerializedObject(hud);
        so.FindProperty("_petName").objectReferenceValue = title;
        so.FindProperty("_coins").objectReferenceValue = coinsCard;
        so.FindProperty("_hunger").objectReferenceValue = hungerCard;
        so.FindProperty("_feedback").objectReferenceValue = feedback;
        so.FindProperty("_workButton").objectReferenceValue = work;
        so.FindProperty("_snackButton").objectReferenceValue = snack;
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
        iconLayout.minWidth = 52f;
        iconLayout.preferredWidth = 56f;
        iconLayout.minHeight = 52f;
        iconLayout.preferredHeight = 56f;
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

        var valueLabel = Label(texts.transform, valueName, value, 28f, 44f, FontStyles.Bold, Ink, TextAlignmentOptions.MidlineLeft);
        MakeReadable(valueLabel, Ink);
        valueLabel.textWrappingMode = TextWrappingModes.NoWrap;
        valueLabel.margin = Vector4.zero;
        PrefHeight(valueLabel.gameObject, 48f);

        var captionLabel = Label(texts.transform, "Caption", caption, 16f, 22f, FontStyles.Bold, Ink, TextAlignmentOptions.MidlineLeft);
        MakeReadable(captionLabel, Ink);
        captionLabel.textWrappingMode = TextWrappingModes.NoWrap;
        captionLabel.margin = Vector4.zero;
        PrefHeight(captionLabel.gameObject, 28f);
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

        WriteIconPng(CoinIconPath, PaintCoin);
        WriteIconPng(HungerIconPath, PaintHunger);
        AssetDatabase.Refresh();
        SetSpriteImport(CoinIconPath);
        SetSpriteImport(HungerIconPath);
        AssetDatabase.ImportAsset(CoinIconPath);
        AssetDatabase.ImportAsset(HungerIconPath);
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

    static void WriteIconPng(string path, System.Action<Color32[], int> paint)
    {
        const int size = 128;
        var pixels = new Color32[size * size];
        paint(pixels, size);
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixels);
        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }

    static void PaintCoin(Color32[] pixels, int size)
    {
        var gold = (Color32)Gold;
        var dark = (Color32)new Color(0.55f, 0.34f, 0.06f, 1f);
        var light = (Color32)new Color(0.98f, 0.86f, 0.45f, 1f);
        float cx = size * 0.5f;
        float cy = size * 0.5f;
        float outer = size * 0.44f;
        float ringOuter = size * 0.36f;
        float ringInner = size * 0.3f;
        int i;
        for (i = 0; i < pixels.Length; i++)
        {
            int x = i % size;
            int y = i / size;
            float dx = x + 0.5f - cx;
            float dy = y + 0.5f - cy;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Clamp01(outer + 1.2f - d);
            if (a <= 0.001f)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
                continue;
            }

            Color32 c = gold;
            if (d < ringOuter && d > ringInner)
            {
                c = dark;
            }
            else if (dx < -size * 0.08f && dy > size * 0.08f && d < ringInner)
            {
                c = light;
            }

            c.a = (byte)Mathf.RoundToInt(a * 255f);
            pixels[i] = c;
        }
    }

    static void PaintHunger(Color32[] pixels, int size)
    {
        var gold = (Color32)Gold;
        var dark = (Color32)new Color(0.55f, 0.34f, 0.06f, 1f);
        var leaf = (Color32)Gold;
        float cx = size * 0.5f;
        float cy = size * 0.44f;
        float body = size * 0.32f;
        int i;
        for (i = 0; i < pixels.Length; i++)
        {
            int x = i % size;
            int y = i / size;
            float px = x + 0.5f;
            float py = y + 0.5f;
            float dx = px - cx;
            float dy = py - cy;
            float dBody = Mathf.Sqrt(dx * dx + dy * dy);
            float bite = Mathf.Sqrt((px - cx) * (px - cx) + (py - (cy + body * 0.85f)) * (py - (cy + body * 0.85f)));
            float stem = 0f;
            if (Mathf.Abs(px - cx) < size * 0.035f && py > cy + body * 0.55f && py < cy + body * 1.35f)
            {
                stem = 1f;
            }

            float leafDx = (px - (cx + size * 0.12f)) / (size * 0.14f);
            float leafDy = (py - (cy + body * 1.05f)) / (size * 0.08f);
            float leafD = leafDx * leafDx + leafDy * leafDy;
            float a = 0f;
            Color32 c = gold;
            if (dBody < body + 1.2f && bite > size * 0.12f)
            {
                a = Mathf.Clamp01(body + 1.2f - dBody);
                if (dx < -size * 0.06f && dy > size * 0.04f)
                {
                    c = (Color32)new Color(0.98f, 0.86f, 0.45f, 1f);
                }
            }

            if (stem > 0f)
            {
                a = 1f;
                c = dark;
            }

            if (leafD < 1f)
            {
                a = Mathf.Max(a, Mathf.Clamp01(1.2f - leafD));
                c = leaf;
            }

            if (a <= 0.001f)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
            }
            else
            {
                c.a = (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f);
                pixels[i] = c;
            }
        }
    }
}
