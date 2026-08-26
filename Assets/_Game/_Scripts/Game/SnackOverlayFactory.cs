using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class SnackOverlayFactory
{
    static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Gold = new Color(0.96f, 0.78f, 0.28f, 1f);
    static readonly Color Snack = new Color(0.7f, 0.32f, 0.14f, 1f);
    static readonly Color Dim = new Color(0.04f, 0.08f, 0.03f, 0.78f);
    static readonly Color Sheet = new Color(0.16f, 0.32f, 0.18f, 0.98f);
    static readonly Color Card = new Color(0.10f, 0.22f, 0.12f, 1f);
    static readonly Color Buy = new Color(0.95f, 0.74f, 0.18f, 1f);
    static readonly Color Ink = new Color(0.14f, 0.16f, 0.08f, 1f);

    public static void Build(Transform canvas, SnackShopView view)
    {
        Transform existing = canvas.Find("SnackOverlay");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var overlay = new GameObject("SnackOverlay", typeof(RectTransform));
        Stretch(overlay, canvas);
        overlay.SetActive(false);

        var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image), typeof(Button));
        Stretch(dimGo, overlay.transform);
        var dimImg = dimGo.GetComponent<Image>();
        dimImg.color = Dim;
        dimImg.raycastTarget = true;
        var dimBtn = dimGo.GetComponent<Button>();
        dimBtn.targetGraphic = dimImg;
        dimBtn.transition = Selectable.Transition.None;

        var safe = new GameObject("SheetSafe", typeof(RectTransform), typeof(SafeAreaFitter));
        Stretch(safe, overlay.transform);
        Column(safe, new RectOffset(36, 36, 36, 36), 0f, TextAnchor.MiddleCenter);

        var sheet = new GameObject("Sheet", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        sheet.transform.SetParent(safe.transform, false);
        Paint(sheet, Sheet, true);
        sheet.GetComponent<LayoutElement>().flexibleHeight = 1f;
        Column(sheet, new RectOffset(28, 28, 24, 24), 12f, TextAnchor.UpperCenter);

        var header = Label(sheet.transform, "SnackTitle", "Перекус", 34f, 52f, FontStyles.Bold, Gold, TextAlignmentOptions.Center);
        PrefHeight(header.gameObject, 56f);

        var coins = Label(sheet.transform, "SnackCoins", "Монеты: 100", 22f, 34f, FontStyles.Bold, Cream, TextAlignmentOptions.Center);
        PrefHeight(coins.gameObject, 40f);

        var intro = Label(sheet.transform, "SnackIntro", "Разная еда — разная цена. Дороже обычно сытнее.", 18f, 28f, FontStyles.Normal, Cream, TextAlignmentOptions.Center);
        PrefHeight(intro.gameObject, 64f);

        var scrollGo = new GameObject("FoodScroll", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect), typeof(LayoutElement));
        scrollGo.transform.SetParent(sheet.transform, false);
        Paint(scrollGo, new Color(0f, 0f, 0f, 0.12f), true);
        scrollGo.GetComponent<Mask>().showMaskGraphic = false;
        var scrollLayout = scrollGo.GetComponent<LayoutElement>();
        scrollLayout.flexibleHeight = 1f;
        scrollLayout.minHeight = 420f;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(scrollGo.transform, false);
        var contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;
        Column(content, new RectOffset(8, 8, 8, 8), 12f, TextAnchor.UpperCenter);
        var fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.content = contentRt;
        scroll.viewport = scrollGo.GetComponent<RectTransform>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24f;

        var buyButtons = new Button[FoodCatalog.All.Length];
        for (int i = 0; i < FoodCatalog.All.Length; i++)
        {
            buyButtons[i] = FoodCard(content.transform, FoodCatalog.All[i], i);
        }

        var status = Label(sheet.transform, "SnackStatus", "Выбери еду. Дороже — сытнее.", 18f, 28f, FontStyles.Bold, Cream, TextAlignmentOptions.Center);
        PrefHeight(status.gameObject, 56f);
        SlimButton(sheet.transform, "CloseSnackButton", "Закрыть", Snack, Cream, 96f);

        view.Setup(
            overlay,
            dimBtn,
            overlay.transform.Find("SheetSafe/Sheet/CloseSnackButton").GetComponent<Button>(),
            coins,
            status,
            buyButtons);
    }

    static Button FoodCard(Transform parent, FoodItem food, int index)
    {
        var go = new GameObject("Food_" + index, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(VerticalLayoutGroup));
        go.transform.SetParent(parent, false);
        Paint(go, Card, true);
        PrefHeight(go, 168f);
        var column = go.GetComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(16, 16, 12, 12);
        column.spacing = 4f;
        column.childAlignment = TextAnchor.MiddleCenter;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;

        var title = Label(go.transform, "Title", food.Title, 26f, 38f, FontStyles.Bold, Gold, TextAlignmentOptions.Center);
        PrefHeight(title.gameObject, 40f);
        var price = Label(go.transform, "Price", food.Cost + " монет  ·  +" + food.Hunger + " сытости", 18f, 26f, FontStyles.Bold, Cream, TextAlignmentOptions.Center);
        PrefHeight(price.gameObject, 32f);
        var hint = Label(go.transform, "Hint", food.Hint, 16f, 22f, FontStyles.Normal, Cream, TextAlignmentOptions.Center);
        PrefHeight(hint.gameObject, 28f);
        return SlimButton(go.transform, "BuyButton", "Купить", Buy, Ink, 52f);
    }

    static void Stretch(GameObject go, Transform parent)
    {
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void Paint(GameObject go, Color color, bool raycast)
    {
        var image = go.GetComponent<Image>();
        if (image == null)
        {
            image = go.AddComponent<Image>();
        }

        image.color = color;
        image.raycastTarget = raycast;
        image.type = Image.Type.Simple;
    }

    static void Column(GameObject go, RectOffset padding, float spacing, TextAnchor align)
    {
        var layout = go.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = go.AddComponent<VerticalLayoutGroup>();
        }

        layout.padding = padding;
        layout.spacing = spacing;
        layout.childAlignment = align;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    static void PrefHeight(GameObject go, float height)
    {
        var layout = go.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = go.AddComponent<LayoutElement>();
        }

        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleHeight = 0f;
    }

    static TextMeshProUGUI Label(Transform parent, string name, string text, float min, float max, FontStyles style, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = min;
        tmp.fontSizeMax = max;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        if (tmp.font == null && TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }

        return tmp;
    }

    static Button SlimButton(Transform parent, string name, string text, Color color, Color labelColor, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        Paint(go, color, true);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleWidth = 1f;
        var button = go.GetComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        var label = Label(go.transform, "Label", text, 20f, 34f, FontStyles.Bold, labelColor, TextAlignmentOptions.Center);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        var lrt = label.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        return button;
    }
}
