using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AllowanceWindow : MonoBehaviour
{
    GameObject _root;
    CanvasGroup _group;
    RectTransform _motion;
    TMP_Text _message;
    bool _open;
    bool _busy;

    public Action Confirmed;

    public bool IsReady => _root != null;

    public bool IsOpen => _open;

    public void Setup(GameObject root, Button confirmButton, TMP_Text message, CanvasGroup group, RectTransform motion)
    {
        _root = root;
        _message = message;
        _group = group;
        _motion = motion;
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirm);
        }

        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    public void Open(int amount)
    {
        if (_root == null || _open)
        {
            return;
        }

        if (_message != null)
        {
            _message.text = "Вы получили " + amount + " монет";
        }

        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        StopAllCoroutines();
        StartCoroutine(OpenRoutine());
    }

    void OnConfirm()
    {
        if (_busy || !_open)
        {
            return;
        }

        if (Confirmed != null)
        {
            Confirmed();
        }

        StopAllCoroutines();
        StartCoroutine(CloseRoutine());
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
