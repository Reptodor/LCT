using UnityEngine;
using UnityEngine.UI;

namespace LCT.MiniGames.Finance
{
    public static class FinancePiggy
    {
        public static readonly Color Body = new Color(1f, 0.68f, 0.76f, 1f);
        public static readonly Color Shade = new Color(0.94f, 0.49f, 0.61f, 1f);
        public static readonly Color Eye = new Color(0.22f, 0.12f, 0.15f, 1f);

        const float BaseWidth = 320f;
        const float BaseHeight = 270f;

        public static RectTransform Build(Transform parent, string name, float width)
        {
            var root = FinanceUi.Node(parent, name);
            root.sizeDelta = new Vector2(width, width * BaseHeight / BaseWidth);

            var art = FinanceUi.Node(root, "Art");
            art.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(BaseWidth, BaseHeight));
            art.localScale = Vector3.one * (width / BaseWidth);

            Part(art, "LegBackL", Shade, new Vector2(-92f, -100f), new Vector2(46f, 64f), 18f);
            Part(art, "LegBackR", Shade, new Vector2(88f, -100f), new Vector2(46f, 64f), 18f);
            Part(art, "EarL", Shade, new Vector2(-40f, 96f), new Vector2(62f, 62f), 16f).localRotation = Quaternion.Euler(0f, 0f, 45f);
            Part(art, "EarR", Shade, new Vector2(52f, 92f), new Vector2(56f, 56f), 16f).localRotation = Quaternion.Euler(0f, 0f, 45f);
            Part(art, "Tail", Shade, new Vector2(-150f, 14f), new Vector2(40f, 40f), 20f);

            Part(art, "Body", Body, new Vector2(0f, -4f), new Vector2(300f, 220f), 110f);
            Part(art, "Belly", new Color(1f, 0.78f, 0.84f, 1f), new Vector2(-10f, -40f), new Vector2(200f, 110f), 55f);

            Part(art, "LegFrontL", Shade, new Vector2(-50f, -108f), new Vector2(46f, 58f), 18f);
            Part(art, "LegFrontR", Shade, new Vector2(46f, -108f), new Vector2(46f, 58f), 18f);

            Part(art, "Slot", Eye, new Vector2(-8f, 92f), new Vector2(96f, 16f), 8f);
            Part(art, "Snout", Shade, new Vector2(134f, -10f), new Vector2(84f, 72f), 36f);
            Dot(art, "NostrilL", Eye, new Vector2(122f, -10f), 16f);
            Dot(art, "NostrilR", Eye, new Vector2(146f, -10f), 16f);
            Dot(art, "Eye", Eye, new Vector2(70f, 34f), 28f);
            Dot(art, "EyeShine", Color.white, new Vector2(76f, 40f), 9f);
            Dot(art, "Cheek", new Color(1f, 0.45f, 0.55f, 0.45f), new Vector2(64f, -8f), 36f);

            return root;
        }

        static RectTransform Part(RectTransform parent, string name, Color color, Vector2 position, Vector2 size, float radius)
        {
            var image = FinanceUi.Box(parent, name, color, radius);
            image.rectTransform.Place(new Vector2(0.5f, 0.5f), position, size);
            return image.rectTransform;
        }

        static void Dot(RectTransform parent, string name, Color color, Vector2 position, float diameter)
        {
            Image dot = FinanceUi.Dot(parent, name, color, diameter);
            dot.rectTransform.Place(new Vector2(0.5f, 0.5f), position, new Vector2(diameter, diameter));
        }
    }
}
