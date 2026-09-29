using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TabletopArDirector : MonoBehaviour
{
    const float StableSeconds = 0.7f;
    const float SurfaceLift = 0.008f;

    [SerializeField] GameObject _rig;

    ARPlaneManager _planes;
    ARPlane _plane;
    GameObject _house;
    TabletopPet _pet;
    TMP_Text _prompt;
    RectTransform _backRect;
    float _footprint = 1f;
    float _scale = 0.08f;
    Vector3 _pivotToFloor;
    Vector3 _facing = Vector3.forward;
    bool _facingLocked;
    bool _placed;
    bool _leaving;
    TrackableId _candidate;
    float _stable;
    readonly List<TabletopPet.Patch> _patches = new List<TabletopPet.Patch>();

    void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        EnsureSession();
        EnsureRig();
        EnsureLight();
    }

    void Start()
    {
        MuteScreenSpaceInteractor();
        _planes = FindFirstObjectByType<ARPlaneManager>();
        if (_planes != null)
        {
            _planes.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.nearClipPlane = 0.03f;
        }

        BuildHud();
        PrepareHouse();
    }

    void Update()
    {
        if (WantLeave())
        {
            Leave();
            return;
        }

        if (_placed)
        {
            return;
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            SetPrompt("Камера AR здесь недоступна");
            return;
        }

        if (_house == null)
        {
            SetPrompt("Не нашёл комнату");
            return;
        }

        if (_planes == null)
        {
            SetPrompt("Не удалось запустить камеру");
            return;
        }

        SetPrompt("Наведи камеру на стол");
        ARPlane best = BestPlane();
        if (best == null)
        {
            _candidate = TrackableId.invalidId;
            _stable = 0f;
            return;
        }

        if (best.trackableId != _candidate)
        {
            _candidate = best.trackableId;
            _stable = 0f;
            return;
        }

        _stable += Time.deltaTime;
        if (_stable >= StableSeconds)
        {
            Place(best);
        }
    }

    void LateUpdate()
    {
        if (!_placed || _house == null)
        {
            return;
        }

        if (_plane != null)
        {
            ApplyPose(_plane);
        }

        HidePlaneMeshes();
        if (_pet != null)
        {
            _pet.Tick(_house.transform);
        }
    }

    void EnsureSession()
    {
        if (FindFirstObjectByType<ARSession>() != null)
        {
            return;
        }

        var session = new GameObject("AR Session");
        session.AddComponent<ARSession>();
        session.AddComponent<ARInputManager>();
    }

    void EnsureRig()
    {
        if (FindFirstObjectByType<ARPlaneManager>() != null)
        {
            return;
        }

        if (_rig == null)
        {
            Debug.LogError("[Finashka] В сцене PetAR нет AR-рига.");
            return;
        }

        Instantiate(_rig);
    }

    void EnsureLight()
    {
        if (FindFirstObjectByType<Light>() != null)
        {
            return;
        }

        var sun = new GameObject("Tabletop Light");
        var light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.shadows = LightShadows.Soft;
        light.color = new Color(1f, 0.96f, 0.90f, 1f);
        light.intensity = 1.15f;
        sun.transform.rotation = Quaternion.Euler(68f, -24f, 0f);
    }

    void PrepareHouse()
    {
        var prefab = Resources.Load<GameObject>("House");
        if (prefab == null)
        {
            return;
        }

        _house = Instantiate(prefab);
        _house.name = "House";
        _house.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _house.transform.localScale = new Vector3(1f, 1f, -1f);
        if (GameSession.IsReady)
        {
            LivingFurnish.Install(_house);
            ShopCheckout.RestoreFurniture();
        }

        if (!TryBounds(_house, out Bounds bounds))
        {
            _house.SetActive(false);
            return;
        }

        _footprint = Mathf.Max(bounds.size.x, bounds.size.z);
        _pivotToFloor = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
        CollectFloors(bounds);
        _house.SetActive(false);
    }

    void Place(ARPlane plane)
    {
        _plane = plane;
        _placed = true;
        float span = Mathf.Min(plane.size.x, plane.size.y);
        float target = Mathf.Clamp(span * 0.72f, 0.22f, 0.48f);
        _scale = _footprint > 0.01f ? target / _footprint : 0.05f;
        _house.SetActive(true);
        LockFacing(plane);
        ApplyPose(plane);
        _pet = TabletopPet.Spawn(_house.transform, _patches, _scale);
        HidePlaneMeshes();
        if (_prompt != null)
        {
            _prompt.gameObject.SetActive(false);
        }
    }

    void LockFacing(ARPlane plane)
    {
        Camera cam = Camera.main;
        Vector3 normal = plane.transform.up;
        Vector3 forward = cam != null ? cam.transform.forward : Vector3.forward;
        _facing = Vector3.ProjectOnPlane(forward, normal);
        if (_facing.sqrMagnitude < 0.0001f)
        {
            _facing = Vector3.ProjectOnPlane(Vector3.forward, normal);
        }

        _facing.Normalize();
        _facingLocked = true;
    }

    void ApplyPose(ARPlane plane)
    {
        Vector3 normal = plane.transform.up;
        Vector3 forward = _facingLocked ? Vector3.ProjectOnPlane(_facing, normal) : plane.transform.forward;
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.ProjectOnPlane(plane.transform.forward, normal);
        }

        Quaternion rotation = Quaternion.LookRotation(forward.normalized, normal);
        Vector2 center = plane.centerInPlaneSpace;
        Vector3 position = plane.transform.TransformPoint(new Vector3(center.x, 0f, center.y));
        _house.transform.localScale = new Vector3(_scale, _scale, -_scale);
        _house.transform.SetPositionAndRotation(
            position + rotation * (_pivotToFloor * _scale) + rotation * (Vector3.up * SurfaceLift),
            rotation);
    }

    ARPlane BestPlane()
    {
        Camera cam = Camera.main;
        ARPlane best = null;
        float bestScore = float.MinValue;
        foreach (ARPlane plane in _planes.trackables)
        {
            if (!IsTable(plane))
            {
                continue;
            }

            float area = plane.size.x * plane.size.y;
            Vector2 center = plane.centerInPlaneSpace;
            Vector3 world = plane.transform.TransformPoint(new Vector3(center.x, 0f, center.y));
            float edge = 0f;
            if (cam != null)
            {
                Vector3 view = cam.WorldToViewportPoint(world);
                if (view.z < 0.2f)
                {
                    continue;
                }

                edge = Mathf.Abs(view.x - 0.5f) + Mathf.Abs(view.y - 0.5f);
            }

            bool named = (plane.classifications & PlaneClassifications.Table) != 0;
            float score = area * (named ? 5f : 1f) - edge * 0.35f;
            if (score > bestScore)
            {
                bestScore = score;
                best = plane;
            }
        }

        return best;
    }

    static bool IsTable(ARPlane plane)
    {
        if (plane == null || plane.alignment != PlaneAlignment.HorizontalUp)
        {
            return false;
        }

        float area = plane.size.x * plane.size.y;
        if (area < 0.12f)
        {
            return false;
        }

        PlaneClassifications kind = plane.classifications;
        if ((kind & (PlaneClassifications.Ceiling | PlaneClassifications.WallFace)) != 0)
        {
            return false;
        }

        if ((kind & PlaneClassifications.Table) != 0)
        {
            return true;
        }

        if ((kind & PlaneClassifications.Floor) != 0)
        {
            return false;
        }

        return area <= 6f;
    }

    void HidePlaneMeshes()
    {
        if (_planes == null)
        {
            return;
        }

        foreach (ARPlane plane in _planes.trackables)
        {
            Renderer[] renderers = plane.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }
    }

    void CollectFloors(Bounds all)
    {
        _patches.Clear();
        Renderer[] renderers = _house.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            Bounds box = renderer.bounds;
            if (box.size.x < 1.2f || box.size.z < 1.2f || box.size.y > 0.7f)
            {
                continue;
            }

            _patches.Add(PatchFrom(box, 0.8f, true));
        }

        if (_patches.Count == 0)
        {
            _patches.Add(PatchFrom(all, 0.28f, false));
            return;
        }

        float lowest = float.MaxValue;
        for (int i = 0; i < _patches.Count; i++)
        {
            if (_patches[i].Y < lowest)
            {
                lowest = _patches[i].Y;
            }
        }

        TabletopPet.Patch largest = _patches[0];
        float largestArea = -1f;
        bool found = false;
        for (int i = 0; i < _patches.Count; i++)
        {
            TabletopPet.Patch patch = _patches[i];
            if (patch.Y > lowest + 0.45f)
            {
                continue;
            }

            float area = patch.Hx * patch.Hz;
            if (area > largestArea)
            {
                largestArea = area;
                largest = patch;
                found = true;
            }
        }

        _patches.Clear();
        _patches.Add(found ? largest : PatchFrom(all, 0.28f, false));
    }

    TabletopPet.Patch PatchFrom(Bounds world, float inset, bool top)
    {
        Vector3 center = _house.transform.InverseTransformPoint(world.center);
        Vector3 extents = _house.transform.InverseTransformVector(world.extents);
        float y = top ? center.y + Mathf.Abs(extents.y) : center.y - Mathf.Abs(extents.y);
        return new TabletopPet.Patch
        {
            X = center.x,
            Z = center.z,
            Hx = Mathf.Abs(extents.x) * inset,
            Hz = Mathf.Abs(extents.z) * inset,
            Y = y
        };
    }

    static bool TryBounds(GameObject house, out Bounds bounds)
    {
        Renderer[] renderers = house.GetComponentsInChildren<Renderer>();
        bounds = default;
        bool any = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || !renderers[i].enabled)
            {
                continue;
            }

            if (!any)
            {
                bounds = renderers[i].bounds;
                any = true;
            }
            else
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        return any;
    }

    void BuildHud()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var events = new GameObject("EventSystem");
            events.AddComponent<UnityEngine.EventSystems.EventSystem>();
            events.AddComponent<InputSystemUIInputModule>();
        }

        var canvasGo = new GameObject("Tabletop Hud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        var safe = new GameObject("Safe", typeof(RectTransform), typeof(SafeAreaFitter));
        Stretch(safe, canvasGo.transform);

        var promptGo = new GameObject("Prompt", typeof(RectTransform));
        promptGo.transform.SetParent(safe.transform, false);
        var promptRt = promptGo.GetComponent<RectTransform>();
        promptRt.anchorMin = new Vector2(0.08f, 0.84f);
        promptRt.anchorMax = new Vector2(0.92f, 0.96f);
        promptRt.offsetMin = Vector2.zero;
        promptRt.offsetMax = Vector2.zero;
        _prompt = promptGo.AddComponent<TextMeshProUGUI>();
        _prompt.font = Font();
        _prompt.text = "Наведи камеру на стол";
        _prompt.fontSize = 42f;
        _prompt.fontStyle = FontStyles.Bold;
        _prompt.alignment = TextAlignmentOptions.Center;
        _prompt.color = new Color(1f, 0.97f, 0.88f, 1f);
        _prompt.outlineColor = new Color(0.05f, 0.08f, 0.04f, 0.92f);
        _prompt.outlineWidth = 0.2f;
        _prompt.raycastTarget = false;

        var back = new GameObject("Back", typeof(RectTransform), typeof(Image), typeof(Button));
        back.layer = 5;
        back.transform.SetParent(safe.transform, false);
        _backRect = back.GetComponent<RectTransform>();
        _backRect.anchorMin = new Vector2(0.1f, 0.02f);
        _backRect.anchorMax = new Vector2(0.9f, 0.1f);
        _backRect.offsetMin = Vector2.zero;
        _backRect.offsetMax = Vector2.zero;
        var image = back.GetComponent<Image>();
        image.color = new Color(1f, 0.97f, 0.88f, 0.96f);
        image.raycastTarget = true;
        var button = back.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(Leave);

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(back.transform, false);
        Stretch(labelGo, back.transform);
        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.font = _prompt.font;
        label.text = "Назад";
        label.fontSize = 40f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.14f, 0.16f, 0.08f, 1f);
        label.raycastTarget = false;
    }

    static void Stretch(GameObject go, Transform parent)
    {
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void SetPrompt(string text)
    {
        if (_prompt != null)
        {
            _prompt.text = text;
        }
    }

    static TMP_FontAsset Font()
    {
        if (TMP_Settings.defaultFontAsset != null)
        {
            return TMP_Settings.defaultFontAsset;
        }

        return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    }

    void Leave()
    {
        if (_leaving)
        {
            return;
        }

        _leaving = true;
        SceneManager.LoadScene(BootController.GameSceneName);
    }

    bool WantLeave()
    {
        if (_leaving)
        {
            return false;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return true;
        }

        if (!WasTap(out Vector2 screen))
        {
            return false;
        }

        return OverBack(screen);
    }

    bool OverBack(Vector2 screen)
    {
        if (_backRect == null)
        {
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(_backRect, screen, null);
    }

    static bool WasTap(out Vector2 screen)
    {
        Touchscreen touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
        {
            screen = touch.primaryTouch.position.ReadValue();
            return true;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            screen = mouse.position.ReadValue();
            return true;
        }

        screen = default;
        return false;
    }

    static void MuteScreenSpaceInteractor()
    {
        GameObject found = GameObject.Find("Screen Space Ray Interactor");
        if (found != null)
        {
            found.SetActive(false);
        }
    }
}

public sealed class TabletopPet
{
    public struct Patch
    {
        public float X;
        public float Z;
        public float Hx;
        public float Hz;
        public float Y;
    }

    const float Speed = 0.75f;

    readonly GameObject _body;
    readonly Animator _animator;
    readonly Patch _patch;
    readonly float _scale;
    readonly float _feet;
    Vector3 _local;
    Vector3 _goal;
    float _wait = 0.8f;
    bool _moving;

    TabletopPet(GameObject body, Animator animator, Patch patch, float scale, float feet)
    {
        _body = body;
        _animator = animator;
        _patch = patch;
        _scale = Mathf.Max(0.0001f, scale);
        _feet = feet;
        _local = new Vector3(patch.X, StandY(), patch.Z);
        _goal = _local;
    }

    public static TabletopPet Spawn(Transform house, List<Patch> patches, float scale)
    {
        if (house == null)
        {
            return null;
        }

        PetLookCatalog catalog = PetLookCatalog.Load();
        if (catalog == null || catalog.Cat == null)
        {
            Debug.LogError("[Finashka] Не удалось показать питомца: нет модели в каталоге.");
            return null;
        }

        Patch patch = patches != null && patches.Count > 0
            ? patches[0]
            : new Patch { X = 0f, Z = 0f, Hx = 0.6f, Hz = 0.6f, Y = 0f };

        GameObject cat = Object.Instantiate(catalog.Cat);
        cat.name = PetLook.HomeObjectName;
        PetLook.PrepareLocomotion(cat);
        PetLook.ScaleToHeight(cat, 1.05f * scale);
        int color = 0;
        int hat = PetLook.NoHat;
        if (GameSession.IsReady)
        {
            PetLook.Read(GameSession.State, out color, out hat);
        }

        PetLook.Apply(cat, color, hat);
        Collider[] colliders = cat.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        float feet = 0f;
        if (PetLook.TryBodyBounds(cat, out Bounds bounds))
        {
            feet = cat.transform.position.y - bounds.min.y;
        }

        var pet = new TabletopPet(cat, cat.GetComponentInChildren<Animator>(), patch, scale, feet);
        pet.Seat(house);
        return pet;
    }

    public void Tick(Transform house)
    {
        if (house == null || _body == null)
        {
            return;
        }

        if (_moving)
        {
            Vector3 next = Vector3.MoveTowards(
                new Vector3(_local.x, 0f, _local.z),
                new Vector3(_goal.x, 0f, _goal.z),
                Speed * Time.deltaTime);
            Vector3 delta = next - new Vector3(_local.x, 0f, _local.z);
            _local.x = next.x;
            _local.z = next.z;
            _local.y = StandY();
            if (delta.sqrMagnitude > 0.000001f)
            {
                PetLook.FaceTowards(_body, house.TransformVector(delta));
            }

            if ((next - new Vector3(_goal.x, 0f, _goal.z)).sqrMagnitude < 0.0004f)
            {
                _moving = false;
                _wait = Random.Range(1.1f, 3.2f);
            }
        }
        else
        {
            _wait -= Time.deltaTime;
            if (_wait <= 0f)
            {
                _goal = _local;
                for (int i = 0; i < 6; i++)
                {
                    float x = _patch.X + Random.Range(-_patch.Hx, _patch.Hx);
                    float z = _patch.Z + Random.Range(-_patch.Hz, _patch.Hz);
                    if (i < 5 && !Clear(house, x, z))
                    {
                        continue;
                    }

                    _goal = new Vector3(x, StandY(), z);
                    break;
                }

                _moving = true;
            }
        }

        Seat(house);
        DriveAnimation();
    }

    void Seat(Transform house)
    {
        _body.transform.position = house.TransformPoint(_local);
    }

    bool Clear(Transform house, float x, float z)
    {
        Vector3 world = house.TransformPoint(new Vector3(x, StandY() + 0.25f, z));
        return !Physics.CheckSphere(world, 0.12f * _scale, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    float StandY()
    {
        return _patch.Y + _feet / _scale;
    }

    void DriveAnimation()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetBool("is_standing", _moving);
        _animator.SetBool("is_walking", _moving);
    }
}
