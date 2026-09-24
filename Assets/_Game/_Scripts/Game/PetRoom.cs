using UnityEngine;
using UnityEngine.SceneManagement;

public static class PetRoom
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureOnPlay()
    {
        if (!SceneManager.GetActiveScene().name.Contains("Game"))
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
