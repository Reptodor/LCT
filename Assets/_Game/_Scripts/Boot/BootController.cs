using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootController : MonoBehaviour
{
    public const string ProfileSetupSceneName = "ProfileSetup";
    public const string PetCustomizeSceneName = "PetCustomize";
    public const string GameSceneName = "Game";

    [SerializeField] LoadingView _loading;

    void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
    }

    void Start()
    {
        StartCoroutine(BootRoutine());
    }

    IEnumerator BootRoutine()
    {
        _loading?.SetTitle(AppInfo.Title);
        _loading?.SetStatus("Запуск…");
        yield return Fill(0.2f, 0.4f);

        _loading?.SetStatus("Готовим профили…");
        yield return Fill(0.55f, 0.45f);

        _loading?.SetStatus("Почти готово…");
        yield return Fill(0.85f, 0.4f);

        string nextScene = ResolveLaunchScene();
        if (!IsSceneInBuild(nextScene))
        {
            _loading?.ShowError("Сцена " + nextScene + " не в Build Settings");
            yield break;
        }

        _loading?.SetStatus(nextScene == ProfileSetupSceneName ? "Открываем вход…" : "Открываем дом…");
        yield return Fill(1f, 0.35f);
        SceneManager.LoadScene(nextScene);
    }

    static string ResolveLaunchScene()
    {
        var auth = new AuthService();
        if (auth.GetSavedCredentials() == null)
        {
            return ProfileSetupSceneName;
        }

        GameSession.BindProfile(Application.persistentDataPath, auth.ActiveProfileId, auth.LegacySaveOwner);
        if (GameSession.IsReady && GameSession.State.petLookSet)
        {
            return GameSceneName;
        }

        return PetCustomizeSceneName;
    }

    IEnumerator Fill(float target, float duration)
    {
        if (_loading == null)
        {
            yield break;
        }

        yield return _loading.FillTo(target, duration);
    }

    static bool IsSceneInBuild(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}
