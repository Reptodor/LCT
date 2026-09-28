using System.Collections.Generic;
using LCT.MiniGames.Finance;
using UnityEngine;
using UnityEngine.UI;

namespace LCT.MiniGames.BudgetEnvelopes
{
    public class BudgetEnvelopesGame : FinanceGame
    {
        const int Needs = 0;
        const int Savings = 1;
        const int Wants = 2;

        [Header("Иконки конвертов")]
        [SerializeField] Sprite needsIcon;
        [SerializeField] Sprite wantsIcon;

        class Level
        {
            public string Name;
            public string Rule;
            public int Income;
            public int[] Wallet;
            public int[] Targets;
            public string[] Labels;
            public bool RevealTargets;
            public bool SaveFirst;
            public float ThreeStars;
            public float TwoStars;
            public string Lesson;
        }

        class Envelope
        {
            public int Id;
            public string Title;
            public Color Color;
            public int Target;
            public string Label;
            public int Sum;
            public readonly Stack<FinanceDraggable> Pieces = new Stack<FinanceDraggable>();
            public RectTransform Rect;
            public Vector2 Rest;
            public Text SumText;
            public Text TargetText;
            public FinanceBar Bar;
            public Image Check;
            public bool Full => Sum == Target;
        }

        static readonly Level[] Levels =
        {
            new Level
            {
                Name = "Первые карманные",
                Rule = "Мама дала 100 руб. Разложи: 50 — на нужное, 20 — в копилку, 30 — на желания.",
                Income = 100,
                Wallet = new[] { 50, 10, 10, 10, 10, 10 },
                Targets = new[] { 50, 20, 30 },
                Labels = new[] { "50 руб", "20 руб", "30 руб" },
                RevealTargets = true,
                ThreeStars = 12f,
                TwoStars = 25f,
                Lesson = "Бюджет — это план: ты заранее решаешь, куда пойдёт каждый рубль."
            },
            new Level
            {
                Name = "Неделя в школе",
                Rule = "Карманные на неделю — 200 руб. Нужное — 100, копилка — 40, желания — 60. Пригодится мелочь!",
                Income = 200,
                Wallet = new[] { 100, 50, 10, 10, 10, 5, 5, 5, 2, 2, 1 },
                Targets = new[] { 100, 40, 60 },
                Labels = new[] { "100 руб", "40 руб", "60 руб" },
                RevealTargets = true,
                ThreeStars = 18f,
                TwoStars = 35f,
                Lesson = "Мелочь — тоже деньги. Из монет легко собрать точную сумму."
            },
            new Level
            {
                Name = "Половина на нужное",
                Rule = "Доход — 300 руб. Половина — на нужное, 10% — в копилку, остаток — на желания.",
                Income = 300,
                Wallet = new[] { 100, 50, 50, 50, 10, 10, 10, 10, 5, 5 },
                Targets = new[] { 150, 30, 120 },
                Labels = new[] { "половина", "10%", "остаток" },
                ThreeStars = 22f,
                TwoStars = 45f,
                Lesson = "Сначала посчитай обязательное: еду, проезд, школу. Желания — из того, что осталось."
            },
            new Level
            {
                Name = "Правило 50/30/20",
                Rule = "Доход — 500 руб. 50% — на нужное, 20% — в копилку, 30% — на желания.",
                Income = 500,
                Wallet = new[] { 200, 100, 50, 50, 50, 10, 10, 10, 10, 10 },
                Targets = new[] { 250, 100, 150 },
                Labels = new[] { "50%", "20%", "30%" },
                ThreeStars = 22f,
                TwoStars = 45f,
                Lesson = "Правило 50/30/20: половина — на нужное, 30% — на желания, 20% — в сбережения."
            },
            new Level
            {
                Name = "Сначала заплати себе",
                Rule = "Заработал за лето 1000 руб. Сначала 20% — в копилку! Потом 50% — на нужное, остаток — на желания.",
                Income = 1000,
                Wallet = new[] { 500, 200, 100, 50, 50, 50, 10, 10, 10, 10, 5, 5 },
                Targets = new[] { 500, 200, 300 },
                Labels = new[] { "50%", "20%", "остаток" },
                SaveFirst = true,
                ThreeStars = 25f,
                TwoStars = 50f,
                Lesson = "«Сначала заплати себе»: откладывай сразу, как получил деньги, а не то, что случайно осталось."
            }
        };

