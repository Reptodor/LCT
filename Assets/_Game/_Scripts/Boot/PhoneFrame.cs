using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhoneFrame : MonoBehaviour
{
    const float Aspect = 9f / 16f;
    static readonly Color Bar = new Color(0.06f, 0.07f, 0.08f, 1f);

    Camera _bars;
    int _width;
    int _height;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (Application.isMobilePlatform)
        {
            return;
        }

        var go = new GameObject("PhoneFrame");
        DontDestroyOnLoad(go);
        go.AddComponent<PhoneFrame>();
    }

    void Awake()
    {
        var bars = new GameObject("PhoneBars");
        bars.transform.SetParent(transform, false);
        _bars = bars.AddComponent<Camera>();
        _bars.clearFlags = CameraClearFlags.SolidColor;
        _bars.backgroundColor = Bar;
        _bars.cullingMask = 0;
        _bars.depth = -100;
        _bars.orthographic = true;
        _bars.rect = new Rect(0f, 0f, 1f, 1f);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Apply();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply();
    }

    void Update()
    {
        if (_width != Screen.width || _height != Screen.height)
        {
            Apply();
        }
    }

    void Apply()
    {
        _width = Screen.width;
        _height = Mathf.Max(1, Screen.height);
        float window = (float)_width / _height;
        Rect view;
        if (window > Aspect + 0.01f)
        {
            float width = Aspect / window;
            view = new Rect((1f - width) * 0.5f, 0f, width, 1f);
        }
        else if (window < Aspect - 0.01f)
        {
            float height = window / Aspect;
            view = new Rect(0f, (1f - height) * 0.5f, 1f, height);
        }
        else
        {
            view = new Rect(0f, 0f, 1f, 1f);
        }

        var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == _bars)
            {
                continue;
            }

            cameras[i].rect = view;
        }

        Camera uiCamera = null;
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != _bars && cameras[i].CompareTag("MainCamera"))
            {
                uiCamera = cameras[i];
                break;
            }
        }

        if (uiCamera == null)
        {
            return;
        }

        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                continue;
            }

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera;
            float near = uiCamera.nearClipPlane + 0.2f;
            float far = uiCamera.farClipPlane - 0.2f;
            if (canvas.planeDistance < near || canvas.planeDistance > far)
            {
                canvas.planeDistance = Mathf.Clamp(1f, near, far);
            }
        }
    }
}
