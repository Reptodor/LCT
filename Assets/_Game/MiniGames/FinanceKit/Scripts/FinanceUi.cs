using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LCT.MiniGames.Finance
{
    public enum FontWeight
    {
        Regular,
        Bold,
        Heavy
    }

    public static class FinanceUi
    {
        public static readonly Color Cream = new Color(1f, 0.97f, 0.9f, 1f);
        public static readonly Color Paper = new Color(0.95f, 0.92f, 0.84f, 1f);
        public static readonly Color Ink = new Color(0.16f, 0.2f, 0.15f, 1f);
        public static readonly Color Muted = new Color(0.38f, 0.42f, 0.36f, 1f);
        public static readonly Color Forest = new Color(0.12f, 0.34f, 0.2f, 0.94f);
        public static readonly Color Leaf = new Color(0.34f, 0.7f, 0.27f, 1f);
        public static readonly Color Mint = new Color(0.86f, 0.95f, 0.82f, 1f);
        public static readonly Color Orange = new Color(1f, 0.55f, 0.12f, 1f);
        public static readonly Color Coral = new Color(0.92f, 0.32f, 0.27f, 1f);
        public static readonly Color Sky = new Color(0.2f, 0.53f, 0.93f, 1f);
        public static readonly Color Gold = new Color(1f, 0.79f, 0.2f, 1f);
        public static readonly Color Violet = new Color(0.55f, 0.36f, 0.86f, 1f);
        public static readonly Color Dim = new Color(0f, 0f, 0f, 0.6f);

        public static FinanceTheme Theme;

        static Font _fallbackFont;

        public static Font GetFont(FontWeight weight)
        {
            Font font = null;
            if (Theme != null)
            {
                if (weight == FontWeight.Heavy)
                    font = Theme.heavyFont;
                else if (weight == FontWeight.Bold)
                    font = Theme.boldFont;
                else
                    font = Theme.regularFont;

                if (font == null)
                    font = Theme.boldFont;
            }

            if (font == null)
            {
                if (_fallbackFont == null)
                    _fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                font = _fallbackFont;
            }

            return font;
        }

        public static string Rub(int value)
        {
            string digits = Mathf.Abs(value).ToString("#,0", CultureInfo.InvariantCulture).Replace(',', ' ');
            return (value < 0 ? "−" : "") + digits + " руб";
        }

        public static string SignedRub(int value)
        {
            return (value > 0 ? "+" : "") + Rub(value);
        }

        public static string Hex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        public static string Paint(string text, Color color)
        {
            return "<color=" + Hex(color) + ">" + text + "</color>";
        }

        public static RectTransform Node(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static RectTransform Stretch(this RectTransform rect, float left = 0f, float top = 0f, float right = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        public static RectTransform Place(this RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static RectTransform TopBand(this RectTransform rect, float top, float height, float sidePadding = 0f)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(-sidePadding * 2f, height);
            return rect;
        }

        public static RectTransform BottomBand(this RectTransform rect, float bottom, float height, float sidePadding = 0f)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, bottom);
            rect.sizeDelta = new Vector2(-sidePadding * 2f, height);
            return rect;
        }

        public static Image Box(Transform parent, string name, Color color, float radius = 40f, bool shadow = false)
        {
            var rect = Node(parent, name);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (radius > 0f)
            {
                image.sprite = FinanceSprites.Rounded;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = FinanceSprites.RoundedRadius / radius;
            }

            if (shadow)
            {
                var effect = rect.gameObject.AddComponent<Shadow>();
                effect.effectColor = new Color(0f, 0f, 0f, 0.28f);
                effect.effectDistance = new Vector2(0f, -10f);
            }

            return image;
        }

        public static Image Dot(Transform parent, string name, Color color, float diameter)
        {
            var rect = Node(parent, name);
            rect.sizeDelta = new Vector2(diameter, diameter);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = FinanceSprites.Circle;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static Image Picture(Transform parent, string name, Sprite sprite, bool preserveAspect = true)
        {
            var rect = Node(parent, name);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = false;
            image.enabled = sprite != null;
            return image;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter, FontWeight weight = FontWeight.Bold)
        {
            var rect = Node(parent, name);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = GetFont(weight);
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = anchor;
            label.supportRichText = true;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.resizeTextForBestFit = true;
            label.resizeTextMaxSize = size;
            label.resizeTextMinSize = Mathf.Max(12, Mathf.RoundToInt(size * 0.55f));
            label.raycastTarget = false;
            return label;
        }

        public static Text Outlined(this Text label, Color color, float distance = 3f)
        {
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
            return label;
        }

        public static Button TextButton(Transform parent, string name, string text, Color color, UnityAction onClick,
            int fontSize = 52, float radius = 44f)
        {
            var image = Box(parent, name, color, radius, true);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = ButtonColors();

            var label = Label(image.transform, "Label", text, fontSize, Color.white);
            label.rectTransform.Stretch(24f, 10f, 24f, 10f);
            label.Outlined(new Color(0f, 0f, 0f, 0.18f), 2f);

            if (onClick != null)
                button.onClick.AddListener(onClick);

            image.gameObject.AddComponent<FinancePressFx>();
            return button;
        }

        public static Button SpriteButton(Transform parent, string name, Sprite sprite, string fallbackText, Color fallbackColor,
            UnityAction onClick)
        {
            if (sprite == null)
                return TextButton(parent, name, fallbackText, fallbackColor, onClick);

            var rect = Node(parent, name);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = ButtonColors();
            if (onClick != null)
                button.onClick.AddListener(onClick);

            rect.gameObject.AddComponent<FinancePressFx>();
            return button;
        }

        public static Text ButtonText(this Button button)
        {
            return button != null ? button.GetComponentInChildren<Text>(true) : null;
        }

        static ColorBlock ButtonColors()
        {
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.97f, 0.97f, 0.97f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.62f, 0.62f, 0.62f, 0.6f);
            colors.fadeDuration = 0.08f;
            return colors;
        }
    }
}
