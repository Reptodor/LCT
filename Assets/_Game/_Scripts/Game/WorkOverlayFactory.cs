using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class WorkOverlayFactory
{
    static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Gold = new Color(0.96f, 0.78f, 0.28f, 1f);
    static readonly Color Ink = new Color(0.14f, 0.16f, 0.08f, 1f);
    static readonly Color Work = new Color(0.95f, 0.74f, 0.18f, 1f);
    static readonly Color Snack = new Color(0.7f, 0.32f, 0.14f, 1f);
    static readonly Color Track = new Color(0.08f, 0.16f, 0.08f, 0.92f);
    static readonly Color Dim = new Color(0.04f, 0.08f, 0.03f, 0.78f);
    static readonly Color Sheet = new Color(0.16f, 0.32f, 0.18f, 0.98f);
    static readonly Color Card = new Color(0.10f, 0.22f, 0.12f, 1f);
    static readonly Color Need = new Color(0.28f, 0.58f, 0.34f, 1f);
    static readonly Color Want = new Color(0.93f, 0.64f, 0.18f, 1f);

    public static void Build(Transform canvas, WorkMinigamesView view)
    {
        Transform existing = canvas.Find("WorkOverlay");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var overlay = new GameObject("WorkOverlay", typeof(RectTransform));
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
        var sheetLayout = sheet.GetComponent<LayoutElement>();
        sheetLayout.flexibleHeight = 1f;
        sheetLayout.flexibleWidth = 1f;
        Column(sheet, new RectOffset(28, 28, 28, 24), 12f, TextAnchor.UpperCenter);

        var header = Label(sheet.transform, "WorkTitle", "Подработка", 34f, 52f, FontStyles.Bold, Gold);
        PrefHeight(header.gameObject, 60f);

        var pages = new GameObject("Pages", typeof(RectTransform), typeof(LayoutElement));
        pages.transform.SetParent(sheet.transform, false);
        pages.GetComponent<LayoutElement>().flexibleHeight = 1f;

        var menu = StretchPage(pages.transform, "Menu");
        Column(menu, new RectOffset(0, 0, 0, 0), 14f, TextAnchor.UpperCenter);
        var intro = Label(menu.transform, "MenuIntro", "Выбери мини-игру и заработай монеты. Финашка учится: что нужно купить, а что просто хочется.", 20f, 30f, FontStyles.Normal, Cream);
        PrefHeight(intro.gameObject, 96f);
        CoverCard(menu.transform, "NeedWantCard", LoadCover(), "Нужно или хочу?", "5 вопросов · до +15 монет");
        var soon = Label(menu.transform, "MenuSoon", "Скоро появятся новые игры", 18f, 26f, FontStyles.Italic, Cream);
        PrefHeight(soon.gameObject, 40f);
        Spacer(menu.transform);
        SlimButton(menu.transform, "CloseMenuButton", "Закрыть", Snack, Cream, 96f);

        var play = StretchPage(pages.transform, "Play");
        Column(play, new RectOffset(0, 0, 0, 0), 10f, TextAnchor.UpperCenter);
        var progress = Label(play.transform, "PlayProgress", "1 / 5", 22f, 32f, FontStyles.Bold, Gold);
        PrefHeight(progress.gameObject, 40f);

        var itemCard = new GameObject("ItemCard", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(VerticalLayoutGroup));
        itemCard.transform.SetParent(play.transform, false);
        Paint(itemCard, Card, false);
        var itemLayout = itemCard.GetComponent<LayoutElement>();
        itemLayout.minHeight = 260f;
        itemLayout.preferredHeight = 300f;
        itemLayout.flexibleHeight = 1f;
        var itemCol = itemCard.GetComponent<VerticalLayoutGroup>();
        itemCol.padding = new RectOffset(20, 20, 24, 20);
        itemCol.spacing = 12f;
        itemCol.childAlignment = TextAnchor.MiddleCenter;
        itemCol.childControlWidth = true;
        itemCol.childControlHeight = true;
        itemCol.childForceExpandWidth = true;
        itemCol.childForceExpandHeight = false;
        var itemTitle = Label(itemCard.transform, "ItemTitle", "Хлеб", 34f, 52f, FontStyles.Bold, Gold);
        PrefHeight(itemTitle.gameObject, 72f);
        var itemPrompt = Label(itemCard.transform, "ItemPrompt", "В магазине лежит свежий хлеб.", 22f, 34f, FontStyles.Normal, Cream);
        PrefHeight(itemPrompt.gameObject, 120f);

        var playFeedback = Label(play.transform, "PlayFeedback", "Это нужно купить или просто хочется?", 20f, 30f, FontStyles.Bold, Cream);
        PrefHeight(playFeedback.gameObject, 88f);
        SlimButton(play.transform, "NeedButton", "Нужно", Need, Cream, 112f);
        SlimButton(play.transform, "WantButton", "Хочу", Want, Ink, 112f);
        SlimButton(play.transform, "ClosePlayButton", "В меню", Track, Cream, 80f);

        var result = StretchPage(pages.transform, "Result");
        Column(result, new RectOffset(0, 0, 8, 0), 14f, TextAnchor.MiddleCenter);
        Spacer(result.transform);
        var resultTitle = Label(result.transform, "ResultTitle", "Супер!", 36f, 58f, FontStyles.Bold, Gold);
        PrefHeight(resultTitle.gameObject, 72f);
        var resultScore = Label(result.transform, "ResultScore", "Верно 5 из 5", 22f, 32f, FontStyles.Normal, Cream);
        PrefHeight(resultScore.gameObject, 160f);
        var resultCoins = Label(result.transform, "ResultCoins", "+15 монет", 32f, 48f, FontStyles.Bold, Gold);
        PrefHeight(resultCoins.gameObject, 64f);
        Spacer(result.transform);
        SlimButton(result.transform, "CollectButton", "Забрать монеты", Work, Ink, 120f);

        play.SetActive(false);
        result.SetActive(false);

        Transform pagesT = overlay.transform.Find("SheetSafe/Sheet/Pages");
        view.Setup(
            overlay,
            pagesT.Find("Menu").gameObject,
            pagesT.Find("Play").gameObject,
            pagesT.Find("Result").gameObject,
            overlay.transform.Find("Dim").GetComponent<Button>(),
            pagesT.Find("Menu/CloseMenuButton").GetComponent<Button>(),
            pagesT.Find("Play/ClosePlayButton").GetComponent<Button>(),
            pagesT.Find("Menu/NeedWantCard").GetComponent<Button>(),
            pagesT.Find("Play/NeedButton").GetComponent<Button>(),
            pagesT.Find("Play/WantButton").GetComponent<Button>(),
            pagesT.Find("Result/CollectButton").GetComponent<Button>(),
            pagesT.Find("Play/PlayProgress").GetComponent<TextMeshProUGUI>(),
            pagesT.Find("Play/ItemCard/ItemTitle").GetComponent<TextMeshProUGUI>(),
            pagesT.Find("Play/ItemCard/ItemPrompt").GetComponent<TextMeshProUGUI>(),
            pagesT.Find("Play/PlayFeedback").GetComponent<TextMeshProUGUI>(),
            pagesT.Find("Result/ResultTitle").GetComponent<TextMeshProUGUI>(),
            pagesT.Find("Result/ResultScore").GetComponent<TextMeshProUGUI>(),
            pagesT.Find("Result/ResultCoins").GetComponent<TextMeshProUGUI>());
    }

    static Sprite LoadCover()
    {
        Sprite cover = Resources.Load<Sprite>("cover-need-want");
        if (cover != null)
        {
            return cover;
        }

#if UNITY_EDITOR
        cover = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/_Art/Icons/cover-need-want.png");
#endif
        return cover;
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

    static GameObject StretchPage(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Stretch(go, parent);
        return go;
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

    static void Spacer(Transform parent)
    {
        var go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.GetComponent<LayoutElement>().flexibleHeight = 1f;
    }

    static TextMeshProUGUI Label(Transform parent, string name, string text, float min, float max, FontStyles style, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
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
        var label = Label(go.transform, "Label", text, 22f, 40f, FontStyles.Bold, labelColor);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        var lrt = label.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        return button;
    }

    static void CoverCard(Transform parent, string name, Sprite cover, string title, string subtitle)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(VerticalLayoutGroup));
        go.transform.SetParent(parent, false);
        Paint(go, Card, true);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 420f;
        layout.preferredHeight = 460f;
        var column = go.GetComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(20, 20, 20, 20);
        column.spacing = 10f;
        column.childAlignment = TextAnchor.UpperCenter;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;
        var button = go.GetComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();

        var coverGo = new GameObject("Cover", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        coverGo.transform.SetParent(go.transform, false);
        var coverImage = coverGo.GetComponent<Image>();
        coverImage.sprite = cover;
        coverImage.preserveAspect = true;
        coverImage.raycastTarget = false;
        coverImage.color = Color.white;
        var coverLayout = coverGo.GetComponent<LayoutElement>();
        coverLayout.minHeight = 220f;
        coverLayout.preferredHeight = 240f;

        var titleLabel = Label(go.transform, "Title", title, 28f, 40f, FontStyles.Bold, Gold);
        PrefHeight(titleLabel.gameObject, 48f);
        var subLabel = Label(go.transform, "Subtitle", subtitle, 20f, 28f, FontStyles.Normal, Cream);
        PrefHeight(subLabel.gameObject, 72f);
    }
}
