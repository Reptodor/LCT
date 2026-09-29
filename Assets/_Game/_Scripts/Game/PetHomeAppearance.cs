using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PetHomeAppearance
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Hook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != BootController.GameSceneName)
        {
            return;
        }

        if (GameObject.Find(PetLook.HomeObjectName) != null)
        {
            return;
        }

        var host = new GameObject("PetHomeAppearance");
        host.AddComponent<PetHomeAppearanceRunner>();
    }
}

public class PetHomeAppearanceRunner : MonoBehaviour
{
    const float TargetHeight = 1.05f;

    void Start()
    {
        StartCoroutine(Place());
    }

    IEnumerator Place()
    {
        float waited = 0f;
        while (waited < 2f && GameObject.Find("House") == null && GameObject.Find("Room") == null)
        {
            waited += Time.deltaTime;
            yield return null;
        }

        yield return null;

        if (GameObject.Find(PetLook.HomeObjectName) != null)
        {
            Destroy(gameObject);
            yield break;
        }

        PetLookCatalog catalog = PetLookCatalog.Load();
        if (catalog == null || catalog.Cat == null)
        {
            Debug.LogError("[Finashka] Не удалось показать питомца: нет модели в каталоге.");
            Destroy(gameObject);
            yield break;
        }

        if (!GameSession.IsReady)
        {
            GameSession.Initialize(SaveService.CreateDefault());
        }

        PetLook.Read(GameSession.State, out int color, out int hat);
        Vector3 spawn = SpawnPoint();
        float floor = FloorY(spawn);
        GameObject cat = Instantiate(catalog.Cat);
        cat.name = PetLook.HomeObjectName;
        cat.transform.SetPositionAndRotation(spawn, Quaternion.identity);
        cat.transform.localScale = Vector3.one;
        PetLook.KeepPrefabPose(cat);
        yield return null;
        PetLook.ScaleToHeight(cat, TargetHeight);
        PetLook.SeatOnFloor(cat, floor);
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 toCamera = cam.transform.position - cat.transform.position;
            PetLook.FaceTowards(cat, toCamera);
        }

        PetLook.Apply(cat, color, hat);
        PetLook.SeatOnFloor(cat, floor);
        Destroy(gameObject);
    }

    static Vector3 SpawnPoint()
    {
        GameObject monetok = GameObject.Find("Monetok");
        if (monetok != null)
        {
            Vector3 feet = monetok.transform.position;
            feet.y -= monetok.transform.lossyScale.y;
            feet.x += 1.15f;
            return feet;
        }

        return Vector3.zero;
    }

    static float FloorY(Vector3 near)
    {
        Vector3 origin = near + Vector3.up * 3f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 8f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.point.y;
        }

        GameObject monetok = GameObject.Find("Monetok");
        if (monetok != null)
        {
            return monetok.transform.position.y - monetok.transform.lossyScale.y;
        }

        return near.y;
    }
}