        readonly Envelope[] _envelopes = new Envelope[3];
        readonly List<FinanceDraggable> _pieces = new List<FinanceDraggable>();

        Text _levelNumber;
        Text _levelName;
        Text _timerText;
        Text _incomeText;
        Text _ruleText;
        Text _walletTitle;
        RectTransform _notesGrid;
        RectTransform _coinsGrid;

        int _levelIndex;
        int _totalStars;
        int _mistakes;
        float _time;
        bool _playing;

        Level Current => Levels[_levelIndex];

        protected override void BuildGame()
        {
            BuildHeader();
            BuildEnvelopes();
            BuildWallet();
        }

        protected override void ShowIntro()
        {
            Screens.ShowIntro(new IntroInfo
            {
                Topic = TopicBudget,
                Title = "Конверты бюджета",
                Art = theme != null ? theme.moneyPile : null,
                Body = "Получил деньги — сразу реши, куда они пойдут. Разложи их по трём конвертам!",
                Steps = new[]
                {
                    "Перетащи купюры и монеты в конверты",
                    "Собери в каждом ровно нужную сумму",
                    "Нажми на конверт, чтобы вернуть деньги"
                },
                OnPlay = () =>
                {
                    _totalStars = 0;
                    StartLevel(0);
                }
            });
        }

        void Update()
        {
            if (!_playing)
                return;

            _time += Time.deltaTime;
            int seconds = Mathf.FloorToInt(_time);
            _timerText.text = (seconds / 60) + ":" + (seconds % 60).ToString("00");
        }

        void BuildHeader()
        {
            var header = FinanceUi.Box(GameLayer, "Header", FinanceUi.Forest, 40f, true);
            header.rectTransform.TopBand(30f, 150f, 40f);
            _levelNumber = FinanceUi.Label(header.transform, "LevelNumber", "", 34, FinanceUi.Gold, TextAnchor.UpperLeft);
            _levelNumber.rectTransform.Stretch(40f, 20f, 260f, 90f);
            _levelName = FinanceUi.Label(header.transform, "LevelName", "", 56, Color.white, TextAnchor.LowerLeft, FontWeight.Heavy);
            _levelName.rectTransform.Stretch(40f, 56f, 240f, 18f);

            var timer = FinanceUi.Box(header.transform, "Timer", FinanceUi.Cream, 30f);
            timer.rectTransform.Place(new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(190f, 96f));
            _timerText = FinanceUi.Label(timer.transform, "Text", "0:00", 50, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _timerText.rectTransform.Stretch();

            var rule = FinanceUi.Box(GameLayer, "Rule", FinanceUi.Cream, 40f, true);
            rule.rectTransform.TopBand(200f, 260f, 40f);
            _incomeText = FinanceUi.Label(rule.transform, "Income", "", 60, FinanceUi.Ink, TextAnchor.UpperCenter, FontWeight.Heavy);
            _incomeText.rectTransform.TopBand(22f, 80f, 30f);
            _ruleText = FinanceUi.Label(rule.transform, "Rule", "", 40, FinanceUi.Muted, TextAnchor.MiddleCenter, FontWeight.Regular);
            _ruleText.rectTransform.Stretch(40f, 105f, 40f, 18f);
        }

        void BuildEnvelopes()
        {
            var row = FinanceUi.Node(GameLayer, "Envelopes").TopBand(500f, 520f, 0f);
            string[] titles = { "НУЖНОЕ", "КОПИЛКА", "ЖЕЛАНИЯ" };
            Color[] colors = { FinanceUi.Leaf, FinanceUi.Sky, FinanceUi.Orange };

            for (int i = 0; i < 3; i++)
            {
                var envelope = new Envelope { Id = i, Title = titles[i], Color = colors[i] };
                _envelopes[i] = envelope;

                var body = FinanceUi.Box(row, "Envelope_" + titles[i], colors[i], 40f, true);
                body.raycastTarget = true;
                envelope.Rect = body.rectTransform;
                envelope.Rest = new Vector2((i - 1) * 340f, 0f);
                envelope.Rect.Place(new Vector2(0.5f, 0.5f), envelope.Rest, new Vector2(316f, 500f));
                body.gameObject.AddComponent<FinanceDropTarget>().Id = i;

                var button = body.gameObject.AddComponent<Button>();
                button.targetGraphic = body;
                button.transition = Selectable.Transition.None;
                int id = i;
                button.onClick.AddListener(() => UndoLast(id));

                var flapClip = FinanceUi.Node(body.transform, "FlapClip");
                flapClip.TopBand(0f, 170f, 0f);
                flapClip.gameObject.AddComponent<RectMask2D>();
                var flap = FinanceUi.Box(flapClip, "Flap", Color.Lerp(colors[i], Color.black, 0.18f), 24f);
                flap.rectTransform.Place(new Vector2(0.5f, 1f), Vector2.zero, new Vector2(224f, 224f));
                flap.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

                FinanceUi.Label(body.transform, "Title", titles[i], 42, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy)
                    .Outlined(new Color(0f, 0f, 0f, 0.25f)).rectTransform.TopBand(18f, 60f, 10f);

                var icon = FinanceUi.Node(body.transform, "Icon").TopBand(100f, 130f, 0f);
                if (i == Savings)
                {
                    FinancePiggy.Build(icon, "Piggy", 150f).Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150f, 127f));
                }
                else
                {
                    var disc = FinanceUi.Dot(icon, "Disc", new Color(1f, 1f, 1f, 0.85f), 130f);
                    disc.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130f, 130f));
                    FinanceUi.Picture(disc.transform, "Image", i == Needs ? needsIcon : wantsIcon).rectTransform.Stretch(16f, 16f, 16f, 16f);
                }

