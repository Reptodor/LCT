using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class PetRoom
{
    static GameObject _wardrobeZone;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureOnPlay()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryBuild(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBuild(scene);
    }

    static void TryBuild(Scene scene)
    {
        if (!scene.name.Contains("Game"))
        {
            return;
        }

        Build(Camera.main);
    }

    public static void Build(Camera cam)
    {
        BuildApartment(cam);
    }

    static void BuildApartment(Camera cam)
    {
        DestroyNamed("RoomBackground");
        DestroyNamed("Room");

        var room = new GameObject("Room");
        const float livingW = 4.35f;
        const float bathW = 3.05f;
        const float d = 5.85f;
        const float h = 1.28f;
        const float t = 0.16f;
        const float door = 1.35f;
        float totalW = livingW + bathW;
        float xMin = -totalW * 0.5f;
        float xMax = totalW * 0.5f;
        float dividerX = xMin + livingW;
        float livingX = (xMin + dividerX) * 0.5f;
        float bathX = (dividerX + xMax) * 0.5f;
        float zMin = -d * 0.5f;
        float zMax = d * 0.5f;

        var floorLiving = Mat(new Color(0.62f, 0.44f, 0.26f, 1f), 0.22f);
        var floorBath = Mat(new Color(0.78f, 0.88f, 0.90f, 1f), 0.28f);
        var livingFront = Mat(new Color(0.96f, 0.62f, 0.42f, 1f), 0.08f);
        var livingSide = Mat(new Color(0.98f, 0.82f, 0.55f, 1f), 0.08f);
        var bathBack = Mat(new Color(0.42f, 0.70f, 0.86f, 1f), 0.10f);
        var bathSide = Mat(new Color(0.70f, 0.88f, 0.94f, 1f), 0.10f);
        var trim = Mat(new Color(0.48f, 0.30f, 0.18f, 1f), 0.18f);

        Box(room.transform, "LivingFloor", new Vector3(livingX, -t * 0.5f, 0f), new Vector3(livingW, t, d), floorLiving);
        Box(room.transform, "BathFloor", new Vector3(bathX, -t * 0.5f, 0f), new Vector3(bathW, t, d), floorBath);
        Box(room.transform, "FrontWallLiving", new Vector3(livingX - t * 0.25f, h * 0.5f, zMin), new Vector3(livingW + t * 0.5f, h, t), livingFront);
        Box(room.transform, "BackWallLiving", new Vector3(livingX - t * 0.25f, h * 0.5f, zMax), new Vector3(livingW + t * 0.5f, h, t), livingSide);
        Box(room.transform, "LeftWallLiving", new Vector3(xMin, h * 0.5f, 0f), new Vector3(t, h, d + t), livingFront);
        Box(room.transform, "FrontWallBath", new Vector3(bathX + t * 0.25f, h * 0.5f, zMin), new Vector3(bathW + t * 0.5f, h, t), bathSide);
        Box(room.transform, "BackWallBath", new Vector3(bathX + t * 0.25f, h * 0.5f, zMax), new Vector3(bathW + t * 0.5f, h, t), bathBack);
        Box(room.transform, "RightWallBath", new Vector3(xMax, h * 0.5f, 0f), new Vector3(t, h, d + t), bathSide);

        float side = (d - door) * 0.5f;
        float sideZ = (d + door) * 0.25f;
        Box(room.transform, "DividerNear", new Vector3(dividerX, h * 0.5f, -sideZ), new Vector3(t, h, side), trim);
        Box(room.transform, "DividerFar", new Vector3(dividerX, h * 0.5f, sideZ), new Vector3(t, h, side), trim);

        PlaceWardrobe(room.transform, xMin, dividerX, zMin, zMax);

        var pet = GameObject.Find("Monetok");
        if (pet != null)
        {
            const float petScale = 0.55f;
            pet.transform.localScale = Vector3.one * petScale;
            pet.transform.SetPositionAndRotation(new Vector3(livingX, petScale, 0f), Quaternion.identity);
            PetWalk.Attach(pet, cam, livingX, bathX);
        }

        if (cam != null)
        {
            const float pitch = 56f;
            var focus = new Vector3(livingX, 0.4f, 0f);
            float rad = pitch * Mathf.Deg2Rad;
            var lookDir = new Vector3(0f, -Mathf.Sin(rad), Mathf.Cos(rad));
            cam.orthographic = true;
            cam.orthographicSize = 4.25f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.36f, 0.12f, 0.46f, 1f);
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 50f;
            cam.transform.SetPositionAndRotation(focus - lookDir * 14f, Quaternion.Euler(pitch, 0f, 0f));
        }

        var sun = Object.FindFirstObjectByType<Light>();
        if (sun != null)
        {
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.color = new Color(1f, 0.96f, 0.90f, 1f);
            sun.intensity = 1.15f;
            sun.transform.rotation = Quaternion.Euler(68f, -24f, 0f);
        }
    }

    // Площадки шкафа. Чтобы добавить место, допиши строку в список ниже.
    // AgainstLeft / AgainstRight ставят шкаф длинной стороной вдоль боковой стены.
    // AgainstBack / AgainstFront ставят его вдоль дальней или ближней стены, разворот другой.
    // wallX / wallZ — координата стены. along — где на этой стене стоит центр шкафа.
    // depth — толщина шкафа от стены в комнату, её берём с префаба.
    static WardrobeSpot[] WardrobeLayout(float xMin, float xMax, float zMin, float zMax, float depth)
    {
        const float wall = 0.08f;
        return new[]
        {
            WardrobeSpot.AgainstLeft("LeftBack", xMin, zMax - 1.15f, wall, depth),
            WardrobeSpot.AgainstLeft("LeftFront", xMin, zMin + 1.15f, wall, depth),
            WardrobeSpot.AgainstBack("BackDoor", xMax - 0.9f, zMax, wall, depth)
        };
    }

    static void PlaceWardrobe(Transform room, float xMin, float xMax, float zMin, float zMax)
    {
        var prefab = Resources.Load<GameObject>("Wardrobe");
        if (prefab == null)
        {
            return;
        }

        var wardrobe = Object.Instantiate(prefab, room);
        wardrobe.name = "Wardrobe";
        float depth = wardrobe.transform.localScale.x;
        float height = wardrobe.transform.localScale.y;
        float width = wardrobe.transform.localScale.z;
        var spots = WardrobeLayout(xMin, xMax, zMin, zMax, depth);

        _wardrobeZone = new GameObject("WardrobeZone");
        _wardrobeZone.transform.SetParent(room, false);
        var pads = _wardrobeZone.AddComponent<WardrobePads>();
        var zoneMat = Mat(new Color(0.42f, 0.86f, 0.46f, 1f), 0.08f);
        var chosenMat = Mat(new Color(0.95f, 0.78f, 0.28f, 1f), 0.12f);
        var padScale = new Vector3(depth + 0.36f, 0.045f, width + 0.36f);
        for (int i = 0; i < spots.Length; i++)
        {
            var spot = spots[i];
            var pad = Box(_wardrobeZone.transform, "WardrobeSpot", spot.Position + new Vector3(0f, 0.03f, 0f), padScale, zoneMat);
            pad.transform.localRotation = Quaternion.Euler(0f, spot.Yaw, 0f);
            var col = pad.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
            }

            pads.Add(pad, spot);
        }

        pads.Bind(wardrobe.transform, height, zoneMat, chosenMat);
        pads.MoveTo(0);
        _wardrobeZone.SetActive(false);
    }

    public static void ShowWardrobeZone(bool show)
    {
        if (_wardrobeZone != null)
        {
            _wardrobeZone.SetActive(show);
        }
    }

    static void DestroyNamed(string name)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            return;
        }

        Object.DestroyImmediate(go);
    }

    static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Place(go, parent, name, pos, Quaternion.identity, scale, mat);
        return go;
    }

    static void Place(GameObject go, Transform parent, string name, Vector3 pos, Quaternion rot, Vector3 scale, Material mat)
    {
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = rot;
        go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        var col = go.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    static Material Mat(Color color, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Universal Render Pipeline/Simple Lit")
            ?? Shader.Find("Standard")
            ?? Shader.Find("Sprites/Default");
        var mat = new Material(shader);
        mat.color = color;
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }

        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat("_Smoothness", smoothness);
        }

        return mat;
    }
}

