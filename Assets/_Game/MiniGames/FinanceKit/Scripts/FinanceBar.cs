using UnityEngine;
using UnityEngine.UI;

namespace LCT.MiniGames.Finance
{
    public class FinanceBar
    {
        public RectTransform Root { get; private set; }
        public float Value { get; private set; }

        Image _fill;
        RectTransform _fillRect;

        public static FinanceBar Create(Transform parent, string name, Color back, Color fill, float radius = 20f)
        {
            var bar = new FinanceBar();
            var background = FinanceUi.Box(parent, name, back, radius);
            bar.Root = background.rectTransform;

            bar._fill = FinanceUi.Box(background.transform, "Fill", fill, radius);
            bar._fillRect = bar._fill.rectTransform;
            bar._fillRect.anchorMin = Vector2.zero;
            bar._fillRect.anchorMax = new Vector2(0f, 1f);
            bar._fillRect.pivot = new Vector2(0f, 0.5f);
            bar._fillRect.offsetMin = Vector2.zero;
            bar._fillRect.offsetMax = Vector2.zero;
            bar.Set(0f);
            return bar;
        }

        public void Set(float value)
        {
            Value = Mathf.Clamp01(value);
            _fillRect.anchorMax = new Vector2(Value, 1f);
            _fill.enabled = Value > 0.001f;
        }

        public void Animate(MonoBehaviour host, float value, float duration = 0.35f)
        {
            FinanceTween.Value(host, Value, Mathf.Clamp01(value), duration, Set);
        }

        public void SetColor(Color color)
        {
            _fill.color = color;
        }
    }
}
