using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    [SerializeField] TMP_Text _petName;
    [SerializeField] TMP_Text _coins;
    [SerializeField] TMP_Text _hunger;
    [SerializeField] TMP_Text _feedback;
    [SerializeField] Button _workButton;
    [SerializeField] Button _snackButton;

    void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        if (!GameSession.IsReady)
        {
            GameSession.Initialize(SaveService.CreateDefault());
        }

        if (_workButton != null)
        {
            _workButton.onClick.AddListener(OnWork);
        }

        if (_snackButton != null)
        {
            _snackButton.onClick.AddListener(OnSnack);
        }

        Refresh();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            GameSession.Persist();
        }
    }

    void OnApplicationQuit()
    {
        GameSession.Persist();
    }

    void OnWork()
    {
        if (!PetActions.TryWork(GameSession.State))
        {
            return;
        }

        GameSession.Persist();
        SetFeedback("Подработал. +15 монет");
        Refresh();
    }

    void OnSnack()
    {
        if (!PetActions.TrySnack(GameSession.State))
        {
            SetFeedback("Не хватает монет на перекус");
            Refresh();
            return;
        }

        GameSession.Persist();
        SetFeedback("Перекус куплен");
        Refresh();
    }

    void Refresh()
    {
        GameState state = GameSession.State;
        if (state == null)
        {
            return;
        }

        if (_petName != null)
        {
            _petName.text = AppInfo.Title;
        }

        if (_coins != null)
        {
            _coins.text = state.coins.ToString();
        }

        if (_hunger != null)
        {
            _hunger.text = state.hunger.ToString();
        }
    }

    void SetFeedback(string text)
    {
        if (_feedback != null)
        {
            _feedback.text = text;
        }
    }
}
