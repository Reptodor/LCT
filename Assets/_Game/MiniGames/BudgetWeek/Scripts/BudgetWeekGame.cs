using System;
using System.Collections.Generic;
using LCT.MiniGames.Finance;
using UnityEngine;
using UnityEngine.UI;

namespace LCT.MiniGames.BudgetWeek
{
    public class BudgetWeekGame : FinanceGame
    {
        const int MaxMood = 10;
        const int StartMood = 6;
        const string Surprise = "surprise";

        [Serializable]
        public class IconEntry
        {
            public string key;
            public Sprite sprite;
        }

        [Header("Иконки карточек")]
        [SerializeField] List<IconEntry> icons = new List<IconEntry>();

        class Option
        {
            public string Title;
            public int Money;
            public int Mood;
            public string Feedback;
        }

        class Card
        {
            public string Icon;
            public string Title;
            public string Text;
            public Option A;
            public Option B;
            public bool IsEvent => B == null;
        }

        class EventSlot
        {
            public int AfterDay;
            public Card Card;
        }

        class Week
        {
            public string Name;
            public int Budget;
            public int Goal;
            public Card[] Days;
            public EventSlot[] Events;
            public string Lesson;
        }

        class Entry
        {
            public int Day;
            public Card Card;
        }

        class OptionView
        {
            public Button Button;
            public Image Back;
            public Text Title;
            public Text Effect;
            public Color Color;
        }

        static readonly string[] DayNames = { "ПОНЕДЕЛЬНИК", "ВТОРНИК", "СРЕДА", "ЧЕТВЕРГ", "ПЯТНИЦА", "СУББОТА", "ВОСКРЕСЕНЬЕ" };
        static readonly string[] DayShort = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