                envelope.SumText = FinanceUi.Label(body.transform, "Sum", "0 руб", 60, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy);
                envelope.SumText.Outlined(new Color(0f, 0f, 0f, 0.25f));
                envelope.SumText.rectTransform.TopBand(250f, 80f, 12f);
                envelope.TargetText = FinanceUi.Label(body.transform, "Target", "", 36, new Color(1f, 1f, 1f, 0.92f), TextAnchor.MiddleCenter, FontWeight.Bold);
                envelope.TargetText.rectTransform.TopBand(330f, 60f, 12f);

                envelope.Bar = FinanceBar.Create(body.transform, "Bar", new Color(0f, 0f, 0f, 0.22f), Color.white, 16f);
                envelope.Bar.Root.BottomBand(34f, 32f, 30f);

                envelope.Check = FinanceUi.Picture(body.transform, "Check", theme != null ? theme.checkMark : null);
                envelope.Check.rectTransform.Place(new Vector2(1f, 1f), new Vector2(-18f, -18f), new Vector2(110f, 110f));
                envelope.Check.gameObject.SetActive(false);
            }

            FinanceUi.Label(GameLayer, "Hint", "Нажми на конверт, чтобы вернуть последнюю купюру", 36, Color.white, TextAnchor.MiddleCenter, FontWeight.Regular)
                .Outlined(new Color(0f, 0f, 0f, 0.35f)).rectTransform.TopBand(1035f, 70f, 60f);
        }

        void BuildWallet()
        {
            var wallet = FinanceUi.Box(GameLayer, "Wallet", FinanceUi.Forest, 48f, true);
            wallet.rectTransform.BottomBand(30f, 700f, 40f);

            _walletTitle = FinanceUi.Label(wallet.transform, "Title", "КОШЕЛЁК", 42, FinanceUi.Gold, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _walletTitle.rectTransform.TopBand(20f, 64f, 30f);

            var content = FinanceUi.Node(wallet.transform, "Content").Stretch(30f, 100f, 30f, 24f);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 28f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            _notesGrid = Grid(content, "Banknotes", new Vector2(280f, 112f), new Vector2(22f, 18f));
            _coinsGrid = Grid(content, "Coins", new Vector2(128f, 128f), new Vector2(18f, 14f));
        }

        static RectTransform Grid(Transform parent, string name, Vector2 cell, Vector2 spacing)
        {
            var rect = FinanceUi.Node(parent, name);
            var grid = rect.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = cell;
            grid.spacing = spacing;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            return rect;
        }

        void StartLevel(int index)
        {
            _levelIndex = index;
            _mistakes = 0;
            _time = 0f;
            Level level = Current;

            _levelNumber.text = "УРОВЕНЬ " + (index + 1) + " ИЗ " + Levels.Length;
            _levelName.text = level.Name;
            _incomeText.text = "Доход: " + FinanceUi.Rub(level.Income);
            _ruleText.text = level.Rule;
            _walletTitle.text = "КОШЕЛЁК · " + FinanceUi.Rub(level.Income);

            for (int i = 0; i < 3; i++)
            {
                Envelope envelope = _envelopes[i];
                envelope.Target = level.Targets[i];
                envelope.Label = level.Labels[i];
                envelope.Sum = 0;
                envelope.Pieces.Clear();
                envelope.Rect.anchoredPosition = envelope.Rest;
                RefreshEnvelope(envelope);
                FinanceTween.Pop(this, envelope.Rect, 0.7f, 0.3f + i * 0.08f);
            }

            FillWallet(level.Wallet);
            _playing = true;
        }

        void FillWallet(int[] values)
        {
            foreach (FinanceDraggable piece in _pieces)
            {
                if (piece != null)
                    Destroy(piece.gameObject);
            }
            _pieces.Clear();
            Clear(_notesGrid);
            Clear(_coinsGrid);

            var sorted = new List<int>(values);
            sorted.Sort((a, b) => b.CompareTo(a));
            foreach (int value in sorted)
            {
                bool coin = FinanceTheme.IsCoin(value);
                var slot = FinanceUi.Node(coin ? _coinsGrid : _notesGrid, "Slot_" + value);
                var shade = FinanceUi.Box(slot, "Shade", new Color(0f, 0f, 0f, 0.18f), coin ? 64f : 18f);
                shade.rectTransform.Stretch(6f, 6f, 6f, 6f);

                var piece = FinanceUi.Picture(slot, "Money_" + value, theme != null ? theme.GetMoneySprite(value) : null);
                piece.raycastTarget = true;
                piece.enabled = true;
                if (piece.sprite == null)
                {
                    piece.sprite = FinanceSprites.Rounded;
                    piece.type = Image.Type.Sliced;
                    piece.color = FinanceUi.Gold;
                    FinanceUi.Label(piece.transform, "Value", value.ToString(), 44, FinanceUi.Ink).rectTransform.Stretch();
                }

                piece.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, coin ? new Vector2(128f, 128f) : new Vector2(280f, 112f));
                var draggable = piece.gameObject.AddComponent<FinanceDraggable>();
                draggable.Setup(value, slot, DragLayer, this);
                draggable.Dropped = OnDropped;
                draggable.Hovered = OnHovered;
                _pieces.Add(draggable);
            }
        }

        static void Clear(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        void OnHovered(FinanceDropTarget target)
        {
            for (int i = 0; i < 3; i++)
            {
                float scale = target != null && target.Id == i ? 1.06f : 1f;
                _envelopes[i].Rect.localScale = new Vector3(scale, scale, 1f);
            }
        }

        bool OnDropped(FinanceDraggable piece, FinanceDropTarget target)
        {
            if (!_playing)
                return false;

            Envelope envelope = _envelopes[target.Id];
            Level level = Current;

            if (level.SaveFirst && envelope.Id != Savings && !_envelopes[Savings].Full)
            {
                Reject(envelope, "Сначала заплати себе!", "Наполни копилку, а потом распределяй остальное");
                return false;
            }

            if (envelope.Sum + piece.Value > envelope.Target)
            {
                string details = level.RevealTargets
                    ? "В «" + Capitalize(envelope.Title) + "» нужно ещё " + FinanceUi.Rub(envelope.Target - envelope.Sum)
                    : "Посчитай: " + envelope.Label + " от " + FinanceUi.Rub(level.Income);
                Reject(envelope, piece.Value + " руб — слишком много!", details);
                return false;
            }

            envelope.Sum += piece.Value;
            envelope.Pieces.Push(piece);
            piece.SnapHome();
            piece.gameObject.SetActive(false);

            FinanceAudio.Money(piece.Value);
            FinanceTween.Punch(this, envelope.Rect, 0.08f, 0.22f);
            Float("+" + piece.Value, Color.white, envelope.Rect, new Vector2(0f, 120f));
            RefreshEnvelope(envelope);

            if (envelope.Full)
                FinanceTween.Pop(this, envelope.Check.transform, 0.3f, 0.35f);

            if (_envelopes[Needs].Full && _envelopes[Savings].Full && _envelopes[Wants].Full)
                CompleteLevel();
            else
                CheckDeadlock();

            return true;
        }

        void Reject(Envelope envelope, string title, string details)
        {
            _mistakes++;
            FinanceAudio.Wrong();
            FinanceTween.Shake(this, envelope.Rect, envelope.Rest);
            Screens.Toast(title, details, FinanceUi.Coral);
        }

        void UndoLast(int id)
        {
            if (!_playing)
                return;

            Envelope envelope = _envelopes[id];
            if (envelope.Pieces.Count == 0)
                return;

            FinanceDraggable piece = envelope.Pieces.Pop();
            envelope.Sum -= piece.Value;
            piece.gameObject.SetActive(true);
            piece.SnapHome();
            FinanceTween.Pop(this, piece.transform, 0.5f, 0.25f);
            FinanceAudio.Money(piece.Value);
            RefreshEnvelope(envelope);
        }

        void CheckDeadlock()
        {
            Level level = Current;
            foreach (FinanceDraggable piece in _pieces)
            {
                if (piece == null || !piece.gameObject.activeSelf)
                    continue;

                foreach (Envelope envelope in _envelopes)
                {
                    bool blocked = level.SaveFirst && envelope.Id != Savings && !_envelopes[Savings].Full;
                    if (!blocked && envelope.Sum + piece.Value <= envelope.Target)
                        return;
                }
            }

            Screens.Toast("Тупик!", "Эти деньги никуда не помещаются. Нажми на конверт и верни купюры", FinanceUi.Sky, 2.6f);
        }

        void RefreshEnvelope(Envelope envelope)
        {
            envelope.SumText.text = FinanceUi.Rub(envelope.Sum);
            envelope.TargetText.text = "цель: " + envelope.Label;
            envelope.Bar.Set(envelope.Target > 0 ? (float)envelope.Sum / envelope.Target : 0f);
            envelope.Check.gameObject.SetActive(envelope.Full);
        }

        void CompleteLevel()
        {
            _playing = false;
            FinanceAudio.Good();
            MiniGamePayout.GrantForActiveScene();

            Level level = Current;
            int stars = StarsByTime(_time, level.ThreeStars, level.TwoStars);
            if (_mistakes >= 3)
                stars = Mathf.Max(1, stars - 1);
            _totalStars += stars;

            string body = "Нужное: " + FinanceUi.Rub(level.Targets[Needs])
                          + "\nКопилка: " + FinanceUi.Rub(level.Targets[Savings])
                          + "\nЖелания: " + FinanceUi.Rub(level.Targets[Wants])
                          + "\nВремя: " + Mathf.FloorToInt(_time) + " сек";

            bool last = _levelIndex >= Levels.Length - 1;
            var result = new ResultInfo
            {
                Title = "Бюджет сошёлся!",
                TitleColor = FinanceUi.Leaf,
                Stars = stars,
                Body = body,
                Lesson = level.Lesson
            };
            result.Buttons.Add(RetryButton(() => StartLevel(_levelIndex)));
            result.Buttons.Add(NextButton(() =>
            {
                if (last)
                    ShowFinal();
                else
                    StartLevel(_levelIndex + 1);
            }));

            FinanceTween.Delay(this, 0.5f, () => Screens.ShowResult(result));
        }

        void ShowFinal()
        {
            FinanceAudio.Victory();
            var result = new ResultInfo
            {
                Title = "Мастер бюджета!",
                TitleColor = FinanceUi.Leaf,
                Stars = Mathf.RoundToInt(_totalStars / (float)Levels.Length),
                Body = "Звёзд собрано: " + _totalStars + " из " + Levels.Length * 3
                       + "\nТы распределил " + FinanceUi.Rub(TotalIncome()) + " без единой лишней траты.",
                Lesson = "Получил деньги — сначала отложи в копилку, потом оплати нужное, а на желания трать остаток."
            };
            result.Buttons.Add(RetryButton(() =>
            {
                _totalStars = 0;
                StartLevel(0);
            }));
            result.Buttons.Add(ExitButton());
            Screens.ShowResult(result);
        }

        static int TotalIncome()
        {
            int total = 0;
            foreach (Level level in Levels)
                total += level.Income;
            return total;
        }

        static string Capitalize(string title)
        {
            return title.Substring(0, 1) + title.Substring(1).ToLowerInvariant();
        }
    }
}
