using UnityEngine;

namespace LCT.MiniGames.Finance
{
    public static class FinanceSprites
    {
        public const int RoundedRadius = 64;

        static Sprite _rounded;
        static Sprite _circle;

        public static Sprite Rounded
        {
            get
            {
                if (_rounded == null)
                    _rounded = Build(RoundedRadius, true);
                return _rounded;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (_circle == null)
                    _circle = Build(RoundedRadius, false);
                return _circle;
            }
        }

        static Sprite Build(int radius, bool sliced)
        {
            int size = sliced ? radius * 2 + 2 : radius * 2;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = sliced ? "FinanceRounded" : "FinanceCircle"
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float nx = Mathf.Clamp(px, radius, size - radius);
                    float ny = Mathf.Clamp(py, radius, size - radius);
                    float distance = Vector2.Distance(new Vector2(px, py), new Vector2(nx, ny));
                    float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            Vector4 border = sliced ? new Vector4(radius, radius, radius, radius) : Vector4.zero;
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }
    }
}