        static readonly Week[] Weeks =
        {
            new Week
            {
                Name = "Неделя желаний",
                Budget = 700,
                Goal = 150,
                Lesson = "Не обязательно отказываться от всего. Выбери одну-две радости, которые важнее всего, а на остальном экономь.",
                Days = new[]
                {
                    Day("burger", "Обед", "Большая перемена, очень хочется есть.",
                        Opt("Столовая", -120, 1, "Горячий обед — нужная трата. Главное, чтобы она была в плане."),
                        Opt("Бутерброды из дома", -30, 0, "Еда из дома экономит деньги каждый день.")),
                    Day("cola", "Кино", "Друзья зовут в кино на новый мультфильм.",
                        Opt("Кино с попкорном", -300, 3, "Развлечения — это нормально, если на них заложены деньги."),
                        Opt("Смотреть дома с друзьями", -50, 1, "Почти та же радость, а денег ушло в 6 раз меньше.")),
                    Day("toy", "Робопёс со скидкой", "В витрине — игрушка мечты. Скидка только сегодня!",
                        Opt("Купить сейчас", -400, 2, "Импульсная покупка: радость быстро проходит, а деньги закончились."),
                        Opt("Подумать до завтра", 0, -1, "Правило 24 часов: если завтра всё ещё хочется — внеси покупку в план.")),
                    Day("juice", "Жара", "После уроков очень хочется пить.",
                        Opt("Сок из автомата", -80, 1, "Мелкие траты незаметно съедают бюджет."),
                        Opt("Вода из дома", 0, 0, "Бутылка воды из дома — ноль рублей.")),
                    Day("cake", "День рождения друга", "Нужно подарить подарок.",
                        Opt("Дорогой подарок", -350, 2, "Дорого — не значит лучше. Друг обрадовался бы и открытке."),
                        Opt("Подарок своими руками", -60, 2, "Внимание дороже денег: и друг рад, и бюджет цел.")),
                    Day("chocolate", "Акция «3 по цене 2»", "Шоколадки по акции. Выгодно?",
                        Opt("Взять три", -150, 1, "Акция выгодна, только если тебе правда нужно столько."),
                        Opt("Взять одну", -50, 1, "Та же радость, а денег ушло втрое меньше.")),
                    Day("icecream", "Прогулка в парке", "Воскресенье, вся семья идёт гулять.",
                        Opt("Кафе в парке", -250, 2, "Отдых в кафе — приятно, но дорого."),
                        Opt("Пикник с едой из дома", -40, 1, "Отдых не обязан быть дорогим."))
                },
                Events = new[]
                {
                    new EventSlot
                    {
                        AfterDay = 2,
                        Card = Day("gift", "Бабушка дала денег", "Неожиданный подарок — 150 руб!",
                            Opt("Спасибо, бабушка!", 150, 1, "Неожиданные деньги лучше не тратить сразу, а внести в план."), null)
                    },
                    new EventSlot
                    {
                        AfterDay = 4,
                        Card = Day(Surprise, "Порвался рюкзак", "Срочно нужен ремонт — 150 руб.",
                            Opt("Оплатить ремонт", -150, 0, "Непредвиденные траты бывают у всех. Поэтому нужен запас."), null)
                    }
                }
            },
            new Week
            {
                Name = "Обязательные траты",
                Budget = 1000,
                Goal = 200,
                Lesson = "Сначала заложи в бюджет обязательное — проезд, школу, еду. И сравнивай цены: одно и то же стоит по-разному.",
                Days = new[]
                {
                    Day("bus", "Проезд", "Всю неделю нужно ездить в школу.",
                        Opt("Проездной на неделю", -150, 0, "Проездной выгоднее: оптом дешевле."),
                        Opt("Разовые билеты", -210, 0, "Разовые билеты вышли дороже. Считай заранее!")),
                    Day("notebook", "Тетради", "Учитель попросил купить тетради.",
                        Opt("С любимыми героями", -200, 1, "Нужная вещь, но за «красоту» пришлось переплатить."),
                        Opt("Обычные", -80, 0, "Обязательную покупку можно сделать дешевле.")),
                    Day("apple", "Фрукты домой", "Мама просит купить яблоки.",
                        Opt("На рынке", -60, 0, "На рынке дешевле. Сравнивать цены — полезная привычка."),
                        Opt("В магазине у дома", -90, 0, "Удобно, но дороже. Сравнивай цены!")),
                    Day("pizza", "Семейный вечер", "Решили устроить вечер кино.",
                        Opt("Заказать пиццу", -150, 2, "Вкусно, но готовая еда стоит дороже."),
                        Opt("Приготовить вместе", -50, 2, "Готовить вместе — весело и дешевле.")),
                    Day("ball", "Футбол во дворе", "Все играют в футбол, а мяча у тебя нет.",
                        Opt("Купить мяч", -300, 3, "Покупка для дела и надолго — хорошая трата, если она по карману."),
                        Opt("Играть мячом друга", 0, 0, "Иногда можно обойтись без покупки.")),
                    Day("burger", "Обед", "Большая перемена, очень хочется есть.",
                        Opt("Столовая", -120, 1, "Горячий обед — нужная трата."),
                        Opt("Бутерброды из дома", -30, 0, "Еда из дома экономит деньги каждый день.")),
                    Day("cola", "Кино", "Друзья зовут в кино.",
                        Opt("Кино с попкорном", -300, 3, "Развлечения — это нормально, если на них заложены деньги."),
                        Opt("Смотреть дома с друзьями", -50, 1, "Почти та же радость за меньшие деньги."))
                },
                Events = new[]
                {
                    new EventSlot
                    {
                        AfterDay = 1,
                        Card = Day(Surprise, "Потерялся проездной", "Придётся купить билет — 60 руб.",
                            Opt("Купить билет", -60, 0, "Береги вещи: потери тоже бьют по бюджету."), null)
                    },
                    new EventSlot
                    {
                        AfterDay = 3,
                        Card = Day("gift", "Помог соседу", "Сосед заплатил за помощь с уборкой — 100 руб.",
                            Opt("Отлично!", 100, 1, "Деньги можно не только тратить, но и зарабатывать."), null)
                    },
                    new EventSlot
                    {
                        AfterDay = 5,
                        Card = Day(Surprise, "Сломались наушники", "Новые стоят 120 руб.",
                            Opt("Купить новые", -120, 0, "Запас в бюджете спасает от неприятных сюрпризов."), null)
                    }
                }
            }
        };

        readonly Image[] _dayDots = new Image[7];
        readonly List<Entry> _entries = new List<Entry>();

        Text _weekNumber;
        Text _weekName;
        Text _moneyText;
        Text _goalText;
        Text _moodText;
        FinanceBar _moodBar;
        RectTransform _walletCard;
        RectTransform _moodCard;

