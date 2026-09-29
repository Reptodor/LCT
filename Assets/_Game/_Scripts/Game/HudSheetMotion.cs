using System.Collections.Generic;
using UnityEngine;

public static class HudSheetMotion
{
    public const float OpenDuration = 0.38f;
    public const float CloseDuration = 0.22f;
    const float StartScale = 0.86f;
    const float StartOffsetY = -56f;
    const float CardDuration = 0.34f;
    const float CardStagger = 0.09f;
    const float CardStartScale = 0.64f;
    const float CardRise = 42f;

    public static void SnapHidden(CanvasGroup group, RectTransform sheet, RectTransform[] cards)
    {
        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        if (sheet != null)
        {
            sheet.localScale = new Vector3(StartScale, StartScale, 1f);
            sheet.anchoredPosition = new Vector2(0f, StartOffsetY);
        }

        PlaceCards(cards, -1f);
    }

    public static void ApplyOpen(float linear, CanvasGroup group, RectTransform sheet)
    {
        float scaleEase = LevelSelectEase.OutBack(linear);
        float fade = LevelSelectEase.OutCubic(linear);
        if (group != null)
        {
            group.alpha = fade;
        }

        if (sheet == null)
        {
            return;
        }

        float scale = Mathf.LerpUnclamped(StartScale, 1f, scaleEase);
        sheet.localScale = new Vector3(scale, scale, 1f);
        float y = Mathf.LerpUnclamped(StartOffsetY, 0f, scaleEase);
        sheet.anchoredPosition = new Vector2(0f, y);
    }

    public static void ApplyClose(float linear, CanvasGroup group, RectTransform sheet, float fromAlpha, float fromScale, Vector2 fromPos)
    {
        float k = LevelSelectEase.InCubic(linear);
        if (group != null)
        {
            group.alpha = Mathf.Lerp(fromAlpha, 0f, k);
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        if (sheet == null)
        {
            return;
        }

        float scale = Mathf.Lerp(fromScale, StartScale, k);
        sheet.localScale = new Vector3(scale, scale, 1f);
        sheet.anchoredPosition = Vector2.Lerp(fromPos, new Vector2(0f, StartOffsetY), k);
    }

    public static void PlaceCards(RectTransform[] cards, float time)
    {
        if (cards == null)
        {
            return;
        }

        for (int i = 0; i < cards.Length; i++)
        {
            RectTransform card = cards[i];
            if (card == null)
            {
                continue;
            }

            float linear = time < 0f ? 0f : Mathf.Clamp01((time - CardStagger * i) / CardDuration);
            float eased = LevelSelectEase.OutBack(linear);
            float fade = LevelSelectEase.OutCubic(linear);
            float scale = Mathf.LerpUnclamped(CardStartScale, 1f, eased);
            card.localScale = new Vector3(scale, scale, 1f);
            card.anchoredPosition = new Vector2(0f, Mathf.LerpUnclamped(CardRise, 0f, eased));
            var group = card.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = fade;
            }
        }
    }

    public static float CardTotal(int count)
    {
        if (count <= 0)
        {
            return 0f;
        }

        return CardStagger * (count - 1) + CardDuration;
    }

    public static RectTransform[] Visuals(GameObject[] rows)
    {
        var list = new List<RectTransform>();
        if (rows == null)
        {
            return list.ToArray();
        }

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null || !rows[i].activeInHierarchy)
            {
                continue;
            }

            var visual = rows[i].transform.Find("Visual") as RectTransform;
            if (visual != null)
            {
                list.Add(visual);
            }
        }

        return list.ToArray();
    }
}
