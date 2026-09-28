using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LCT.MiniGames.Finance
{
    public static class FinanceTween
    {
        public static float EaseOutCubic(float t)
        {
            t = 1f - t;
            return 1f - t * t * t;
        }

        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            t -= 1f;
            return 1f + c3 * t * t * t + c1 * t * t;
        }

        public static IEnumerator Run(float duration, Action<float> step)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                step(Mathf.Clamp01(time / duration));
                yield return null;
            }

            step(1f);
        }

        public static Coroutine Pop(MonoBehaviour host, Transform target, float from = 0.6f, float duration = 0.3f)
        {
            if (host == null || target == null)
                return null;

            target.localScale = Vector3.one * from;
            return host.StartCoroutine(Run(duration, k =>
            {
                if (target != null)
                    target.localScale = Vector3.one * Mathf.LerpUnclamped(from, 1f, EaseOutBack(k));
            }));
        }

        public static Coroutine Punch(MonoBehaviour host, Transform target, float amount = 0.15f, float duration = 0.25f)
        {
            if (host == null || target == null)
                return null;

            return host.StartCoroutine(Run(duration, k =>
            {
                if (target != null)
                    target.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI) * amount);
            }));
        }

        public static Coroutine Shake(MonoBehaviour host, RectTransform target, Vector2 rest, float amplitude = 18f, float duration = 0.35f)
        {
            if (host == null || target == null)
                return null;

            return host.StartCoroutine(Run(duration, k =>
            {
                if (target == null)
                    return;
                float offset = Mathf.Sin(k * Mathf.PI * 7f) * amplitude * (1f - k);
                target.anchoredPosition = rest + new Vector2(offset, 0f);
            }));
        }

        public static Coroutine Move(MonoBehaviour host, RectTransform target, Vector2 to, float duration = 0.25f)
        {
            if (host == null || target == null)
                return null;

            Vector2 from = target.anchoredPosition;
            return host.StartCoroutine(Run(duration, k =>
            {
                if (target != null)
                    target.anchoredPosition = Vector2.LerpUnclamped(from, to, EaseOutCubic(k));
            }));
        }

        public static Coroutine Fade(MonoBehaviour host, CanvasGroup group, float to, float duration = 0.25f)
        {
            if (host == null || group == null)
                return null;

            float from = group.alpha;
            return host.StartCoroutine(Run(duration, k =>
            {
                if (group != null)
                    group.alpha = Mathf.Lerp(from, to, k);
            }));
        }

        public static Coroutine Value(MonoBehaviour host, float from, float to, float duration, Action<float> setter)
        {
            if (host == null)
                return null;

            return host.StartCoroutine(Run(duration, k => setter(Mathf.LerpUnclamped(from, to, EaseOutCubic(k)))));
        }

        public static Coroutine Delay(MonoBehaviour host, float seconds, Action action)
        {
            if (host == null)
                return null;

            return host.StartCoroutine(DelayRoutine(seconds, action));
        }

        static IEnumerator DelayRoutine(float seconds, Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action?.Invoke();
        }

        public static void FloatText(MonoBehaviour host, RectTransform layer, string text, Color color, Vector3 worldPosition,
            int size = 60, float rise = 150f, float duration = 0.9f)
        {
            if (host == null || layer == null)
                return;

            var label = FinanceUi.Label(layer, "FloatText", text, size, color, TextAnchor.MiddleCenter, FontWeight.Heavy);
            label.Outlined(new Color(0f, 0f, 0f, 0.45f), 3f);
            var rect = label.rectTransform;
            rect.sizeDelta = new Vector2(700f, size * 1.6f);
            rect.position = worldPosition;
            Vector2 start = rect.anchoredPosition;
            var group = label.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            host.StartCoroutine(FloatRoutine(rect, group, start, rise, duration));
        }

        static IEnumerator FloatRoutine(RectTransform rect, CanvasGroup group, Vector2 start, float rise, float duration)
        {
            yield return Run(duration, k =>
            {
                if (rect == null)
                    return;
                rect.anchoredPosition = start + new Vector2(0f, rise * EaseOutCubic(k));
                rect.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, EaseOutBack(Mathf.Clamp01(k * 3f)));
                group.alpha = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
            });

            if (rect != null)
                UnityEngine.Object.Destroy(rect.gameObject);
        }
    }
}