public struct WardrobeSpot
{
    public string Name;
    public Vector3 Position;
    public float Yaw;

    public static WardrobeSpot AgainstLeft(string name, float wallX, float alongZ, float wallInset, float depth)
    {
        return new WardrobeSpot
        {
            Name = name,
            Position = new Vector3(wallX + wallInset + depth * 0.5f, 0f, alongZ),
            Yaw = 0f
        };
    }

    public static WardrobeSpot AgainstRight(string name, float wallX, float alongZ, float wallInset, float depth)
    {
        return new WardrobeSpot
        {
            Name = name,
            Position = new Vector3(wallX - wallInset - depth * 0.5f, 0f, alongZ),
            Yaw = 180f
        };
    }

    public static WardrobeSpot AgainstBack(string name, float alongX, float wallZ, float wallInset, float depth)
    {
        return new WardrobeSpot
        {
            Name = name,
            Position = new Vector3(alongX, 0f, wallZ - wallInset - depth * 0.5f),
            Yaw = 90f
        };
    }

    public static WardrobeSpot AgainstFront(string name, float alongX, float wallZ, float wallInset, float depth)
    {
        return new WardrobeSpot
        {
            Name = name,
            Position = new Vector3(alongX, 0f, wallZ + wallInset + depth * 0.5f),
            Yaw = -90f
        };
    }
}

