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
        Transform room = FindRoom(pack.Room);
        if (room == null)
        {
            return false;
        }

        var found = new List<GameObject>();
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

        if (found.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < found.Count; i++)
        {
            EnsureBlocker(found[i]);
            found[i].SetActive(visible);
        }

        return true;
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
