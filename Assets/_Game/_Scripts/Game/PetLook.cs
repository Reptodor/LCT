using UnityEngine;

public static class PetLook
{
    public const int ColorCount = 4;
    public const int HatCount = 4;
    public const int NoHat = 3;
    public const string HomeObjectName = "Finashka";

    public static readonly Color[] Colors =
    {
        new Color(0.9056604f, 0.7222667f, 0.5254539f, 1f),
        new Color(0.55f, 0.58f, 0.64f, 1f),
        new Color(0.94f, 0.91f, 0.86f, 1f),
        new Color(0.20f, 0.16f, 0.15f, 1f)
    };

    public static readonly string[] ColorNames = { "Рыжий", "Серый", "Белый", "Чёрный" };
    public static readonly string[] HatNames = { "Кепка", "Шапка", "Цилиндр", "Нет" };

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    public static int ClampColor(int index)
    {
        return Mathf.Clamp(index, 0, ColorCount - 1);
    }

    public static int ClampHat(int index)
    {
        return Mathf.Clamp(index, 0, HatCount - 1);
    }

    public static void Read(GameState state, out int color, out int hat)
    {
        if (state == null || !state.petLookSet)
        {
            color = 0;
            hat = NoHat;
            return;
        }

        color = ClampColor(state.petColor);
        hat = ClampHat(state.petHat);
    }

    public static void Apply(GameObject cat, int colorIndex, int hatIndex)
    {
        ApplyColor(cat, colorIndex);
        ApplyHat(cat, hatIndex);
    }

    public static void ApplyColor(GameObject cat, int colorIndex)
    {
        if (cat == null)
        {
            return;
        }

        Paint(cat, ClampColor(colorIndex));
    }

    public static void ApplyHat(GameObject cat, int hatIndex)
    {
        if (cat == null)
        {
            return;
        }

        ShowHat(cat, ClampHat(hatIndex));
    }

    public static void KeepPrefabPose(GameObject cat)
    {
        if (cat == null)
        {
            return;
        }

        Animator[] animators = cat.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].applyRootMotion = false;
            animators[i].enabled = false;
        }
    }

    public static void SeatOnFloor(GameObject cat, float floorY)
    {
        if (cat == null || !TryBounds(cat, IncludeBody, out Bounds bounds))
        {
            return;
        }

        float lift = floorY - bounds.min.y;
        cat.transform.position += Vector3.up * lift;
    }

    public static void ScaleToHeight(GameObject cat, float targetHeight)
    {
        if (cat == null || targetHeight <= 0.01f || !TryBounds(cat, IncludeBody, out Bounds bounds))
        {
            return;
        }

        if (bounds.size.y < 0.001f)
        {
            return;
        }

        float factor = targetHeight / bounds.size.y;
        cat.transform.localScale *= factor;
    }

    public static bool TryBodyBounds(GameObject cat, out Bounds bounds)
    {
        return TryBounds(cat, IncludeBody, out bounds);
    }

    public static bool TryPreviewBounds(GameObject cat, out Bounds bounds)
    {
        return TryBounds(cat, IncludePreview, out bounds);
    }

    public static Vector3 FaceDirection(GameObject cat)
    {
        if (cat == null)
        {
            return Vector3.forward;
        }

        Transform eyes = FindNamed(cat.transform, "Eyeses");
        Vector3 origin = cat.transform.position;
        if (TryBodyBounds(cat, out Bounds bounds))
        {
            origin = bounds.center;
        }

        Vector3 face = eyes != null ? eyes.position - origin : cat.transform.forward;
        face.y = 0f;
        if (face.sqrMagnitude < 0.0001f)
        {
            face = Vector3.forward;
        }

        return face.normalized;
    }

    public static void FaceTowards(GameObject cat, Vector3 worldDirection)
    {
        if (cat == null)
        {
            return;
        }

        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Vector3 face = FaceDirection(cat);
        Quaternion turn = Quaternion.FromToRotation(face, worldDirection.normalized);
        cat.transform.rotation = turn * cat.transform.rotation;
    }

    static void Paint(GameObject cat, int colorIndex)
    {
        Color color = Colors[colorIndex];
        Renderer[] renderers = cat.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (!UsesCatColor(renderer))
            {
                continue;
            }

            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            renderer.SetPropertyBlock(block);
        }
    }

    static bool UsesCatColor(Renderer renderer)
    {
        Material[] materials = renderer.sharedMaterials;
        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material != null && material.name.Contains("Color_cat"))
            {
                return true;
            }
        }

        return false;
    }

    static void ShowHat(GameObject cat, int hatIndex)
    {
        Transform[] all = cat.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            int slot = HatSlot(all[i].name);
            if (slot < 0)
            {
                continue;
            }

            all[i].gameObject.SetActive(slot == hatIndex);
        }
    }

    static int HatSlot(string name)
    {
        if (name == "Кепка")
        {
            return 0;
        }

        if (name == "Шапка")
        {
            return 1;
        }

        if (name == "Цилиндр")
        {
            return 2;
        }

        return -1;
    }

    static Transform FindNamed(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindNamed(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    static bool IncludePreview(Renderer renderer)
    {
        if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
        {
            return false;
        }

        return !IsHatOrPodium(renderer.transform);
    }

    static bool IncludeBody(Renderer renderer)
    {
        if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
        {
            return false;
        }

        return !IsHatOrPodium(renderer.transform);
    }

    static bool IsHatOrPodium(Transform cursor)
    {
        while (cursor != null)
        {
            if (cursor.name == "PreviewPodium" || HatSlot(cursor.name) >= 0)
            {
                return true;
            }

            cursor = cursor.parent;
        }

        return false;
    }

    static bool TryBounds(GameObject root, System.Predicate<Renderer> include, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bounds = default;
        bool any = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (include != null && !include(renderers[i]))
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
}
