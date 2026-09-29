using System.Collections.Generic;
using LCT.MiniGames.Finance;
using UnityEngine;
using UnityEngine.UI;

namespace LCT.MiniGames.DreamGoal
{
    public class DreamGoalGame : FinanceGame
    {
        const int Allowance = 200;
        const int Step = 50;
        const int MaxMood = 10;
        const int StartMood = 6;
        const float InterestRate = 0.1f;

        [Header("Мечты")]
        [SerializeField] Sprite ballSprite;
        [SerializeField] Sprite robotSprite;
        [SerializeField] Sprite carSprite;

        class Level
        {
            public string Dream;
            public string Bought;
            public int Price;
            public int Weeks;
            public bool Bank;
            public string Lesson;
        }

        class Choice
        {
            public string Text;
            public int Piggy;
            public int Mood;
            public string Feedback;
        }

        class GameEvent
        {
            public string Title;
            public string Text;
            public int RequiresPiggy;
            public Choice A;
            public Choice B;
        }

        static readonly Level[] Levels =
        {
            new Level
            {
                Dream = "Футбольный мяч",
                Bought = "куплен",
                Price = 500,
                Weeks = 6,
                Lesson = "Регулярность — главный секрет: откладывай понемногу каждую неделю, и мечта станет реальной."
            },
            new Level
            {
                Dream = "Робопёс",
                Bought = "куплен",
                Price = 1000,
                Weeks = 8,
                Bank = true,
                Lesson = "Вклад приносит проценты: банк платит тебе за то, что деньги лежат у него. И их труднее потратить случайно."
            },
            new Level
            {
                Dream = "Машинка на пульте",
                Bought = "куплена",
                Price = 1500,
                Weeks = 10,
                Bank = true,
                Lesson = "Сложный процент: проценты начисляются и на проценты. Чем раньше начнёшь копить, тем быстрее растёт сумма."
            }
        };

        static readonly GameEvent[] Events =
        {
            new GameEvent
            {
                Title = "Распродажа игрушек!",
                Text = "Всё со скидкой. Можно взять 150 руб из копилки…",
                RequiresPiggy = 150,
                A = new Choice { Text = "Потратить 150", Piggy = -150, Mood = 2, Feedback = "Копилку легко открыть ради соблазна. Деньги на вкладе так просто не потратишь!" },
                B = new Choice { Text = "Не сейчас", Mood = -1, Feedback = "Ты удержался — мечта стала ближе!" }
            },
            new GameEvent
            {
                Title = "Бабушкин подарок",
                Text = "Бабушка подарила 200 руб! Что с ними сделать?",
                A = new Choice { Text = "В копилку", Piggy = 200, Feedback = "Подарки — отличный способ ускорить накопление." },
                B = new Choice { Text = "На радости", Mood = 2, Feedback = "Тоже можно, но мечта отодвинулась." }
            },
            new GameEvent
            {
                Title = "Аквапарк с друзьями",
                Text = "Билет стоит 100 руб из копилки.",
                RequiresPiggy = 100,
                A = new Choice { Text = "Пойти", Piggy = -100, Mood = 2, Feedback = "Весело! Но из копилки ушли деньги." },
                B = new Choice { Text = "Гулять в парке", Mood = 1, Feedback = "Можно веселиться и без трат!" }
            },
            new GameEvent
            {
                Title = "Подработка",
                Text = "Сосед просит помочь убрать гараж и заплатит 100 руб.",
                A = new Choice { Text = "Помочь", Piggy = 100, Mood = -1, Feedback = "Заработанные деньги — самые приятные." },
                B = new Choice { Text = "Отдохнуть", Mood = 1, Feedback = "Отдых тоже важен." }
            },
            new GameEvent
            {
                Title = "Сломался велосипед",
                Text = "Ремонт стоит 100 руб.",
                RequiresPiggy = 100,
                A = new Choice { Text = "Починить", Piggy = -100, Feedback = "Хорошо, что есть сбережения: они спасают в трудную минуту." },
                B = new Choice { Text = "Ходить пешком", Mood = -1, Feedback = "Иногда можно подождать с ремонтом." }
            }
        };

