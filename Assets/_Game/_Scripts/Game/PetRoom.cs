using UnityEngine;
using UnityEngine.SceneManagement;

public enum RoomKind
{
    Living = 0,
    Bath = 1
}

public static class PetRoom
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureOnPlay()
    {
        if (!SceneManager.GetActiveScene().name.Contains("Game"))
        {
            return;
        }

        Build(Camera.main, RoomKind.Living);
    }

    public static void Build(Camera cam)
    {
        Build(cam, RoomKind.Living);
    }

    public static void Build(Camera cam, RoomKind kind)
    {
        DestroyNamed("RoomBackground");
        DestroyNamed("Room");

        var room = new GameObject("Room");
        const float w = 4.4f;
        const float d = 4.8f;
        const float h = 3.2f;
        const float t = 0.10f;
        const float z = 0.85f;
        float backZ = z + d * 0.5f - 0.03f;

        if (kind == RoomKind.Bath)
        {
            BuildBath(room.transform, w, d, h, t, z, backZ);
        }
        else
        {
            BuildLiving(room.transform, w, d, h, t, z, backZ);
        }

        var pet = GameObject.Find("Monetok");
        if (pet != null)
        {
            pet.transform.SetPositionAndRotation(new Vector3(0f, 1f, 0.22f), Quaternion.identity);
        }

        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.22f, 0.12f, 1f);
            cam.fieldOfView = 52f;
            cam.transform.SetPositionAndRotation(
                new Vector3(0f, 1.62f, -5.15f),
                Quaternion.Euler(7.5f, 0f, 0f));
        }

        var sun = Object.FindFirstObjectByType<Light>();
        if (sun != null)
        {
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.color = kind == RoomKind.Bath
                ? new Color(0.90f, 0.96f, 1f, 1f)
                : new Color(1f, 0.95f, 0.85f, 1f);
            sun.intensity = 1.05f;
            sun.transform.rotation = Quaternion.Euler(38f, -18f, 0f);
        }
    }

    static void BuildLiving(Transform room, float w, float d, float h, float t, float z, float backZ)
    {
        var floor = Mat(new Color(0.58f, 0.40f, 0.24f, 1f), 0.22f);
        var wall = Mat(new Color(0.95f, 0.90f, 0.76f, 1f), 0.08f);
        var back = Mat(new Color(0.72f, 0.84f, 0.54f, 1f), 0.08f);
        var ceil = Mat(new Color(0.97f, 0.95f, 0.88f, 1f), 0.05f);
        var wood = Mat(new Color(0.40f, 0.26f, 0.14f, 1f), 0.18f);
        var rug = Mat(new Color(0.82f, 0.34f, 0.18f, 1f), 0.10f);
        var glass = Mat(new Color(0.70f, 0.88f, 0.96f, 1f), 0.65f);
        var leaf = Mat(new Color(0.28f, 0.58f, 0.30f, 1f), 0.12f);
        var pot = Mat(new Color(0.72f, 0.38f, 0.22f, 1f), 0.16f);
        var gold = Mat(new Color(0.86f, 0.62f, 0.16f, 1f), 0.35f);
        var cream = Mat(new Color(0.98f, 0.93f, 0.80f, 1f), 0.10f);
        var bookA = Mat(new Color(0.78f, 0.22f, 0.18f, 1f), 0.12f);
        var bookB = Mat(new Color(0.22f, 0.38f, 0.62f, 1f), 0.12f);
        var bookC = Mat(new Color(0.90f, 0.72f, 0.20f, 1f), 0.12f);
        var cushion = Mat(new Color(0.90f, 0.55f, 0.22f, 1f), 0.14f);

        Shell(room, w, d, h, t, z, floor, wall, back, ceil, wood);

        Box(room, "Rug", new Vector3(0f, 0.02f, 0.25f), new Vector3(2.15f, 0.035f, 1.7f), rug);
        Box(room, "WindowFrame", new Vector3(0f, 2.05f, backZ - 0.01f), new Vector3(1.55f, 1.28f, 0.06f), wood);
        Box(room, "WindowGlass", new Vector3(0f, 2.05f, backZ - 0.04f), new Vector3(1.32f, 1.06f, 0.03f), glass);
        Box(room, "WindowBarH", new Vector3(0f, 2.05f, backZ - 0.055f), new Vector3(1.32f, 0.05f, 0.02f), wood);
        Box(room, "WindowBarV", new Vector3(0f, 2.05f, backZ - 0.055f), new Vector3(0.05f, 1.06f, 0.02f), wood);
        Box(room, "CurtainL", new Vector3(-0.92f, 2.05f, backZ - 0.07f), new Vector3(0.22f, 1.34f, 0.04f), cushion);
        Box(room, "CurtainR", new Vector3(0.92f, 2.05f, backZ - 0.07f), new Vector3(0.22f, 1.34f, 0.04f), cushion);
        Box(room, "PictureFrame", new Vector3(-1.55f, 1.95f, backZ - 0.02f), new Vector3(0.48f, 0.52f, 0.04f), gold);
        Box(room, "Picture", new Vector3(-1.55f, 1.95f, backZ - 0.04f), new Vector3(0.36f, 0.40f, 0.02f), cream);
        Box(room, "Shelf", new Vector3(1.55f, 0.92f, backZ - 0.22f), new Vector3(0.84f, 0.06f, 0.32f), wood);
        Box(room, "ShelfLegL", new Vector3(1.20f, 0.46f, backZ - 0.22f), new Vector3(0.06f, 0.86f, 0.26f), wood);
        Box(room, "ShelfLegR", new Vector3(1.88f, 0.46f, backZ - 0.22f), new Vector3(0.06f, 0.86f, 0.26f), wood);
        Box(room, "Book1", new Vector3(1.36f, 1.07f, backZ - 0.22f), new Vector3(0.08f, 0.24f, 0.18f), bookA);
        Box(room, "Book2", new Vector3(1.46f, 1.10f, backZ - 0.22f), new Vector3(0.08f, 0.30f, 0.18f), bookB);
        Box(room, "Book3", new Vector3(1.56f, 1.05f, backZ - 0.22f), new Vector3(0.08f, 0.20f, 0.18f), bookC);
        Cylinder(room, "Pot", new Vector3(-1.55f, 0.16f, 1.35f), new Vector3(0.32f, 0.16f, 0.32f), pot);
        Sphere(room, "Leaf1", new Vector3(-1.55f, 0.52f, 1.35f), 0.38f, leaf);
        Sphere(room, "Leaf2", new Vector3(-1.70f, 0.62f, 1.26f), 0.24f, leaf);
        Sphere(room, "Leaf3", new Vector3(-1.40f, 0.66f, 1.44f), 0.22f, leaf);
        Cylinder(room, "LampPole", new Vector3(1.55f, 0.55f, 1.25f), new Vector3(0.06f, 0.55f, 0.06f), wood);
        Sphere(room, "LampShade", new Vector3(1.55f, 1.18f, 1.25f), 0.24f, gold);
        Box(room, "Cushion", new Vector3(0.55f, 0.10f, 0.62f), new Vector3(0.42f, 0.14f, 0.34f), cushion);
        Box(room, "Table", new Vector3(-1.45f, 0.28f, 0.85f), new Vector3(0.55f, 0.06f, 0.40f), wood);
        Box(room, "TableLegL", new Vector3(-1.64f, 0.14f, 0.85f), new Vector3(0.06f, 0.28f, 0.06f), wood);
        Box(room, "TableLegR", new Vector3(-1.26f, 0.14f, 0.85f), new Vector3(0.06f, 0.28f, 0.06f), wood);
        Sphere(room, "CeilingLamp", new Vector3(0f, h - 0.14f, 0.45f), 0.18f, gold);
        AddLampLight(room.Find("LampShade"), new Color(1f, 0.88f, 0.62f, 1f));
    }

    static void BuildBath(Transform room, float w, float d, float h, float t, float z, float backZ)
    {
        var floor = Mat(new Color(0.70f, 0.82f, 0.86f, 1f), 0.28f);
        var wall = Mat(new Color(0.90f, 0.94f, 0.95f, 1f), 0.10f);
        var back = Mat(new Color(0.74f, 0.88f, 0.90f, 1f), 0.10f);
        var ceil = Mat(new Color(0.96f, 0.98f, 0.99f, 1f), 0.06f);
        var tile = Mat(new Color(0.55f, 0.72f, 0.76f, 1f), 0.20f);
        var white = Mat(new Color(0.96f, 0.97f, 0.98f, 1f), 0.35f);
        var water = Mat(new Color(0.42f, 0.72f, 0.88f, 1f), 0.70f);
        var chrome = Mat(new Color(0.72f, 0.78f, 0.82f, 1f), 0.65f);
        var towel = Mat(new Color(0.95f, 0.62f, 0.38f, 1f), 0.12f);
        var matte = Mat(new Color(0.48f, 0.70f, 0.68f, 1f), 0.12f);

        Shell(room, w, d, h, t, z, floor, wall, back, ceil, tile);

        Box(room, "BathMat", new Vector3(0f, 0.02f, 0.55f), new Vector3(1.6f, 0.03f, 0.9f), matte);
        Box(room, "Tub", new Vector3(0f, 0.28f, backZ - 0.75f), new Vector3(2.15f, 0.52f, 0.95f), white);
        Box(room, "TubInner", new Vector3(0f, 0.42f, backZ - 0.75f), new Vector3(1.85f, 0.18f, 0.72f), water);
        Cylinder(room, "Tap", new Vector3(0f, 0.72f, backZ - 0.38f), new Vector3(0.08f, 0.18f, 0.08f), chrome);

        Box(room, "Sink", new Vector3(-1.55f, 0.55f, backZ - 0.45f), new Vector3(0.70f, 0.12f, 0.48f), white);
        Box(room, "SinkPedestal", new Vector3(-1.55f, 0.26f, backZ - 0.45f), new Vector3(0.28f, 0.46f, 0.28f), white);
        Box(room, "MirrorFrame", new Vector3(-1.55f, 1.55f, backZ - 0.04f), new Vector3(0.62f, 0.72f, 0.04f), chrome);
        Box(room, "Mirror", new Vector3(-1.55f, 1.55f, backZ - 0.06f), new Vector3(0.50f, 0.58f, 0.02f), water);

        Box(room, "TowelRail", new Vector3(1.55f, 1.25f, backZ - 0.12f), new Vector3(0.55f, 0.05f, 0.05f), chrome);
        Box(room, "Towel", new Vector3(1.55f, 0.95f, backZ - 0.16f), new Vector3(0.42f, 0.55f, 0.06f), towel);

        Sphere(room, "CeilingLamp", new Vector3(0f, h - 0.14f, 0.45f), 0.16f, white);
        AddLampLight(room.Find("CeilingLamp"), new Color(0.85f, 0.95f, 1f, 1f));
    }

    static void Shell(Transform room, float w, float d, float h, float t, float z, Material floor, Material wall, Material back, Material ceil, Material trim)
    {
        float backZ = z + d * 0.5f - 0.03f;
        Box(room, "Floor", new Vector3(0f, -t * 0.5f, z), new Vector3(w, t, d), floor);
        Box(room, "Ceiling", new Vector3(0f, h + t * 0.5f, z), new Vector3(w, t, d), ceil);
        Box(room, "BackWall", new Vector3(0f, h * 0.5f, z + d * 0.5f), new Vector3(w, h, t), back);
        Box(room, "LeftWall", new Vector3(-w * 0.5f, h * 0.5f, z), new Vector3(t, h, d), wall);
        Box(room, "RightWall", new Vector3(w * 0.5f, h * 0.5f, z), new Vector3(t, h, d), wall);
        Box(room, "BaseBack", new Vector3(0f, 0.07f, backZ), new Vector3(w - 0.16f, 0.14f, 0.05f), trim);
        Box(room, "BaseLeft", new Vector3(-w * 0.5f + 0.06f, 0.07f, z), new Vector3(0.05f, 0.14f, d - 0.16f), trim);
        Box(room, "BaseRight", new Vector3(w * 0.5f - 0.06f, 0.07f, z), new Vector3(0.05f, 0.14f, d - 0.16f), trim);
    }

    static void AddLampLight(Transform lamp, Color color)
    {
        if (lamp == null || lamp.GetComponent<Light>() != null)
        {
            return;
        }

        var point = lamp.gameObject.AddComponent<Light>();
        point.type = LightType.Point;
        point.color = color;
        point.intensity = 1.4f;
        point.range = 7f;
        point.shadows = LightShadows.None;
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

    static void Box(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Place(go, parent, name, pos, Quaternion.identity, scale, mat);
    }

    static void Sphere(Transform parent, string name, Vector3 pos, float diameter, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Place(go, parent, name, pos, Quaternion.identity, Vector3.one * diameter, mat);
    }

    static void Cylinder(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Place(go, parent, name, pos, Quaternion.identity, scale, mat);
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
