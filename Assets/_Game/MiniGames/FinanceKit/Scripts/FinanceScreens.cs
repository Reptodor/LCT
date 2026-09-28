using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LCT.MiniGames.Finance
{
    public class IntroInfo
    {
        public string Topic;
        public string Title;
        public string Body;
        public string[] Steps;
        public Sprite Art;
        public Action<RectTransform> BuildArt;
        public Action OnPlay;
    }

    public struct ResultButton
    {
        public Sprite Sprite;
        public string Text;
        public Color Color;
        public Vector2 Size;
        public Action OnClick;
    }

    public class ResultInfo
    {
        public string Title;
        public Color TitleColor = FinanceUi.Ink;
        public int Stars = -1;
        public string Body;
        public string Lesson;
        public readonly List<ResultButton> Buttons = new List<ResultButton>();
    }

    public class FinanceScreens
    {
        const float CardWidth = 940f;

        readonly MonoBehaviour _host;
        readonly RectTransform _layer;
        readonly RectTransform _toastLayer;
        readonly FinanceTheme _theme;

        GameObject _screen;
        RectTransform _toast;
        Image _toastBack;
        Text _toastTitle;
        Text _toastBody;
        CanvasGroup _toastGroup;
        Coroutine _toastRoutine;

        public bool IsOpen => _screen != null;

        public FinanceScreens(MonoBehaviour host, RectTransform layer, RectTransform toastLayer, FinanceTheme theme)
        {
            _host = host;
            _layer = layer;
            _toastLayer = toastLayer;
            _theme = theme;
        }

        public void Hide()
        {
            if (_screen != null)
                Object.Destroy(_screen);
            _screen = null;
        }

        public void ShowIntro(IntroInfo info)
        {
            bool hasArt = info.Art != null || info.BuildArt != null;
            int steps = info.Steps != null ? info.Steps.Length : 0;
            float height = 70f + 70f + 30f + 150f + (hasArt ? 330f : 0f) + 190f + steps * 96f + 60f + 260f + 50f;
            RectTransform card = OpenCard(height);

            float y = 70f;
            var chip = FinanceUi.Box(card, "Topic", FinanceUi.Orange, 35f);
            chip.rectTransform.anchorMin = chip.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            chip.rectTransform.pivot = new Vector2(0.5f, 1f);
            chip.rectTransform.anchoredPosition = new Vector2(0f, -y + 20f);
            chip.rectTransform.sizeDelta = new Vector2(640f, 70f);
            FinanceUi.Label(chip.transform, "Text", info.Topic.ToUpperInvariant(), 34, Color.white).rectTransform.Stretch(20f, 4f, 20f, 4f);
            y += 100f;

            FinanceUi.Label(card, "Title", info.Title, 86, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Heavy)
                .rectTransform.TopBand(y, 150f, 40f);
            y += 160f;

            if (hasArt)
            {
                var art = FinanceUi.Node(card, "Art").TopBand(y, 300f, 120f);
                if (info.BuildArt != null)
                    info.BuildArt(art);
                else
                    FinanceUi.Picture(art, "Image", info.Art).rectTransform.Stretch();
                FinanceTween.Pop(_host, art, 0.4f, 0.45f);
                y += 330f;
            }

            FinanceUi.Label(card, "Body", info.Body, 44, FinanceUi.Muted, TextAnchor.MiddleCenter, FontWeight.Regular)
                .rectTransform.TopBand(y, 170f, 60f);
            y += 190f;

            for (int i = 0; i < steps; i++)
            {
                var row = FinanceUi.Node(card, "Step" + i).TopBand(y, 84f, 60f);
                var dot = FinanceUi.Dot(row, "Number", FinanceUi.Leaf, 64f);
                dot.rectTransform.anchorMin = dot.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                dot.rectTransform.anchoredPosition = new Vector2(32f, 0f);
                FinanceUi.Label(dot.transform, "Text", (i + 1).ToString(), 36, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy)
                    .rectTransform.Stretch();
                FinanceUi.Label(row, "Text", info.Steps[i], 40, FinanceUi.Ink, TextAnchor.MiddleLeft)
                    .rectTransform.Stretch(90f, 0f, 0f, 0f);
                y += 96f;
            }

            Button play = FinanceUi.SpriteButton(card, "Play", _theme != null ? _theme.playButton : null, "ИГРАТЬ", FinanceUi.Orange, () =>
            {
                Hide();
                info.OnPlay?.Invoke();
            });
            var playRect = (RectTransform)play.transform;
            playRect.anchorMin = playRect.anchorMax = new Vector2(0.5f, 0f);
            playRect.pivot = new Vector2(0.5f, 0f);
            playRect.anchoredPosition = new Vector2(0f, 50f);
            playRect.sizeDelta = new Vector2(480f, 260f);
        }

        public void ShowResult(ResultInfo info)
        {
            bool hasStars = info.Stars >= 0;
            bool hasLesson = !string.IsNullOrEmpty(info.Lesson);
            float height = 60f + 140f + (hasStars ? 200f : 0f) + 230f + (hasLesson ? 300f : 0f) + 280f;
            RectTransform card = OpenCard(height);

            float y = 60f;
            FinanceUi.Label(card, "Title", info.Title, 78, info.TitleColor, TextAnchor.MiddleCenter, FontWeight.Heavy)
                .rectTransform.TopBand(y, 130f, 40f);
            y += 140f;

            if (hasStars)
            {
                var row = FinanceUi.Node(card, "Stars").TopBand(y, 180f, 0f);
                for (int i = 0; i < 3; i++)
                {
                    bool full = i < info.Stars;
                    Sprite sprite = _theme != null ? (full ? _theme.starFull : _theme.starEmpty) : null;
                    Image star;
                    if (sprite != null)
                    {
                        star = FinanceUi.Picture(row, "Star" + i, sprite);
                    }
                    else
                    {
                        star = FinanceUi.Dot(row, "Star" + i, full ? FinanceUi.Gold : new Color(0.8f, 0.8f, 0.75f), 150f);
                    }

                    float size = i == 1 ? 180f : 150f;
                    star.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 200f, i == 1 ? 12f : -8f), new Vector2(size, size));
                    star.transform.localScale = Vector3.zero;
                    _host.StartCoroutine(PopLater(star.transform, 0.15f + i * 0.18f, full));
                }

                y += 200f;
            }

            FinanceUi.Label(card, "Body", info.Body, 46, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Bold)
                .rectTransform.TopBand(y, 210f, 50f);
            y += 230f;

            if (hasLesson)
            {
                var lesson = FinanceUi.Box(card, "Lesson", FinanceUi.Mint, 36f);
                lesson.rectTransform.TopBand(y, 270f, 50f);
                FinanceUi.Label(lesson.transform, "Header", "ЗАПОМНИ", 34, FinanceUi.Leaf, TextAnchor.UpperCenter, FontWeight.Heavy)
                    .rectTransform.TopBand(18f, 50f, 30f);
                FinanceUi.Label(lesson.transform, "Text", info.Lesson, 40, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Regular)
                    .rectTransform.Stretch(34f, 70f, 34f, 18f);
            }

            var buttons = FinanceUi.Node(card, "Buttons");
            buttons.BottomBand(40f, 230f, 30f);
            var layout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 40f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            foreach (ResultButton spec in info.Buttons)
            {
                ResultButton captured = spec;
                Button button = FinanceUi.SpriteButton(buttons, "Button", spec.Sprite, spec.Text, spec.Color, () =>
                {
                    Hide();
                    captured.OnClick?.Invoke();
                });
                Vector2 size = spec.Size != Vector2.zero ? spec.Size : new Vector2(220f, 220f);
                if (spec.Sprite == null)
                    size = new Vector2(Mathf.Max(size.x, 320f), 150f);
                ((RectTransform)button.transform).sizeDelta = size;
            }
        }

        public void Toast(string title, string details, Color color, float seconds = 1.9f)
        {
            if (_toast == null)
                BuildToast();

            _toastBack.color = color;
            _toastTitle.text = title;
            _toastBody.text = details;
            _toastBody.gameObject.SetActive(!string.IsNullOrEmpty(details));
            _toastTitle.rectTransform.Stretch(30f, 16f, 30f, string.IsNullOrEmpty(details) ? 16f : 110f);
            _toast.gameObject.SetActive(true);
            _toast.SetAsLastSibling();
            _toastGroup.alpha = 1f;

            if (_toastRoutine != null)
                _host.StopCoroutine(_toastRoutine);
            FinanceTween.Pop(_host, _toast, 0.85f, 0.22f);
            _toastRoutine = _host.StartCoroutine(HideToastLater(seconds));
        }

        public void HideToast()
        {
            if (_toast != null)
                _toast.gameObject.SetActive(false);
        }

        void BuildToast()
        {
            _toastBack = FinanceUi.Box(_toastLayer, "Toast", FinanceUi.Coral, 40f, true);
            _toast = _toastBack.rectTransform;
            _toast.Place(new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(940f, 220f));
            _toastGroup = _toast.gameObject.AddComponent<CanvasGroup>();
            _toastGroup.blocksRaycasts = false;
            _toastGroup.interactable = false;

            _toastTitle = FinanceUi.Label(_toast, "Title", "", 54, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _toastTitle.Outlined(new Color(0f, 0f, 0f, 0.2f), 2f);
            _toastBody = FinanceUi.Label(_toast, "Details", "", 40, Color.white, TextAnchor.MiddleCenter, FontWeight.Regular);
            _toastBody.rectTransform.Stretch(30f, 96f, 30f, 18f);
            _toast.gameObject.SetActive(false);
        }

        IEnumerator HideToastLater(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            yield return FinanceTween.Run(0.25f, k =>
            {
                if (_toastGroup != null)
                    _toastGroup.alpha = 1f - k;
            });
            if (_toast != null)
                _toast.gameObject.SetActive(false);
            _toastRoutine = null;
        }

        RectTransform OpenCard(float height)
        {
            Hide();
            var dim = FinanceUi.Box(_layer, "Screen", FinanceUi.Dim, 0f);
            dim.raycastTarget = true;
            dim.rectTransform.Stretch();
            _screen = dim.gameObject;

            var card = FinanceUi.Box(dim.transform, "Card", FinanceUi.Cream, 56f, true);
            card.raycastTarget = true;
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(CardWidth, height));
            FinanceTween.Pop(_host, card.transform, 0.82f, 0.32f);
            return card.rectTransform;
        }

        IEnumerator PopLater(Transform target, float delay, bool full)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (target == null)
                yield break;
            FinanceTween.Pop(_host, target, 0.2f, full ? 0.4f : 0.25f);
            if (full)
                FinanceAudio.Coin();
        }
    }
}
