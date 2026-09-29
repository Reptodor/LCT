using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindow : MonoBehaviour
{
    static readonly Color ResetColor = new Color(0.45f, 0.28f, 0.12f, 1f);
    static readonly Color ConfirmColor = new Color(0.55f, 0.18f, 0.14f, 1f);

    GameObject _root;
    CanvasGroup _group;
    RectTransform _motion;
    Slider _soundSlider;
    Slider _musicSlider;
    Button _resetButton;
    TMP_Text _soundLabel;
    TMP_Text _musicLabel;
    TMP_Text _resetLabel;
    bool _open;
    bool _busy;
    bool _confirmReset;
    bool _ignoreSlider;

    public System.Action ResetDone;
    public System.Action LogoutRequested;

    public bool IsReady => _root != null;

    public bool IsOpen => _open;

    public void Setup(
        GameObject root,
        Button dimButton,
        Button closeButton,
        Slider soundSlider,
        Slider musicSlider,
        Button resetButton,
        Button logoutButton,
        TMP_Text soundLabel,
        TMP_Text musicLabel,
        TMP_Text resetLabel,
        CanvasGroup group,
        RectTransform motion)
    {
        _root = root;
        _soundSlider = soundSlider;
        _musicSlider = musicSlider;
        _resetButton = resetButton;
        _soundLabel = soundLabel;
        _musicLabel = musicLabel;
        _resetLabel = resetLabel;
        _group = group;
        _motion = motion;

        if (dimButton != null)
        {
            dimButton.onClick.AddListener(Close);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (_soundSlider != null)
        {
            _soundSlider.onValueChanged.AddListener(OnSound);
        }

        if (_musicSlider != null)
        {
            _musicSlider.onValueChanged.AddListener(OnMusic);
        }

        if (_resetButton != null)
        {
            _resetButton.onClick.AddListener(OnReset);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogout);
        }

        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    public void Open()
    {
        if (_root == null || _busy)
        {
            return;
        }

        GameAudio.Reload();
        ClearConfirm();
        RefreshSliders();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        StopAllCoroutines();
        StartCoroutine(OpenRoutine());
    }

    public void Close()
    {
        if (_root == null || !_root.activeSelf)
        {
            return;
        }

        ClearConfirm();
        StopAllCoroutines();
        StartCoroutine(CloseRoutine());
    }

    void OnSound(float value)
    {
        if (_ignoreSlider)
        {
            return;
        }

        ClearConfirm();
        GameAudio.SetSoundVolume(value);
        RefreshCaptions();
    }

    void OnMusic(float value)
    {
        if (_ignoreSlider)
        {
            return;
        }

        ClearConfirm();
        GameAudio.SetMusicVolume(value);
        RefreshCaptions();
    }

    void OnReset()
    {
        if (!_confirmReset)
        {
            _confirmReset = true;
            if (_resetLabel != null)
            {
                _resetLabel.text = "Точно сбросить?";
            }

            Tint(_resetButton, ConfirmColor);
            return;
        }

        ClearConfirm();
        HideImmediate();
        if (ResetDone != null)
        {
            ResetDone();
        }
    }

    void OnLogout()
    {
        if (LogoutRequested != null)
        {
            LogoutRequested();
        }
    }

    void RefreshSliders()
    {
        _ignoreSlider = true;
        if (_soundSlider != null)
        {
            _soundSlider.value = GameAudio.SoundVolume;
        }

        if (_musicSlider != null)
        {
            _musicSlider.value = GameAudio.MusicVolume;
        }

        _ignoreSlider = false;
        RefreshCaptions();
    }

    void RefreshCaptions()
    {
        if (_soundLabel != null)
        {
            _soundLabel.text = "Звук  " + Percent(GameAudio.SoundVolume);
        }

        if (_musicLabel != null)
        {
            _musicLabel.text = "Музыка  " + Percent(GameAudio.MusicVolume);
        }
    }

    static string Percent(float volume)
    {
        return Mathf.RoundToInt(Mathf.Clamp01(volume) * 100f) + "%";
    }

    void ClearConfirm()
    {
        _confirmReset = false;
        if (_resetLabel != null)
        {
            _resetLabel.text = "Сбросить прогресс";
        }

        Tint(_resetButton, ResetColor);
    }

    static void Tint(Button button, Color color)
    {
        if (button == null)
        {
            return;
        }

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }
    }

    void HideImmediate()
    {
        StopAllCoroutines();
        _open = false;
        _busy = false;
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    IEnumerator OpenRoutine()
    {
        _busy = true;
        _open = true;
        HudSheetMotion.SnapHidden(_group, _motion, null);
        float time = 0f;
        while (time < HudSheetMotion.OpenDuration)
        {
            time += Time.unscaledDeltaTime;
            HudSheetMotion.ApplyOpen(Mathf.Clamp01(time / HudSheetMotion.OpenDuration), _group, _motion);
            yield return null;
        }

        HudSheetMotion.ApplyOpen(1f, _group, _motion);
        if (_group != null)
        {
            _group.blocksRaycasts = true;
            _group.interactable = true;
        }

        _busy = false;
    }

    IEnumerator CloseRoutine()
    {
        _busy = true;
        float fromAlpha = _group != null ? _group.alpha : 1f;
        float fromScale = _motion != null ? _motion.localScale.x : 1f;
        Vector2 fromPos = _motion != null ? _motion.anchoredPosition : Vector2.zero;
        float time = 0f;
        while (time < HudSheetMotion.CloseDuration)
        {
            time += Time.unscaledDeltaTime;
            HudSheetMotion.ApplyClose(Mathf.Clamp01(time / HudSheetMotion.CloseDuration), _group, _motion, fromAlpha, fromScale, fromPos);
            yield return null;
        }

        _open = false;
        _busy = false;
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }
}
