using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class LivingFurnish
{
    struct Pack
    {
        public string Id;
        public string Room;
        public string[] Parts;
    }

    static readonly Pack[] Packs =
    {
        new Pack { Id = "Bed", Room = "BedRoom", Parts = new[] { "bedSingle" } },
        new Pack { Id = "ComputerDesk", Room = "BedRoom", Parts = new[] { "deskCorner", "computerScreen", "computerKeyboard", "computerMouse", "chairDesk", "books" } },
        new Pack { Id = "Bathtub", Room = "Bathroom", Parts = new[] { "bathtub" } },
        new Pack { Id = "Toilet", Room = "Bathroom", Parts = new[] { "toilet" } },
        new Pack { Id = "Vanity", Room = "Bathroom", Parts = new[] { "bathroomSink", "bathroomMirror" } },
        new Pack { Id = "Sofa", Room = "Mainroom", Parts = new[] { "loungeSofaCorner" } },
        new Pack { Id = "LoungeChair", Room = "Mainroom", Parts = new[] { "loungeChairRelax" } },
        new Pack { Id = "CoffeeTable", Room = "Mainroom", Parts = new[] { "tableCoffee" } },
        new Pack { Id = "Television", Room = "Mainroom", Parts = new[] { "televisionModern" } },
        new Pack { Id = "Bookcase", Room = "Mainroom", Parts = new[] { "bookcaseClosed", "books", "books (1)", "books (2)" } },
        new Pack { Id = "DiningSet", Room = "kitchen", Parts = new[] { "tableCrossCloth", "chairCushion", "chairCushion (1)", "chairCushion (2)", "chairCushion (3)" } },
        new Pack { Id = "Fridge", Room = "kitchen", Parts = new[] { "kitchenFridge" } },
        new Pack { Id = "KitchenCounter", Room = "kitchen", Parts = new[] { "kitchenCabinetCornerInner", "kitchenCabinetCornerInner (1)", "kitchenStoveElectric", "kitchenCabinetDrawer", "kitchenCabinet", "kitchenCabinet (1)", "kitchenCabinet (2)", "kitchenCoffeeMachine", "kitchenCabinetUpper" } }
    };

    public static void Install(GameObject house)
    {
        for (int i = 0; i < Packs.Length; i++)
        {
            SetVisible(Packs[i], false);
        }
    }

    public static bool Place(string itemId)
    {
        for (int i = 0; i < Packs.Length; i++)
        {
            if (Packs[i].Id != itemId)
            {
                continue;
            }

            return SetVisible(Packs[i], true);
        }

        return false;
    }

    static bool SetVisible(Pack pack, bool visible)
    {
        var found = Collect(pack);
        if (found.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < found.Count; i++)
        {
            if (visible)
            {
                found[i].SetActive(true);
                EnsureBlocker(found[i]);
                BindPick(found[i], pack.Id, true);
            }
            else
            {
                BindPick(found[i], pack.Id, false);
                found[i].SetActive(false);
            }
        }

        return true;
    }

    public static void SetBroken(string itemId, bool broken)
    {
        for (int i = 0; i < Packs.Length; i++)
        {
            if (Packs[i].Id != itemId)
            {
                continue;
            }

            var found = Collect(Packs[i]);
            for (int p = 0; p < found.Count; p++)
            {
                Tint(found[p], broken);
            }

            return;
        }
    }

    static void BindPick(GameObject go, string itemId, bool visible)
    {
        var pick = go.GetComponent<FurniturePick>();
        if (pick == null)
        {
            pick = go.AddComponent<FurniturePick>();
        }

        pick.ItemId = itemId;
        Collider collider = visible ? EnsurePickVolume(go) : FindPickCollider(go);
        if (collider != null)
        {
            collider.enabled = visible;
        }
    }

    static Collider FindPickCollider(GameObject go)
    {
        Collider own = go.GetComponent<Collider>();
        if (own != null)
        {
            return own;
        }

        Transform pick = go.transform.Find("Pick");
        return pick != null ? pick.GetComponent<BoxCollider>() : null;
    }

    static Collider EnsurePickVolume(GameObject go)
    {
        Collider existing = FindPickCollider(go);
        if (existing != null)
        {
            return existing;
        }

        existing = go.GetComponentInChildren<Collider>(true);
        if (existing != null)
        {
            return existing;
        }

        if (!TryWorldBounds(go, out Bounds bounds))
        {
            return null;
        }

        var pickGo = new GameObject("Pick");
        pickGo.transform.SetParent(go.transform, true);
        pickGo.transform.SetPositionAndRotation(bounds.center, Quaternion.identity);
        var box = pickGo.AddComponent<BoxCollider>();
        Vector3 lossy = pickGo.transform.lossyScale;
        box.size = new Vector3(
            bounds.size.x / Mathf.Max(0.001f, Mathf.Abs(lossy.x)),
            bounds.size.y / Mathf.Max(0.001f, Mathf.Abs(lossy.y)),
            bounds.size.z / Mathf.Max(0.001f, Mathf.Abs(lossy.z)));
        return box;
    }

    static bool TryWorldBounds(GameObject go, out Bounds bounds)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        bounds = default;
        bool any = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].bounds.size.sqrMagnitude < 0.000001f)
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

    static void Tint(GameObject go, bool broken)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!broken)
            {
                renderers[i].SetPropertyBlock(null);
                continue;
            }

            var block = new MaterialPropertyBlock();
            renderers[i].GetPropertyBlock(block);
            var tint = new Color(0.32f, 0.32f, 0.32f, 1f);
            block.SetColor("_BaseColor", tint);
            block.SetColor("_Color", tint);
            renderers[i].SetPropertyBlock(block);
        }
    }

    static List<GameObject> Collect(Pack pack)
    {
        var found = new List<GameObject>();
        Transform room = FindRoom(pack.Room);
        if (room == null)
        {
            return found;
        }

        var pending = new List<string>(pack.Parts);
        for (int i = 0; i < room.childCount; i++)
        {
            Transform child = room.GetChild(i);
            int index = pending.IndexOf(child.name);
            if (index < 0)
            {
                continue;
            }

            found.Add(child.gameObject);
            pending.RemoveAt(index);
        }

        return found;
    }

    static void EnsureBlocker(GameObject go)
    {
        if (go.GetComponentInChildren<NavMeshObstacle>(true) != null)
        {
            return;
        }

        var renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var blocker = new GameObject("Block");
        blocker.transform.SetParent(go.transform, true);
        blocker.transform.SetPositionAndRotation(bounds.center, Quaternion.identity);
        var obstacle = blocker.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.carving = true;
        obstacle.carveOnlyStationary = true;
        Vector3 lossy = blocker.transform.lossyScale;
        obstacle.size = new Vector3(
            bounds.size.x * 0.82f / Mathf.Max(0.001f, Mathf.Abs(lossy.x)),
            bounds.size.y * 0.82f / Mathf.Max(0.001f, Mathf.Abs(lossy.y)),
            bounds.size.z * 0.82f / Mathf.Max(0.001f, Mathf.Abs(lossy.z)));
    }

    static Transform FindRoom(string name)
    {
        Transform house = null;
        var roots = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == "House" && roots[i].parent == null)
            {
                house = roots[i];
                break;
            }
        }

        if (house == null)
        {
            return null;
        }

        Transform fallback = null;
        for (int i = 0; i < house.childCount; i++)
        {
            Transform child = house.GetChild(i);
            if (child.name != name)
            {
                continue;
            }

            if (child.childCount > 0)
            {
                return child;
            }

            fallback = child;
        }

        return fallback;
    }
}
