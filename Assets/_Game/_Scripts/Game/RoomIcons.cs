using UnityEngine;

public static class RoomIcons
{
    public static Sprite Bath()
    {
        return Make(DrawBath);
    }

    public static Sprite Living()
    {
        return Make(DrawLiving);
    }

    static Sprite Make(System.Action<Color32[], int> draw)
    {
        const int size = 256;
        var pixels = new Color32[size * size];
        draw(pixels, size);
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(pixels);
        tex.Apply(false, false);
        return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    static void DrawBath(Color32[] px, int s)
    {
        FillCircle(px, s, new Color32(70, 150, 158, 255));
        FillRect(px, s, 18, 70, 220, 150, new Color32(186, 226, 232, 255));
        FillRect(px, s, 48, 92, 160, 78, new Color32(246, 250, 252, 255));
        FillRect(px, s, 62, 108, 132, 42, new Color32(86, 176, 214, 255));
        FillRect(px, s, 118, 148, 18, 18, new Color32(160, 176, 186, 255));
        Ring(px, s, new Color32(255, 236, 180, 255));
    }

    static void DrawLiving(Color32[] px, int s)
    {
        FillCircle(px, s, new Color32(118, 168, 86, 255));
        FillRect(px, s, 28, 78, 200, 130, new Color32(236, 220, 170, 255));
        FillRect(px, s, 86, 118, 84, 64, new Color32(150, 210, 230, 255));
        FillRect(px, s, 28, 78, 200, 28, new Color32(148, 96, 52, 255));
        FillRect(px, s, 44, 88, 28, 42, new Color32(62, 140, 72, 255));
        Ring(px, s, new Color32(255, 236, 180, 255));
    }

    static void FillCircle(Color32[] px, int s, Color32 color)
    {
        float cx = (s - 1) * 0.5f;
        float r = s * 0.5f - 3f;
        float r2 = r * r;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = x - cx;
                float dy = y - cx;
                px[y * s + x] = dx * dx + dy * dy <= r2 ? color : new Color32(0, 0, 0, 0);
            }
        }
    }

    static void Ring(Color32[] px, int s, Color32 color)
    {
        float cx = (s - 1) * 0.5f;
        float outer = s * 0.5f - 3f;
        float inner = outer - 10f;
        float o2 = outer * outer;
        float i2 = inner * inner;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = x - cx;
                float dy = y - cx;
                float d2 = dx * dx + dy * dy;
                if (d2 <= o2 && d2 >= i2)
                {
                    px[y * s + x] = color;
                }
            }
        }
    }

    static void FillRect(Color32[] px, int s, int x, int y, int w, int h, Color32 color)
    {
        float cx = (s - 1) * 0.5f;
        float r = s * 0.5f - 8f;
        float r2 = r * r;
        int x1 = Mathf.Max(0, x);
        int y1 = Mathf.Max(0, y);
        int x2 = Mathf.Min(s, x + w);
        int y2 = Mathf.Min(s, y + h);
        for (int py = y1; py < y2; py++)
        {
            for (int pxI = x1; pxI < x2; pxI++)
            {
                float dx = pxI - cx;
                float dy = py - cx;
                if (dx * dx + dy * dy <= r2)
                {
                    px[py * s + pxI] = color;
                }
            }
        }
    }
}