        RectTransform _card;
        Vector2 _cardRest;
        Image _chip;
        Text _chipText;
        Image _icon;
        Image _surpriseDot;
        Text _title;
        Text _text;
        Button _continue;
        OptionView _optionA;
        OptionView _optionB;

        int _weekIndex;
        int _entryIndex;
        int _currentDay;
        int _money;
        int _mood;
        int _totalStars;
        bool _busy;

        Week Current => Weeks[_weekIndex];

        static Card Day(string icon, string title, string text, Option a, Option b)
        {
            return new Card { Icon = icon, Title = title, Text = text, A = a, B = b };
        }

        static Option Opt(string title, int money, int mood, string feedback)
        {
            return new Option { Title = title, Money = money, Mood = mood, Feedback = feedback };
        }

        protected override void BuildGame()
        {
            BuildHeader();
            BuildStats();
            BuildCard();
            _optionA = BuildOption("OptionA", FinanceUi.Orange, 290f, () => Choose(true));
            _optionB = BuildOption("OptionB", FinanceUi.Sky, 50f, () => Choose(false));
        }

        protected override void ShowIntro()
        {
            Screens.ShowIntro(new IntroInfo
            {
                Topic = TopicBudget,
                Title = "Неделя на бюджете",
                Art = theme != null ? theme.moneyPile : null,
                Body = "У тебя есть деньги на неделю. Каждый день — выбор. Доживи до воскресенья с запасом и хорошим настроением!",
                Steps = new[]
                {
                    "Выбирай один из двух вариантов",
                    "Сохрани нужную сумму к концу недели",
                    "Не дай настроению упасть до нуля"
                },
                OnPlay = () =>
                {
                    _totalStars = 0;
                    StartWeek(0);
                }
            });
        }

        void BuildHeader()
        {
            var header = FinanceUi.Box(GameLayer, "Header", FinanceUi.Forest, 40f, true);
            header.rectTransform.TopBand(30f, 150f, 40f);
            _weekNumber = FinanceUi.Label(header.transform, "WeekNumber", "", 34, FinanceUi.Gold, TextAnchor.UpperLeft);
            _weekNumber.rectTransform.Stretch(40f, 20f, 40f, 90f);
            _weekName = FinanceUi.Label(header.transform, "WeekName", "", 56, Color.white, TextAnchor.LowerLeft, FontWeight.Heavy);
            _weekName.rectTransform.Stretch(40f, 56f, 40f, 18f);

            var days = FinanceUi.Node(GameLayer, "Days").TopBand(200f, 96f, 40f);
            for (int i = 0; i < 7; i++)
            {
                var dot = FinanceUi.Dot(days, "Day" + i, Color.white, 88f);
                dot.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2((i - 3) * 138f, 0f), new Vector2(88f, 88f));
                FinanceUi.Label(dot.transform, "Text", DayShort[i], 34, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Heavy)
                    .rectTransform.Stretch();
                _dayDots[i] = dot;
            }
        }