public class WardrobePads : MonoBehaviour
{
    struct Spot
    {
        public Transform Pad;
        public WardrobeSpot Place;
    }

    readonly System.Collections.Generic.List<Spot> _spots = new System.Collections.Generic.List<Spot>();
    Transform _wardrobe;
    float _height;
    Material _idle;
    Material _chosen;
    int _current = -1;
    bool _pressed;
    Vector2 _pressPos;

    public void Add(GameObject pad, WardrobeSpot place)
    {
        _spots.Add(new Spot { Pad = pad.transform, Place = place });
    }

    public void Bind(Transform wardrobe, float height, Material idle, Material chosen)
    {
        _wardrobe = wardrobe;
        _height = height;
        _idle = idle;
        _chosen = chosen;
    }

    public void MoveTo(int index)
    {
        if (_wardrobe == null || index < 0 || index >= _spots.Count)
        {
            return;
        }

        var place = _spots[index].Place;
        _wardrobe.localPosition = new Vector3(place.Position.x, _height * 0.5f, place.Position.z);
        _wardrobe.localRotation = Quaternion.Euler(0f, place.Yaw, 0f);
        _current = index;
        for (int i = 0; i < _spots.Count; i++)
        {
            var renderer = _spots[i].Pad.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = i == index ? _chosen : _idle;
            }
        }
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                _pressed = true;
                _pressPos = mouse.position.ReadValue();
            }
            else if (_pressed && mouse.leftButton.wasReleasedThisFrame)
            {
                TryPick(mouse.position.ReadValue());
            }
        }

        var touch = Touchscreen.current;
        if (touch == null)
        {
            return;
        }

        var press = touch.primaryTouch.press;
        if (press.wasPressedThisFrame)
        {
            _pressed = true;
            _pressPos = touch.primaryTouch.position.ReadValue();
        }
        else if (_pressed && press.wasReleasedThisFrame)
        {
            TryPick(touch.primaryTouch.position.ReadValue());
        }
    }

    void TryPick(Vector2 position)
    {
        _pressed = false;
        if ((position - _pressPos).sqrMagnitude > 48f * 48f || Camera.main == null)
        {
            return;
        }

        var ray = Camera.main.ScreenPointToRay(position);
        if (!Physics.Raycast(ray, out var hit, 80f))
        {
            return;
        }

        for (int i = 0; i < _spots.Count; i++)
        {
            if (hit.transform == _spots[i].Pad)
            {
                MoveTo(i);
                return;
            }
        }
    }
}