        readonly List<int> _usedEvents = new List<int>();

        Text _levelNumber;
        Text _weekText;
        Image _dreamIcon;
        Text _dreamTitle;
        Text _progressText;
        FinanceBar _progressBar;
        Text _forecastText;

        RectTransform _piggyCard;
        Text _piggyText;
        RectTransform _bankCard;
        Text _bankText;
        Text _bankRate;
        GameObject _bankLock;

        Text _moodText;
        FinanceBar _moodBar;
        RectTransform _moodCard;

        RectTransform _panel;
        Text _saveText;
        Text _spendText;
        Button _minus;
        Button _plus;
        Button _toPiggy;
        Button _toBank;

        int _levelIndex;
        int _week;
        int _piggy;
        int _bank;
        int _interestEarned;
        int _mood;
        int _save;
        int _totalStars;
        bool _busy;

        Level Current => Levels[_levelIndex];
        int Total => _piggy + _bank;

        protected override void BuildGame()
        {
            BuildHeader();
            BuildStorage();
            BuildPanel();
        }

        protected override void ShowIntro()
        {
            Screens.ShowIntro(new IntroInfo
            {
                Topic = TopicSavings,
                Title = "Путь к мечте",
                Art = ballSprite,
                Body = "Каждую неделю ты получаешь 200 руб. Реши, сколько отложить, чтобы успеть накопить на мечту.",
                Steps = new[]
                {
                    "Выбери, сколько отложить в эту неделю",
                    "Положи деньги в копилку или на вклад",
                    "Не забывай про радости — следи за настроением"
                },
                OnPlay = () =>
                {
                    _totalStars = 0;
                    StartLevel(MiniGameProgress.ResumeIndex(Levels.Length));
                }
            });
        }

        void BuildHeader()
        {
            var header = FinanceUi.Box(GameLayer, "Header", FinanceUi.Forest, 40f, true);
            header.rectTransform.TopBand(30f, 150f, 40f);
            _levelNumber = FinanceUi.Label(header.transform, "LevelNumber", "", 34, FinanceUi.Gold, TextAnchor.UpperLeft);
            _levelNumber.rectTransform.Stretch(40f, 20f, 40f, 90f);
            _weekText = FinanceUi.Label(header.transform, "Week", "", 56, Color.white, TextAnchor.LowerLeft, FontWeight.Heavy);
            _weekText.rectTransform.Stretch(40f, 56f, 40f, 18f);

            var dream = FinanceUi.Box(GameLayer, "Dream", FinanceUi.Cream, 40f, true);
            dream.rectTransform.TopBand(200f, 270f, 40f);
            var disc = FinanceUi.Dot(dream.transform, "Disc", FinanceUi.Mint, 180f);
            disc.rectTransform.Place(new Vector2(0f, 1f), new Vector2(125f, -115f), new Vector2(180f, 180f));
            _dreamIcon = FinanceUi.Picture(disc.transform, "Image", null);
            _dreamIcon.rectTransform.Stretch(26f, 26f, 26f, 26f);

            _dreamTitle = FinanceUi.Label(dream.transform, "Title", "", 48, FinanceUi.Ink, TextAnchor.MiddleLeft, FontWeight.Heavy);
            _dreamTitle.rectTransform.TopBand(24f, 64f, 0f);
            _dreamTitle.rectTransform.offsetMin = new Vector2(240f, _dreamTitle.rectTransform.offsetMin.y);
            _dreamTitle.rectTransform.offsetMax = new Vector2(-30f, _dreamTitle.rectTransform.offsetMax.y);

            _progressBar = FinanceBar.Create(dream.transform, "Bar", new Color(0f, 0f, 0f, 0.1f), FinanceUi.Leaf, 18f);
            _progressBar.Root.anchorMin = _progressBar.Root.anchorMax = new Vector2(0f, 1f);
            _progressBar.Root.pivot = new Vector2(0f, 1f);
            _progressBar.Root.anchoredPosition = new Vector2(240f, -100f);
            _progressBar.Root.sizeDelta = new Vector2(690f, 36f);

            _progressText = FinanceUi.Label(dream.transform, "Progress", "", 38, FinanceUi.Muted, TextAnchor.MiddleLeft, FontWeight.Bold);
            _progressText.rectTransform.anchorMin = _progressText.rectTransform.anchorMax = new Vector2(0f, 1f);
            _progressText.rectTransform.pivot = new Vector2(0f, 1f);
            _progressText.rectTransform.anchoredPosition = new Vector2(240f, -146f);
            _progressText.rectTransform.sizeDelta = new Vector2(690f, 50f);

            _forecastText = FinanceUi.Label(dream.transform, "Forecast", "", 32, FinanceUi.Sky, TextAnchor.MiddleCenter, FontWeight.Bold);
            _forecastText.rectTransform.BottomBand(12f, 56f, 30f);
        }