        void BuildStats()
        {
            var wallet = FinanceUi.Box(GameLayer, "Wallet", FinanceUi.Cream, 36f, true);
            _walletCard = wallet.rectTransform;
            _walletCard.Place(new Vector2(0.5f, 1f), new Vector2(-250f, -410f), new Vector2(480f, 190f));
            FinanceUi.Label(wallet.transform, "Caption", "КОШЕЛЁК", 32, FinanceUi.Muted).rectTransform.TopBand(16f, 44f, 20f);
            _moneyText = FinanceUi.Label(wallet.transform, "Money", "", 64, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _moneyText.rectTransform.TopBand(58f, 76f, 20f);
            _goalText = FinanceUi.Label(wallet.transform, "Goal", "", 30, FinanceUi.Leaf, TextAnchor.MiddleCenter, FontWeight.Bold);
            _goalText.rectTransform.TopBand(136f, 40f, 20f);

            var mood = FinanceUi.Box(GameLayer, "Mood", FinanceUi.Cream, 36f, true);
            _moodCard = mood.rectTransform;
            _moodCard.Place(new Vector2(0.5f, 1f), new Vector2(250f, -410f), new Vector2(480f, 190f));
            FinanceUi.Label(mood.transform, "Caption", "НАСТРОЕНИЕ", 32, FinanceUi.Muted).rectTransform.TopBand(16f, 44f, 20f);
            _moodText = FinanceUi.Label(mood.transform, "Value", "", 54, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _moodText.rectTransform.TopBand(58f, 70f, 20f);
            _moodBar = FinanceBar.Create(mood.transform, "Bar", new Color(0f, 0f, 0f, 0.1f), FinanceUi.Leaf, 16f);
            _moodBar.Root.BottomBand(24f, 32f, 40f);
        }

        void BuildCard()
        {
            var card = FinanceUi.Box(GameLayer, "Card", FinanceUi.Cream, 56f, true);
            _card = card.rectTransform;
            _card.anchorMin = new Vector2(0f, 1f);
            _card.anchorMax = new Vector2(1f, 1f);
            _card.pivot = new Vector2(0.5f, 1f);
            _cardRest = new Vector2(0f, -530f);
            _card.anchoredPosition = _cardRest;
            _card.sizeDelta = new Vector2(-80f, 800f);

            _chip = FinanceUi.Box(_card, "Chip", FinanceUi.Leaf, 32f);
            _chip.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(560f, 76f));
            _chipText = FinanceUi.Label(_chip.transform, "Text", "", 36, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _chipText.rectTransform.Stretch(16f, 4f, 16f, 4f);

            var iconHolder = FinanceUi.Node(_card, "Icon").TopBand(130f, 260f, 0f);
            var disc = FinanceUi.Dot(iconHolder, "Disc", FinanceUi.Mint, 260f);
            disc.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 260f));
            _icon = FinanceUi.Picture(disc.transform, "Image", null);
            _icon.rectTransform.Stretch(36f, 36f, 36f, 36f);
            _surpriseDot = FinanceUi.Dot(disc.transform, "Surprise", FinanceUi.Violet, 220f);
            _surpriseDot.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 220f));
            FinanceUi.Label(_surpriseDot.transform, "Mark", "!", 150, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy)
                .rectTransform.Stretch();

            _title = FinanceUi.Label(_card, "Title", "", 66, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _title.rectTransform.TopBand(410f, 100f, 40f);
            _text = FinanceUi.Label(_card, "Text", "", 44, FinanceUi.Muted, TextAnchor.UpperCenter, FontWeight.Regular);
            _text.rectTransform.TopBand(520f, 150f, 50f);

            _continue = FinanceUi.TextButton(_card, "Continue", "ДАЛЕЕ", FinanceUi.Leaf, Next, 52);
            ((RectTransform)_continue.transform).Place(new Vector2(0.5f, 0f), new Vector2(0f, 75f), new Vector2(420f, 120f));
            _continue.gameObject.SetActive(false);
        }

        OptionView BuildOption(string name, Color color, float bottom, UnityEngine.Events.UnityAction onClick)
        {
            var view = new OptionView { Color = color };
            view.Back = FinanceUi.Box(GameLayer, name, color, 44f, true);
            view.Back.raycastTarget = true;
            view.Back.rectTransform.BottomBand(bottom, 210f, 40f);
            view.Button = view.Back.gameObject.AddComponent<Button>();
            view.Button.targetGraphic = view.Back;
            view.Button.onClick.AddListener(onClick);
            view.Back.gameObject.AddComponent<FinancePressFx>();

            view.Title = FinanceUi.Label(view.Back.transform, "Title", "", 54, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy);
            view.Title.Outlined(new Color(0f, 0f, 0f, 0.2f), 2f);
            view.Title.rectTransform.Stretch(30f, 22f, 30f, 90f);
            view.Effect = FinanceUi.Label(view.Back.transform, "Effect", "", 40, new Color(1f, 1f, 1f, 0.95f), TextAnchor.MiddleCenter, FontWeight.Bold);
            view.Effect.rectTransform.Stretch(30f, 116f, 30f, 22f);
            return view;
        }

        void StartWeek(int index)
        {
            _weekIndex = index;
            Week week = Current;
            _money = week.Budget;
            _mood = StartMood;
            _entryIndex = 0;
            _currentDay = 0;

            _entries.Clear();
            for (int day = 0; day < week.Days.Length; day++)
            {
                _entries.Add(new Entry { Day = day, Card = week.Days[day] });
                foreach (EventSlot slot in week.Events)
                {
                    if (slot.AfterDay == day)
                        _entries.Add(new Entry { Day = day, Card = slot.Card });
                }
            }

            _weekNumber.text = "НЕДЕЛЯ " + (index + 1) + " ИЗ " + Weeks.Length;
            _weekName.text = week.Name;
            _goalText.text = "цель: оставить " + FinanceUi.Rub(week.Goal);
            RefreshStats();
            ShowEntry();
        }

        void ShowEntry()
        {
            Entry entry = _entries[_entryIndex];
            Card card = entry.Card;

            if (entry.Day != _currentDay)
            {
                _currentDay = entry.Day;
                ChangeMood(-1);
                Float("−1 будни", FinanceUi.Coral, _moodCard, new Vector2(0f, 60f), 44);
            }

            RefreshDays();

            _chip.color = card.IsEvent ? FinanceUi.Violet : FinanceUi.Leaf;
            _chipText.text = card.IsEvent ? "СЮРПРИЗ!" : DayNames[entry.Day];

            Sprite sprite = FindIcon(card.Icon);
            bool surprise = card.Icon == Surprise || sprite == null;
            _surpriseDot.gameObject.SetActive(surprise);
            _icon.sprite = sprite;
            _icon.enabled = !surprise;

            _title.text = card.Title;
            _text.text = card.Text;
            _text.color = FinanceUi.Muted;
            _continue.gameObject.SetActive(false);

            SetupOption(_optionA, card.A, card.IsEvent);
            SetupOption(_optionB, card.B, false);

            _card.anchoredPosition = _cardRest + new Vector2(1100f, 0f);
            FinanceTween.Move(this, _card, _cardRest, 0.3f);
            _busy = false;
        }

        void SetupOption(OptionView view, Option option, bool forced)
        {
            view.Back.gameObject.SetActive(option != null);
            if (option == null)
                return;

            bool affordable = forced || option.Money >= 0 || _money + option.Money >= 0;
            view.Button.interactable = affordable;
            view.Title.text = option.Title;
            view.Effect.text = affordable ? EffectText(option) : "не хватает денег";
            FinanceTween.Pop(this, view.Back.transform, 0.85f, 0.25f);
        }

        static string EffectText(Option option)
        {
            string money = option.Money == 0 ? "бесплатно" : FinanceUi.SignedRub(option.Money);
            if (option.Mood == 0)
                return money;
            return money + "  ·  настроение " + (option.Mood > 0 ? "+" : "−") + Mathf.Abs(option.Mood);
        }

        void Choose(bool first)
        {
            if (_busy)
                return;
            _busy = true;

            Entry entry = _entries[_entryIndex];
            Option option = first ? entry.Card.A : entry.Card.B;
            string feedback = option.Feedback;

            int delta = option.Money;
            if (_money + delta < 0)
            {
                delta = -_money;
                ChangeMood(-2);
                feedback = "Не хватило запаса — пришлось просить у родителей. Всегда оставляй немного на непредвиденное!";
            }

            _money += delta;
            ChangeMood(option.Mood);

            if (delta != 0)
            {
                FinanceAudio.Money(Mathf.Abs(delta) >= 50 ? 100 : 5);
                Float(FinanceUi.SignedRub(delta), delta > 0 ? FinanceUi.Leaf : FinanceUi.Coral, _walletCard, new Vector2(0f, 80f));
                FinanceTween.Punch(this, _walletCard, 0.06f);
            }

            if (option.Mood != 0)
            {
                Float((option.Mood > 0 ? "+" : "−") + Mathf.Abs(option.Mood), option.Mood > 0 ? FinanceUi.Leaf : FinanceUi.Coral, _moodCard, new Vector2(0f, 80f));
                FinanceTween.Punch(this, _moodCard, 0.06f);
            }

            RefreshStats();

            _optionA.Back.gameObject.SetActive(false);
            _optionB.Back.gameObject.SetActive(false);
            _text.text = feedback;
            _text.color = FinanceUi.Ink;
            _continue.gameObject.SetActive(true);
            FinanceTween.Pop(this, _continue.transform, 0.6f, 0.3f);
        }

        void Next()
        {
            _continue.gameObject.SetActive(false);

            if (_mood <= 0)
            {
                ShowFail("Настроение на нуле!",
                    "Ты так долго во всём себе отказывал, что устал от экономии.",
                    "Бюджет без радостей не работает. Оставляй немного денег на желания.");
                return;
            }

            _entryIndex++;
            if (_entryIndex >= _entries.Count)
            {
                FinishWeek();
                return;
            }

            ShowEntry();
        }

        void FinishWeek()
        {
            Week week = Current;
            if (_money < week.Goal)
            {
                ShowFail("Запас не собран",
                    "Осталось " + FinanceUi.Rub(_money) + ", а нужно было " + FinanceUi.Rub(week.Goal) + ".",
                    "Планируй траты заранее: сначала отложи запас, потом трать остальное.");
                return;
            }

            FinanceAudio.Good();
            MiniGamePayout.GrantForActiveScene();
            int stars = _mood >= 6 ? 3 : _mood >= 3 ? 2 : 1;
            _totalStars += stars;
            bool last = _weekIndex >= Weeks.Length - 1;

            var result = new ResultInfo
            {
                Title = "Неделя пройдена!",
                TitleColor = FinanceUi.Leaf,
                Stars = stars,
                Body = "Осталось: " + FinanceUi.Rub(_money) + " (цель " + FinanceUi.Rub(week.Goal) + ")\nНастроение: " + _mood + " из " + MaxMood
                       + (stars < 3 ? "\nДля 3 звёзд настроение должно быть 6+" : ""),
                Lesson = week.Lesson
            };
            result.Buttons.Add(RetryButton(() => StartWeek(_weekIndex)));
            result.Buttons.Add(NextButton(() =>
            {
                if (last)
                    ShowFinal();
                else
                    StartWeek(_weekIndex + 1);
            }));
            Screens.ShowResult(result);
        }

        void ShowFail(string title, string body, string lesson)
        {
            FinanceAudio.Wrong();
            var result = new ResultInfo
            {
                Title = title,
                TitleColor = FinanceUi.Coral,
                Body = body,
                Lesson = lesson
            };
            result.Buttons.Add(RetryButton(() => StartWeek(_weekIndex)));
            result.Buttons.Add(ExitButton());
            Screens.ShowResult(result);
        }

        void ShowFinal()
        {
            FinanceAudio.Victory();
            var result = new ResultInfo
            {
                Title = "Бюджет под контролем!",
                TitleColor = FinanceUi.Leaf,
                Stars = Mathf.RoundToInt(_totalStars / (float)Weeks.Length),
                Body = "Звёзд собрано: " + _totalStars + " из " + Weeks.Length * 3,
                Lesson = "Хороший бюджет — это баланс: обязательное оплачено, есть запас на сюрпризы и немного денег на радости."
            };
            result.Buttons.Add(RetryButton(() =>
            {
                _totalStars = 0;
                StartWeek(0);
            }));
            result.Buttons.Add(ExitButton());
            Screens.ShowResult(result);
        }

        void ChangeMood(int delta)
        {
            _mood = Mathf.Clamp(_mood + delta, 0, MaxMood);
            RefreshStats();
        }

        void RefreshStats()
        {
            _moneyText.text = FinanceUi.Rub(_money);
            _moneyText.color = _money >= Current.Goal ? FinanceUi.Ink : FinanceUi.Coral;
            _moodText.text = _mood + " / " + MaxMood;
            _moodBar.Animate(this, _mood / (float)MaxMood, 0.3f);
            _moodBar.SetColor(_mood >= 6 ? FinanceUi.Leaf : _mood >= 3 ? FinanceUi.Gold : FinanceUi.Coral);
        }

        void RefreshDays()
        {
            for (int i = 0; i < _dayDots.Length; i++)
            {
                bool past = i < _currentDay;
                bool current = i == _currentDay;
                _dayDots[i].color = current ? FinanceUi.Orange : past ? FinanceUi.Leaf : new Color(1f, 1f, 1f, 0.55f);
                _dayDots[i].transform.localScale = Vector3.one * (current ? 1.15f : 1f);
                Text label = _dayDots[i].GetComponentInChildren<Text>();
                label.color = current || past ? Color.white : FinanceUi.Ink;
            }
        }

        Sprite FindIcon(string key)
        {
            foreach (IconEntry entry in icons)
            {
                if (entry != null && entry.key == key)
                    return entry.sprite;
            }

            return key == Surprise || theme == null ? null : theme.moneyPile;
        }
    }
}
