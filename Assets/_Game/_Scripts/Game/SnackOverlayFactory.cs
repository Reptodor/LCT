using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class SnackOverlayFactory
{
    static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Gold = new Color(0.96f, 0.78f, 0.28f, 1f);
    static readonly Color Muted = new Color(0.86f, 0.9f, 0.78f, 1f);
    static readonly Color Snack = new Color(0.7f, 0.32f, 0.14f, 1f);
    static readonly Color Dim = new Color(0.04f, 0.08f, 0.03f, 0.78f);
    static readonly Color Sheet = new Color(0.11f, 0.26f, 0.15f, 0.98f);
    static readonly Color Card = new Color(0.07f, 0.16f, 0.09f, 1f);
    static readonly Color Buy = new Color(0.95f, 0.74f, 0.18f, 1f);
    static readonly Color Ink = new Color(0.14f, 0.16f, 0.08f, 1f);
    static readonly Color Track = new Color(0.08f, 0.14f, 0.07f, 0.55f);

    public static void Build(Transform canvas, SnackShopView view)
    {
        Transform existing = canvas.Find("SnackOverlay");
        if (existing != null)
        {
            Object.Destroy(existing.gameObject);
        }

        var overlay = new GameObject("SnackOverlay", typeof(RectTransform), typeof(CanvasGroup));
        Stretch(overlay, canvas);
        var group = overlay.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        overlay.SetActive(false);

        var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image), typeof(Button));
        Stretch(dimGo, overlay.transform);
        var dimImg = Paint(dimGo, Dim, true);
        var dimBtn = dimGo.GetComponent<Button>();
        dimBtn.targetGraphic = dimImg;
        dimBtn.transition = Selectable.Transition.None;

        var safe = new GameObject("SheetSafe", typeof(RectTransform), typeof(SafeAreaFitter));
        Stretch(safe, overlay.transform);
        Column(safe, new RectOffset(28, 28, 20, 24), 0f, TextAnchor.MiddleCenter);

        var sheet = new GameObject("Sheet", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        sheet.transform.SetParent(safe.transform, false);
        Paint(sheet, Sheet, true);
        sheet.GetComponent<LayoutElement>().flexibleHeight = 1f;

        var motion = new GameObject("Motion", typeof(RectTransform));
        Stretch(motion, sheet.transform);
        Column(motion, new RectOffset(28, 28, 28, 22), 16f, TextAnchor.UpperCenter);

        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        header.transform.SetParent(motion.transform, false);
        PrefHeight(header, 92f);
        var headerRow = header.GetComponent<HorizontalLayoutGroup>();
        headerRow.spacing = 16f;
        headerRow.childAlignment = TextAnchor.MiddleCenter;
        headerRow.childControlWidth = true;
        headerRow.childControlHeight = true;
        headerRow.childForceExpandWidth = false;
        headerRow.childForceExpandHeight = true;
        var title = Label(header.transform, "SnackTitle", "Перекус", 36f, 56f, FontStyles.Bold, Gold, TextAlignmentOptions.MidlineLeft);
        title.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        var close = SlimButton(header.transform, "CloseSnackButton", "Закрыть", Track, Cream, 84f);
        var closeLayout = close.GetComponent<LayoutElement>();
        closeLayout.flexibleWidth = 0f;
        closeLayout.preferredWidth = 196f;

        var coins = Label(motion.transform, "SnackCoins", "На счету: 0", 24f, 36f, FontStyles.Bold, Cream, TextAlignmentOptions.MidlineLeft);
        PrefHeight(coins.gameObject, 52f);

        var intro = Label(motion.transform, "SnackIntro", "Еда — обязательная покупка. Она дает насыщенность.", 20f, 28f, FontStyles.Normal, Muted, TextAlignmentOptions.MidlineLeft);
        PrefHeight(intro.gameObject, 72f);

        var scrollGo = new GameObject("FoodScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
        scrollGo.transform.SetParent(motion.transform, false);
        Paint(scrollGo, new Color(0f, 0f, 0f, 0f), false);
        var scrollLayout = scrollGo.GetComponent<LayoutElement>();
        scrollLayout.flexibleHeight = 1f;
        scrollLayout.minHeight = 280f;

        var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        Stretch(viewportGo, scrollGo.transform);
        Paint(viewportGo, new Color(0f, 0f, 0f, 0.01f), true);

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var contentRt = content.GetComponent<RectTransform>();
        contentRt.SetParent(viewportGo.transform, false);
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = new Vector2(0f, 0f);
        Column(content, new RectOffset(4, 8, 4, 16), 12f, TextAnchor.UpperCenter);
        var fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.content = contentRt;
        scroll.viewport = viewportGo.GetComponent<RectTransform>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        var buyButtons = new Button[ShopCatalog.Food.Length];
        for (int i = 0; i < ShopCatalog.Food.Length; i++)
        {
            buyButtons[i] = FoodCard(content.transform, ShopCatalog.Food[i], i);
        }

        var status = Label(motion.transform, "SnackStatus", "Выбери еду", 20f, 28f, FontStyles.Normal, Muted, TextAlignmentOptions.Center);
        PrefHeight(status.gameObject, 48f);

        var visuals = new RectTransform[buyButtons.Length];
        for (int i = 0; i < buyButtons.Length; i++)
        {
            visuals[i] = content.transform.Find("Food_" + i + "/Visual") as RectTransform;
        }

        view.Setup(overlay, dimBtn, close, coins, status, buyButtons, group, motion.GetComponent<RectTransform>(), visuals);
    }

    static Button FoodCard(Transform parent, ShopItem food, int index)
    {
        var go = new GameObject("Food_" + index, typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        PrefHeight(go, 248f);
        var visualGo = new GameObject("Visual", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(VerticalLayoutGroup), typeof(LevelSelectScrollRelay));
        Stretch(visualGo, go.transform);
        Paint(visualGo, Card, true);
        var column = visualGo.GetComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(20, 20, 14, 14);
        column.spacing = 4f;
        column.childAlignment = TextAnchor.MiddleCenter;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;

        var title = Label(visualGo.transform, "Title", food.Title, 26f, 38f, FontStyles.Bold, Gold, TextAlignmentOptions.MidlineLeft);
        PrefHeight(title.gameObject, 42f);
        var price = Label(visualGo.transform, "Price", food.PriceText, 20f, 28f, FontStyles.Bold, Cream, TextAlignmentOptions.MidlineLeft);
        PrefHeight(price.gameObject, 32f);
        var effect = Label(visualGo.transform, "Effect", food.EffectText, 18f, 26f, FontStyles.Normal, Cream, TextAlignmentOptions.MidlineLeft);
        PrefHeight(effect.gameObject, 30f);
        var category = Label(visualGo.transform, "Category", food.CategoryText, 18f, 26f, FontStyles.Normal, Muted, TextAlignmentOptions.MidlineLeft);
        PrefHeight(category.gameObject, 30f);
        Button buy = SlimButton(visualGo.transform, "BuyButton", "Купить", Buy, Ink, 56f);
        buy.gameObject.AddComponent<LevelSelectScrollRelay>();
        return buy;
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

    static Image Paint(GameObject go, Color color, bool raycast)
    {
        var image = go.GetComponent<Image>();
        if (image == null)
        {
            image = go.AddComponent<Image>();
        }

        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = color;
        image.raycastTarget = raycast;
        return image;
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