        void BuildStorage()
        {
            var piggy = FinanceUi.Box(GameLayer, "PiggyCard", new Color(1f, 0.9f, 0.93f, 1f), 40f, true);
            _piggyCard = piggy.rectTransform;
            _piggyCard.Place(new Vector2(0.5f, 1f), new Vector2(-250f, -680f), new Vector2(480f, 380f));
            FinanceUi.Label(piggy.transform, "Title", "КОПИЛКА", 40, FinancePiggy.Shade, TextAnchor.MiddleCenter, FontWeight.Heavy)
                .rectTransform.TopBand(18f, 56f, 20f);
            var piggyArt = FinanceUi.Node(piggy.transform, "Art").TopBand(80f, 170f, 0f);
            FinancePiggy.Build(piggyArt, "Piggy", 190f).Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(190f, 160f));
            _piggyText = FinanceUi.Label(piggy.transform, "Amount", "", 56, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _piggyText.rectTransform.TopBand(252f, 70f, 20f);
            FinanceUi.Label(piggy.transform, "Note", "без процентов", 30, FinanceUi.Muted, TextAnchor.MiddleCenter, FontWeight.Regular)
                .rectTransform.TopBand(320f, 44f, 20f);

            var bank = FinanceUi.Box(GameLayer, "BankCard", new Color(0.87f, 0.93f, 1f, 1f), 40f, true);
            _bankCard = bank.rectTransform;
            _bankCard.Place(new Vector2(0.5f, 1f), new Vector2(250f, -680f), new Vector2(480f, 380f));
            FinanceUi.Label(bank.transform, "Title", "ВКЛАД В БАНКЕ", 40, FinanceUi.Sky, TextAnchor.MiddleCenter, FontWeight.Heavy)
                .rectTransform.TopBand(18f, 56f, 20f);
            var coin = FinanceUi.Dot(bank.transform, "Icon", FinanceUi.Sky, 150f);
            coin.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -165f), new Vector2(150f, 150f));
            FinanceUi.Label(coin.transform, "Text", "%", 96, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy).rectTransform.Stretch();
            _bankText = FinanceUi.Label(bank.transform, "Amount", "", 56, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _bankText.rectTransform.TopBand(252f, 70f, 20f);
            _bankRate = FinanceUi.Label(bank.transform, "Note", "+10% каждую неделю", 30, FinanceUi.Leaf, TextAnchor.MiddleCenter, FontWeight.Bold);
            _bankRate.rectTransform.TopBand(320f, 44f, 20f);

            var locker = FinanceUi.Box(bank.transform, "Lock", new Color(0.12f, 0.2f, 0.3f, 0.82f), 40f);
            locker.rectTransform.Stretch();
            FinanceUi.Label(locker.transform, "Text", "Откроется\nна уровне 2", 44, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy)
                .rectTransform.Stretch(20f, 20f, 20f, 20f);
            _bankLock = locker.gameObject;

            var mood = FinanceUi.Box(GameLayer, "Mood", FinanceUi.Cream, 36f, true);
            _moodCard = mood.rectTransform;
            _moodCard.TopBand(900f, 120f, 40f);
            FinanceUi.Label(mood.transform, "Caption", "НАСТРОЕНИЕ", 34, FinanceUi.Muted, TextAnchor.MiddleLeft)
                .rectTransform.Stretch(36f, 0f, 700f, 0f);
            _moodBar = FinanceBar.Create(mood.transform, "Bar", new Color(0f, 0f, 0f, 0.1f), FinanceUi.Leaf, 18f);
            _moodBar.Root.Stretch(300f, 42f, 190f, 42f);
            _moodText = FinanceUi.Label(mood.transform, "Value", "", 46, FinanceUi.Ink, TextAnchor.MiddleRight, FontWeight.Heavy);
            _moodText.rectTransform.Stretch(800f, 0f, 36f, 0f);
        }

