using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PetCustomizeView : MonoBehaviour
{
    static readonly Color Backdrop = new Color(0.46f, 0.66f, 0.50f, 1f);
    static readonly Color Sheet = new Color(1f, 0.97f, 0.88f, 0.98f);
    static readonly Color Ink = new Color(0.14f, 0.16f, 0.08f, 1f);
    static readonly Color Gold = new Color(0.86f, 0.58f, 0.08f, 1f);
    static readonly Color Card = new Color(0.22f, 0.46f, 0.30f, 1f);
    static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Field = new Color(1f, 1f, 0.96f, 1f);

    const float PanelShare = 0.40f;
    const float NameShare = 0.50f;

    [Header("Настройка вида")]
    [Tooltip("Насколько далеко камера. 1 — кот вплотную к краям кадра, больше — дальше, чтобы было видно всё тело.")]
    [SerializeField] float _cameraDistance = 1.75f;
    [Tooltip("Сдвиг кота вверх. 0 — лапы на полу, больше — выше.")]
    [SerializeField] float _catY = 0.35f;

    GameObject _cat;
    TMP_InputField _name;
    TMP_Text _status;
    Image[] _colorRings;
    Image[] _hatFills;
    int _color;
    int _hat = PetLook.NoHat;
    bool _dragging;
    TMP_Text[] _hatLabels;
    float _appliedDistance = -1f;
    float _appliedCatY = float.NaN;

    void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        if (!GameSession.IsReady)
        {
            GameSession.Initialize(SaveService.CreateDefault());
        }

        PetLook.Read(GameSession.State, out _color, out _hat);
        EnsureEventSystem();
        BuildUi();
        PrepareCamera();
        StartCoroutine(ShowCat());
    }

    void Update()
    {
        if (_cat != null && !_dragging)
        {
            TurnPreview(18f * Time.deltaTime);
        }

        ApplyTuning();
    }

    void OnValidate()
    {
        _cameraDistance = Mathf.Max(0.8f, _cameraDistance);
        if (Application.isPlaying && _cat != null)
        {
            _appliedDistance = -1f;
            _appliedCatY = float.NaN;
            ApplyTuning();
        }
    }

    IEnumerator ShowCat()
    {
        PetLookCatalog catalog = PetLookCatalog.Load();
        if (catalog == null || catalog.Cat == null)
        {
            Debug.LogError("[Finashka] Нет каталога внешности питомца.");
            yield break;
        }

        _cat = Instantiate(catalog.Cat);
        _cat.name = "PreviewCat";
        _cat.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _cat.transform.localScale = Vector3.one;
        PetLook.KeepPrefabPose(_cat);
        yield return null;
        PetLook.Apply(_cat, _color, _hat);
        PlaceCat();
        _appliedCatY = _catY;
        BuildPodium();
        FrameCamera();
    }

    void SelectColor(int index)
    {
        _color = PetLook.ClampColor(index);
        RefreshSelection();
        if (_cat != null)
        {
            PetLook.ApplyColor(_cat, _color);
        }
    }

    void SelectHat(int index)
    {
        _hat = PetLook.ClampHat(index);
        RefreshSelection();
        if (_cat != null)
        {
            PetLook.ApplyHat(_cat, _hat);
        }
    }

    void Confirm()
    {
        string name = _name != null ? _name.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(name))
        {
            if (_status != null)
            {
                _status.text = "Введи имя питомца";
            }

            return;
        }

        if (!GameSession.IsReady)
        {
            GameSession.Initialize(SaveService.CreateDefault());
        }

        GameSession.State.petName = name;
        GameSession.State.petColor = _color;
        GameSession.State.petHat = _hat;
        GameSession.State.petLookSet = true;
        GameSession.Persist();
        SceneManager.LoadScene(BootController.GameSceneName);
    }

    void RefreshSelection()
    {
        if (_colorRings != null)
        {
            for (int i = 0; i < _colorRings.Length; i++)
            {
                _colorRings[i].color = i == _color ? Gold : new Color(1f, 0.97f, 0.88f, 0.35f);
            }
        }

        if (_hatFills != null)
        {
            for (int i = 0; i < _hatFills.Length; i++)
            {
                bool selected = i == _hat;
                _hatFills[i].color = selected ? Gold : Card;
                if (_hatLabels != null && i < _hatLabels.Length && _hatLabels[i] != null)
                {
                    _hatLabels[i].color = selected ? Ink : Cream;
                }
            }
        }
    }

    void PrepareCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Backdrop;
        cam.orthographic = false;
        cam.fieldOfView = 28f;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 40f;
        cam.rect = new Rect(0f, NameShare - 0.02f, 1f, 1f - (NameShare - 0.02f));
        cam.transform.SetPositionAndRotation(new Vector3(0f, 0.7f, -2.4f), Quaternion.Euler(8f, 0f, 0f));
    }

    void ApplyTuning()
    {
        bool distanceChanged = !Mathf.Approximately(_appliedDistance, _cameraDistance);
        bool heightChanged = !Mathf.Approximately(_appliedCatY, _catY);
        if (!distanceChanged && !heightChanged)
        {
            return;
        }

        _appliedDistance = _cameraDistance;
        _appliedCatY = _catY;
        if (_cat != null)
        {
            PlaceCat();
            FrameCamera();
        }
    }

    void PlaceCat()
    {
        if (_cat == null)
        {
            return;
        }

        PetLook.SeatOnFloor(_cat, 0f);
        _cat.transform.position += Vector3.up * _catY;
    }

    void FrameCamera()
    {
        Camera cam = Camera.main;
        if (cam == null || _cat == null || !PetLook.TryBodyBounds(_cat, out Bounds bounds))
        {
            return;
        }

        Vector3 face = PetLook.FaceDirection(_cat);
        Vector3 look = bounds.center;
        look.y -= _catY;
        float vFov = cam.fieldOfView * Mathf.Deg2Rad;
        float hFov = 2f * Mathf.Atan(Mathf.Tan(vFov * 0.5f) * Mathf.Max(0.1f, cam.aspect));
        float distY = bounds.extents.y / Mathf.Tan(vFov * 0.5f);
        float distX = Mathf.Max(bounds.extents.x, bounds.extents.z) / Mathf.Tan(hFov * 0.5f);
        float dist = Mathf.Max(distY, distX) * Mathf.Max(0.8f, _cameraDistance);
        cam.transform.position = look + face * dist;
        cam.transform.LookAt(look);
        _appliedDistance = _cameraDistance;
    }

    void BuildPodium()
    {
        if (_cat == null || !PetLook.TryBodyBounds(_cat, out Bounds bounds))
        {
            return;
        }

        var podium = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        podium.name = "PreviewPodium";
        Collider collider = podium.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.35f;
        podium.transform.SetPositionAndRotation(new Vector3(bounds.center.x, 0.01f, bounds.center.z), Quaternion.identity);
        podium.transform.localScale = new Vector3(radius * 2f, 0.025f, radius * 2f);
        Renderer renderer = podium.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader != null)
            {
                var mat = new Material(shader);
                Color color = new Color(0.28f, 0.46f, 0.32f, 1f);
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", color);
                }

                mat.color = color;
                renderer.sharedMaterial = mat;
            }
        }
    }

    void BuildUi()
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null)
        {
            font = TMP_Settings.defaultFontAsset;
        }

        Sprite sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");

        var canvasGo = new GameObject("CustomizeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvas.additionalShaderChannels =
            AdditionalCanvasShaderChannels.TexCoord1 |
            AdditionalCanvasShaderChannels.Normal |
            AdditionalCanvasShaderChannels.Tangent;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        var backdrop = new GameObject("SheetBackdrop", typeof(RectTransform), typeof(Image));
        StretchBottom(backdrop, canvasGo.transform, NameShare);
        Paint(backdrop, Sheet, false, sprite);

        var safe = new GameObject("Safe", typeof(RectTransform), typeof(SafeAreaFitter));
        Stretch(safe, canvasGo.transform);

        var dragGo = new GameObject("PreviewDrag", typeof(RectTransform), typeof(Image), typeof(PetPreviewDrag));
        dragGo.transform.SetParent(safe.transform, false);
        var dragRect = dragGo.GetComponent<RectTransform>();
        dragRect.anchorMin = new Vector2(0f, NameShare);
        dragRect.anchorMax = Vector2.one;
        dragRect.offsetMin = Vector2.zero;
        dragRect.offsetMax = Vector2.zero;
        var dragImage = dragGo.GetComponent<Image>();
        dragImage.color = new Color(0f, 0f, 0f, 0f);
        dragImage.raycastTarget = true;
        var drag = dragGo.GetComponent<PetPreviewDrag>();
        drag.Owner = this;

        var nameBand = new GameObject("NameBand", typeof(RectTransform), typeof(VerticalLayoutGroup));
        nameBand.transform.SetParent(safe.transform, false);
        var nameRect = nameBand.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, PanelShare);
        nameRect.anchorMax = new Vector2(1f, NameShare);
        nameRect.offsetMin = new Vector2(36f, 8f);
        nameRect.offsetMax = new Vector2(-36f, -8f);
        var nameColumn = nameBand.GetComponent<VerticalLayoutGroup>();
        nameColumn.padding = new RectOffset(8, 8, 0, 0);
        nameColumn.spacing = 4f;
        nameColumn.childAlignment = TextAnchor.MiddleCenter;
        nameColumn.childControlWidth = true;
        nameColumn.childControlHeight = true;
        nameColumn.childForceExpandWidth = true;
        nameColumn.childForceExpandHeight = false;
        string savedName = GameSession.IsReady ? GameSession.State.petName : AppInfo.Title;
        _name = NameField(nameBand.transform, font, sprite, savedName);

        var sheet = new GameObject("Sheet", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        sheet.transform.SetParent(safe.transform, false);
        var sheetRect = sheet.GetComponent<RectTransform>();
        sheetRect.anchorMin = new Vector2(0f, 0f);
        sheetRect.anchorMax = new Vector2(1f, PanelShare);
        sheetRect.offsetMin = new Vector2(36f, 28f);
        sheetRect.offsetMax = new Vector2(-36f, -8f);
        var column = sheet.GetComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(8, 8, 8, 8);
        column.spacing = 14f;
        column.childAlignment = TextAnchor.UpperCenter;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;

        Label(sheet.transform, "ColorCaption", "Окрас", 22f, 32f, FontStyles.Bold, Ink, font);
        BuildColors(sheet.transform, knob);
        Label(sheet.transform, "HatCaption", "Головной убор", 22f, 32f, FontStyles.Bold, Ink, font);
        BuildHats(sheet.transform, sprite, font);
        Button confirm = WideButton(sheet.transform, "Confirm", "Подтвердить", Gold, Ink, sprite, font);
        confirm.onClick.AddListener(Confirm);
        _status = Label(sheet.transform, "Status", "", 20f, 28f, FontStyles.Normal, new Color(0.55f, 0.22f, 0.16f, 1f), font);
        RefreshSelection();
    }

    void BuildColors(Transform parent, Sprite knob)
    {
        var row = Row(parent, "Colors", 128f);
        _colorRings = new Image[PetLook.ColorCount];
        for (int i = 0; i < PetLook.ColorCount; i++)
        {
            int index = i;
            var ring = new GameObject("Color" + i, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            ring.transform.SetParent(row.transform, false);
            var layout = ring.GetComponent<LayoutElement>();
            layout.preferredWidth = 128f;
            layout.preferredHeight = 128f;
            layout.flexibleWidth = 1f;
            var ringImage = ring.GetComponent<Image>();
            ringImage.sprite = knob;
            ringImage.color = Gold;
            var button = ring.GetComponent<Button>();
            button.targetGraphic = ringImage;
            button.onClick.AddListener(() => SelectColor(index));

            var swatch = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            swatch.transform.SetParent(ring.transform, false);
            var swatchRect = swatch.GetComponent<RectTransform>();
            swatchRect.anchorMin = new Vector2(0.14f, 0.14f);
            swatchRect.anchorMax = new Vector2(0.86f, 0.86f);
            swatchRect.offsetMin = Vector2.zero;
            swatchRect.offsetMax = Vector2.zero;
            var swatchImage = swatch.GetComponent<Image>();
            swatchImage.sprite = knob;
            swatchImage.color = PetLook.Colors[i];
            swatchImage.raycastTarget = false;
            _colorRings[i] = ringImage;
        }
    }

    void BuildHats(Transform parent, Sprite sprite, TMP_FontAsset font)
    {
        var row = Row(parent, "Hats", 120f);
        _hatFills = new Image[PetLook.HatCount];
        _hatLabels = new TMP_Text[PetLook.HatCount];
        for (int i = 0; i < PetLook.HatCount; i++)
        {
            int index = i;
            var go = new GameObject("Hat" + i, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(row.transform, false);
            var layout = go.GetComponent<LayoutElement>();
            layout.preferredHeight = 120f;
            layout.flexibleWidth = 1f;
            layout.minWidth = 0f;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Card;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => SelectHat(index));
            var label = Label(go.transform, "Label", PetLook.HatNames[i], 18f, 30f, FontStyles.Bold, Cream, font);
            var rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 8f);
            rect.offsetMax = new Vector2(-8f, -8f);
            _hatFills[i] = image;
            _hatLabels[i] = label;
        }
    }

    public void SetDragging(bool dragging)
    {
        _dragging = dragging;
    }

    public void TurnPreview(float degrees)
    {
        if (_cat == null)
        {
            return;
        }

        Vector3 pivot = _cat.transform.position;
        if (PetLook.TryBodyBounds(_cat, out Bounds bounds))
        {
            pivot = new Vector3(bounds.center.x, _cat.transform.position.y, bounds.center.z);
        }

        _cat.transform.RotateAround(pivot, Vector3.up, degrees);
    }

    static GameObject Row(Transform parent, string name, float height)
    {
        var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        var layout = row.GetComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleWidth = 1f;
        var horizontal = row.GetComponent<HorizontalLayoutGroup>();
        horizontal.spacing = 16f;
        horizontal.childAlignment = TextAnchor.MiddleCenter;
        horizontal.childControlWidth = true;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandWidth = true;
        horizontal.childForceExpandHeight = true;
        return row;
    }

    static TMP_InputField NameField(Transform parent, TMP_FontAsset font, Sprite sprite, string value)
    {
        var root = new GameObject("NameField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
        root.transform.SetParent(parent, false);
        var layout = root.GetComponent<LayoutElement>();
        layout.minHeight = 100f;
        layout.preferredHeight = 100f;
        layout.flexibleWidth = 1f;
        var image = root.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = Field;

        var area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        area.transform.SetParent(root.transform, false);
        var areaRect = area.GetComponent<RectTransform>();
        areaRect.anchorMin = Vector2.zero;
        areaRect.anchorMax = Vector2.one;
        areaRect.offsetMin = new Vector2(28f, 12f);
        areaRect.offsetMax = new Vector2(-28f, -12f);

        var text = InputText(area.transform, font, "", Ink, FontStyles.Normal);
        var placeholder = InputText(area.transform, font, "Как зовут питомца?", new Color(Ink.r, Ink.g, Ink.b, 0.4f), FontStyles.Italic);

        var input = root.GetComponent<TMP_InputField>();
        input.textViewport = areaRect;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.fontAsset = font;
        input.pointSize = 40f;
        input.characterLimit = 16;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.contentType = TMP_InputField.ContentType.Standard;
        input.text = string.IsNullOrWhiteSpace(value) ? AppInfo.Title : value.Trim();
        input.targetGraphic = image;
        return input;
    }

    static TextMeshProUGUI InputText(Transform parent, TMP_FontAsset font, string text, Color color, FontStyles style)
    {
        var go = new GameObject(text.Length == 0 ? "Text" : "Placeholder", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = font;
        tmp.fontSize = 40f;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return tmp;
    }

    static TMP_Text Label(Transform parent, string name, string text, float min, float max, FontStyles style, Color color, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = max + 8f;
        layout.preferredHeight = max + 12f;
        layout.flexibleWidth = 1f;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = font;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = min;
        tmp.fontSizeMax = max;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button WideButton(Transform parent, string name, string text, Color fill, Color labelColor, Sprite sprite, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 108f;
        layout.preferredHeight = 108f;
        layout.flexibleWidth = 1f;
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = fill;
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        var label = Label(go.transform, "Label", text, 24f, 40f, FontStyles.Bold, labelColor, font);
        var rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var labelLayout = label.GetComponent<LayoutElement>();
        if (labelLayout != null)
        {
            labelLayout.ignoreLayout = true;
        }

        return button;
    }

    static void Paint(GameObject go, Color color, bool raycast, Sprite sprite)
    {
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = raycast;
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

    static void StretchBottom(GameObject go, Transform parent, float height)
    {
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1f, height);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        System.Type inputType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputType != null)
        {
            go.AddComponent(inputType);
        }
        else
        {
            go.AddComponent<StandaloneInputModule>();
        }
    }
}

public class PetPreviewDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public PetCustomizeView Owner;

    public void OnPointerDown(PointerEventData eventData)
    {
        Owner?.SetDragging(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Owner?.TurnPreview(-eventData.delta.x * 0.45f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Owner?.SetDragging(false);
    }
}