        void BuildPanel()
        {
            var panel = FinanceUi.Box(GameLayer, "Allowance", FinanceUi.Forest, 48f, true);
            _panel = panel.rectTransform;
            _panel.BottomBand(30f, 640f, 40f);

            FinanceUi.Label(panel.transform, "Title", "КАРМАННЫЕ ДЕНЬГИ: " + FinanceUi.Rub(Allowance).ToUpperInvariant(), 40, FinanceUi.Gold, TextAnchor.MiddleCenter, FontWeight.Heavy)
                .rectTransform.TopBand(26f, 60f, 30f);

            var stepper = FinanceUi.Node(panel.transform, "Stepper").TopBand(110f, 150f, 40f);
            _minus = FinanceUi.TextButton(stepper, "Minus", "−", FinanceUi.Coral, () => ChangeSave(-Step), 90, 40f);
            ((RectTransform)_minus.transform).Place(new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(150f, 150f));
            _plus = FinanceUi.TextButton(stepper, "Plus", "+", FinanceUi.Leaf, () => ChangeSave(Step), 90, 40f);
            ((RectTransform)_plus.transform).Place(new Vector2(1f, 0.5f), new Vector2(-80f, 0f), new Vector2(150f, 150f));
            var saveBox = FinanceUi.Box(stepper, "Save", FinanceUi.Cream, 36f);
            saveBox.rectTransform.Stretch(180f, 0f, 180f, 0f);
            FinanceUi.Label(saveBox.transform, "Caption", "ОТЛОЖИТЬ", 30, FinanceUi.Muted).rectTransform.TopBand(12f, 40f, 10f);
            _saveText = FinanceUi.Label(saveBox.transform, "Value", "", 66, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _saveText.rectTransform.Stretch(10f, 44f, 10f, 8f);

            _spendText = FinanceUi.Label(panel.transform, "Spend", "", 38, Color.white, TextAnchor.MiddleCenter, FontWeight.Bold);
            _spendText.rectTransform.TopBand(280f, 60f, 30f);

            var buttons = FinanceUi.Node(panel.transform, "Buttons").BottomBand(40f, 220f, 40f);
            _toPiggy = FinanceUi.TextButton(buttons, "ToPiggy", "В КОПИЛКУ", FinancePiggy.Shade, () => Deposit(false), 50);
            var piggyRect = (RectTransform)_toPiggy.transform;
            piggyRect.anchorMin = new Vector2(0f, 0f);
            piggyRect.anchorMax = new Vector2(0.5f, 1f);
            piggyRect.offsetMin = Vector2.zero;
            piggyRect.offsetMax = new Vector2(-15f, 0f);

            _toBank = FinanceUi.TextButton(buttons, "ToBank", "НА ВКЛАД", FinanceUi.Sky, () => Deposit(true), 50);
            var bankRect = (RectTransform)_toBank.transform;
            bankRect.anchorMin = new Vector2(0.5f, 0f);
            bankRect.anchorMax = new Vector2(1f, 1f);
            bankRect.offsetMin = new Vector2(15f, 0f);
            bankRect.offsetMax = Vector2.zero;
        }

        void StartLevel(int index)
        {
            _levelIndex = index;
            _week = 1;
            _piggy = 0;
            _bank = 0;
            _interestEarned = 0;
            _mood = StartMood;
            _usedEvents.Clear();

            Level level = Current;
            _levelNumber.text = "УРОВЕНЬ " + (index + 1) + " ИЗ " + Levels.Length;
            _dreamTitle.text = level.Dream + " — " + FinanceUi.Rub(level.Price);
            _dreamIcon.sprite = DreamSprite(index);
            _dreamIcon.enabled = _dreamIcon.sprite != null;
            _bankLock.SetActive(!level.Bank);
            _toBank.interactable = level.Bank;

            if (level.Bank && index == 1)
                Screens.Toast("Открылся вклад!", "Банк добавляет 10% к деньгам на вкладе каждую неделю", FinanceUi.Sky, 3f);

            BeginWeek();
        }

        void BeginWeek()
        {
            _busy = false;
            if (_week > 1 && _bank > 0)
            {
                int interest = Mathf.RoundToInt(_bank * InterestRate);
                if (interest > 0)
                {
                    _bank += interest;
                    _interestEarned += interest;
                    Float("+" + interest + " проценты", FinanceUi.Leaf, _bankCard, new Vector2(0f, 120f), 52);
                    FinanceTween.Punch(this, _bankCard, 0.06f);
                    FinanceAudio.Coin();
                }
            }

            _weekText.text = "Неделя " + _week + " из " + Current.Weeks;
            _save = Mathf.Min(100, Allowance);
            Float("+" + FinanceUi.Rub(Allowance), FinanceUi.Gold, _panel, new Vector2(0f, 360f), 56);
            FinanceTween.Pop(this, _panel, 0.94f, 0.25f);
            SetPanelInteractable(true);
            Refresh();

            if (Total >= Current.Price)
                Win();
        }

        void ChangeSave(int delta)
        {
            if (_busy)
                return;

            _save = Mathf.Clamp(_save + delta, 0, Allowance);
            FinanceTween.Punch(this, _saveText.transform, 0.12f, 0.18f);
            FinanceAudio.Coin();
            Refresh();
        }

        void Deposit(bool toBank)
        {
            if (_busy)
                return;
            _busy = true;
            SetPanelInteractable(false);

            int spend = Allowance - _save;
            int moodDelta = MoodForSpend(spend);
            _mood = Mathf.Clamp(_mood + moodDelta, 0, MaxMood);

            if (_save > 0)
            {
                RectTransform card = toBank ? _bankCard : _piggyCard;
                if (toBank)
                    _bank += _save;
                else
                    _piggy += _save;
                FinanceAudio.Banknote();
                Float("+" + FinanceUi.Rub(_save), FinanceUi.Leaf, card, new Vector2(0f, 120f));
                FinanceTween.Punch(this, card, 0.08f);
            }

            if (moodDelta != 0)
                Float("настроение " + (moodDelta > 0 ? "+" : "−") + Mathf.Abs(moodDelta), moodDelta > 0 ? FinanceUi.Leaf : FinanceUi.Coral, _moodCard, new Vector2(0f, 60f), 46);

            Refresh();

            if (Total >= Current.Price)
            {
                FinanceTween.Delay(this, 0.6f, Win);
                return;
            }

            FinanceTween.Delay(this, 0.7f, () =>
            {
                if (!TryShowEvent())
                    EndWeek();
            });
        }

        bool TryShowEvent()
        {
            if (_week < 2 || Random.value > 0.45f)
                return false;

            var candidates = new List<int>();
            for (int i = 0; i < Events.Length; i++)
            {
                if (!_usedEvents.Contains(i) && _piggy >= Events[i].RequiresPiggy)
                    candidates.Add(i);
            }

            if (candidates.Count == 0)
                return false;

            int index = candidates[Random.Range(0, candidates.Count)];
            _usedEvents.Add(index);
            GameEvent gameEvent = Events[index];

            var info = new ResultInfo
            {
                Title = gameEvent.Title,
                TitleColor = FinanceUi.Violet,
                Body = gameEvent.Text + "\n\n" + Describe(gameEvent.A) + "\n" + Describe(gameEvent.B)
            };
            info.Buttons.Add(new ResultButton { Text = gameEvent.A.Text, Color = FinanceUi.Orange, Size = new Vector2(400f, 150f), OnClick = () => ApplyChoice(gameEvent.A) });
            info.Buttons.Add(new ResultButton { Text = gameEvent.B.Text, Color = FinanceUi.Sky, Size = new Vector2(400f, 150f), OnClick = () => ApplyChoice(gameEvent.B) });
            Screens.ShowResult(info);
            return true;
        }

        static string Describe(Choice choice)
        {
            var parts = new List<string>();
            if (choice.Piggy != 0)
                parts.Add("копилка " + FinanceUi.SignedRub(choice.Piggy));
            if (choice.Mood != 0)
                parts.Add("настроение " + (choice.Mood > 0 ? "+" : "−") + Mathf.Abs(choice.Mood));
            if (parts.Count == 0)
                parts.Add("без изменений");
            return "«" + choice.Text + "»: " + string.Join(", ", parts);
        }

        void ApplyChoice(Choice choice)
        {
            _piggy = Mathf.Max(0, _piggy + choice.Piggy);
            _mood = Mathf.Clamp(_mood + choice.Mood, 0, MaxMood);
            if (choice.Piggy != 0)
            {
                Float(FinanceUi.SignedRub(choice.Piggy), choice.Piggy > 0 ? FinanceUi.Leaf : FinanceUi.Coral, _piggyCard, new Vector2(0f, 120f));
                if (choice.Piggy > 0)
                    FinanceAudio.Banknote();
                else
                    FinanceAudio.Wrong();
            }

            Screens.Toast(choice.Text, choice.Feedback, choice.Piggy < 0 ? FinanceUi.Coral : FinanceUi.Leaf, 2.4f);
            Refresh();

            if (Total >= Current.Price)
                Win();
            else
                EndWeek();
        }

        void EndWeek()
        {
            if (_mood <= 0)
            {
                ShowFail("Настроение на нуле!",
                    "Ты так экономил, что совсем перестал радоваться.",
                    "Копить важно, но не в ущерб себе. Оставляй немного денег на радости.");
                return;
            }

            _week++;
            if (_week > Current.Weeks)
            {
                ShowFail("Не успел накопить",
                    "Накоплено " + FinanceUi.Rub(Total) + " из " + FinanceUi.Rub(Current.Price)
                    + "\nНе хватило " + FinanceUi.Rub(Current.Price - Total),
                    "Смотри на прогноз: он подскажет, сколько откладывать каждую неделю, чтобы успеть.");
                return;
            }

            BeginWeek();
        }

        void Win()
        {
            _busy = true;
            SetPanelInteractable(false);
            FinanceAudio.Good();
            MiniGamePayout.GrantForActiveScene();
            MiniGameProgress.AdvanceTo(_levelIndex + 1, Levels.Length);

            Level level = Current;
            int weeksLeft = level.Weeks - _week;
            int stars = 1 + (_mood >= 5 ? 1 : 0) + (weeksLeft >= 1 ? 1 : 0);
            _totalStars += stars;
            bool last = _levelIndex >= Levels.Length - 1;

            string body = level.Dream + " " + level.Bought + " за " + _week + " нед.!"
                          + "\nНастроение: " + _mood + " из " + MaxMood;
            if (_interestEarned > 0)
                body += "\nБанк добавил процентов: " + FinanceUi.Rub(_interestEarned);

            var result = new ResultInfo
            {
                Title = "Мечта сбылась!",
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
            Screens.ShowResult(result);
        }

        void ShowFail(string title, string body, string lesson)
        {
            FinanceAudio.Wrong();
            SetPanelInteractable(false);
            var result = new ResultInfo
            {
                Title = title,
                TitleColor = FinanceUi.Coral,
                Body = body,
                Lesson = lesson
            };
            result.Buttons.Add(RetryButton(() => StartLevel(_levelIndex)));
            result.Buttons.Add(ExitButton());
            Screens.ShowResult(result);
        }

        void ShowFinal()
        {
            FinanceAudio.Victory();
            var result = new ResultInfo
            {
                Title = "Мастер накоплений!",
                TitleColor = FinanceUi.Leaf,
                Stars = Mathf.RoundToInt(_totalStars / (float)Levels.Length),
                Body = "Звёзд собрано: " + _totalStars + " из " + Levels.Length * 3,
                Lesson = "Цель + регулярность + проценты = мечта. И не забывай оставлять немного на радости."
            };
            result.Buttons.Add(RetryButton(() =>
            {
                _totalStars = 0;
                MiniGameProgress.ResetActive();
                StartLevel(0);
            }));
            result.Buttons.Add(ExitButton());
            Screens.ShowResult(result);
        }

        static int MoodForSpend(int spend)
        {
            if (spend >= 150)
                return 1;
            if (spend >= 100)
                return 0;
            return spend >= 50 ? -1 : -2;
        }

        void Refresh()
        {
            Level level = Current;
            _piggyText.text = FinanceUi.Rub(_piggy);
            _bankText.text = level.Bank ? FinanceUi.Rub(_bank) : "—";
            _progressText.text = "Накоплено " + FinanceUi.Rub(Total) + " из " + FinanceUi.Rub(level.Price);
            _progressBar.Animate(this, Total / (float)level.Price, 0.3f);

            _moodText.text = _mood + " / " + MaxMood;
            _moodBar.Animate(this, _mood / (float)MaxMood, 0.3f);
            _moodBar.SetColor(_mood >= 6 ? FinanceUi.Leaf : _mood >= 3 ? FinanceUi.Gold : FinanceUi.Coral);

            _saveText.text = FinanceUi.Rub(_save);
            int spend = Allowance - _save;
            int moodDelta = MoodForSpend(spend);
            string moodPart = moodDelta == 0 ? "настроение не изменится" : "настроение " + (moodDelta > 0 ? "+" : "−") + Mathf.Abs(moodDelta);
            _spendText.text = "На радости: " + FinanceUi.Rub(spend) + "  ·  " + moodPart;

            if (!_busy)
            {
                _minus.interactable = _save > 0;
                _plus.interactable = _save < Allowance;
            }

            _forecastText.text = Forecast();
        }

        string Forecast()
        {
            Level level = Current;
            int weeksLeft = level.Weeks - _week + 1;
            if (Total >= level.Price)
                return "Цель достигнута!";
            if (_save <= 0)
                return FinanceUi.Paint("Если ничего не откладывать, мечта не приблизится", FinanceUi.Coral);

            int piggyWeeks = WeeksNeeded(false);
            string text = "Прогноз: копилка — " + WeeksText(piggyWeeks, weeksLeft);
            if (level.Bank)
                text += ", вклад — " + WeeksText(WeeksNeeded(true), weeksLeft);
            return text;
        }

        static string WeeksText(int weeks, int weeksLeft)
        {
            string text = weeks >= 99 ? "не успеть" : weeks + " нед.";
            return FinanceUi.Paint(text, weeks <= weeksLeft ? FinanceUi.Leaf : FinanceUi.Coral);
        }

        int WeeksNeeded(bool bank)
        {
            float piggy = _piggy;
            float deposit = _bank;
            for (int week = 1; week < 99; week++)
            {
                if (week > 1)
                    deposit += Mathf.Round(deposit * InterestRate);
                if (bank)
                    deposit += _save;
                else
                    piggy += _save;
                if (piggy + deposit >= Current.Price)
                    return week;
            }

            return 99;
        }

        void SetPanelInteractable(bool value)
        {
            _minus.interactable = value && _save > 0;
            _plus.interactable = value && _save < Allowance;
            _toPiggy.interactable = value;
            _toBank.interactable = value && Current.Bank;
        }

        Sprite DreamSprite(int index)
        {
            switch (index)
            {
                case 0: return ballSprite;
                case 1: return robotSprite;
                default: return carSprite;
            }
        }
    }
}
