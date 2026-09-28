using System;
using System.Collections.Generic;
using LCT.MiniGames.Finance;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace LCT.MiniGames.PiggyCatch
{
    public class PiggyCatchGame : FinanceGame
    {
        const float PiggyWidth = 300f;
        const float FieldTop = 420f;

        [Serializable]
        public class Temptation
        {
            public string title;
            public int price;
            public Sprite sprite;
        }

        [Header("Мечты")]
        [SerializeField] Sprite ballSprite;
        [SerializeField] Sprite robotSprite;
        [SerializeField] Sprite carSprite;

        [Header("Соблазны")]
        [SerializeField] List<Temptation> temptations = new List<Temptation>();

        enum Kind
        {
            Coin,
            Banknote,
            Temptation,
            Bonus
        }

        class Level
        {
            public string Dream;
            public int Goal;
            public float Duration;
            public float SpawnInterval;
            public float MinSpeed;
            public float MaxSpeed;
            public float TemptationChance;
            public float BonusChance;
            public float BanknoteChance;
            public string Lesson;
        }

        class Faller
        {
            public Kind Kind;
            public int Value;
            public string Title;
            public RectTransform Rect;
            public float X;
            public float Y;
            public float Speed;
            public float Spin;
            public float Phase;
        }

        static readonly Level[] Levels =
        {
            new Level
            {
                Dream = "Футбольный мяч",
                Goal = 150,
                Duration = 30f,
                SpawnInterval = 0.6f,
                MinSpeed = 480f,
                MaxSpeed = 600f,
                TemptationChance = 0.22f,
                BanknoteChance = 0.08f,
                Lesson = "Копейка рубль бережёт: даже монеты по 1–2 руб быстро складываются в большую сумму."
            },
            new Level
            {
                Dream = "Робопёс",
                Goal = 250,
                Duration = 35f,
                SpawnInterval = 0.52f,
                MinSpeed = 560f,
                MaxSpeed = 700f,
                TemptationChance = 0.3f,
                BonusChance = 0.05f,
                BanknoteChance = 0.08f,
                Lesson = "Импульсные покупки — главный враг копилки. Пропусти соблазн — и мечта станет ближе."
            },
            new Level
            {
                Dream = "Машинка на пульте",
                Goal = 500,
                Duration = 45f,
                SpawnInterval = 0.45f,
                MinSpeed = 640f,
                MaxSpeed = 800f,
                TemptationChance = 0.33f,
                BonusChance = 0.07f,
                BanknoteChance = 0.14f,
                Lesson = "Проценты в банке — это деньги, которые зарабатывают твои сбережения. Чем больше накоплено, тем больше проценты."
            }
        };

        static readonly int[] CoinValues = { 1, 2, 5, 10 };
        static readonly float[] CoinWeights = { 0.2f, 0.25f, 0.3f, 0.25f };

        readonly List<Faller> _fallers = new List<Faller>();

        Text _levelNumber;
        Text _levelName;
        Text _timerText;
        Image _dreamIcon;
        Text _savedText;
        Text _leftText;
        FinanceBar _bar;
        RectTransform _goalCard;
        RectTransform _field;
        RectTransform _piggy;

        int _levelIndex;
        int _saved;
        int _totalStars;
        int _caughtCoins;
        int _caughtTemptations;
        float _timeLeft;
        float _spawnTimer;
        float _piggyX;
        float _targetX;
        bool _playing;

        Level Current => Levels[_levelIndex];

        protected override void BuildGame()
        {
            BuildHeader();
            BuildField();
        }

        protected override void ShowIntro()
        {
            Screens.ShowIntro(new IntroInfo
            {
                Topic = TopicSavings,
                Title = "Копилка",
                BuildArt = art => FinancePiggy.Build(art, "Piggy", 300f).Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 253f)),
                Body = "Лови монеты и купюры в копилку, чтобы накопить на мечту. Осторожно: соблазны отнимают деньги!",
                Steps = new[]
                {
                    "Води пальцем — копилка поедет за ним",
                    "Лови деньги и золотые проценты",
                    "Уворачивайся от соблазнов"
                },
                OnPlay = () =>
                {
                    _totalStars = 0;
                    StartLevel(0);
                }
            });
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
            _timerText = FinanceUi.Label(timer.transform, "Text", "0:30", 50, FinanceUi.Ink, TextAnchor.MiddleCenter, FontWeight.Heavy);
            _timerText.rectTransform.Stretch();

            var goal = FinanceUi.Box(GameLayer, "Goal", FinanceUi.Cream, 40f, true);
            _goalCard = goal.rectTransform;
            _goalCard.TopBand(200f, 200f, 40f);
            var disc = FinanceUi.Dot(goal.transform, "Disc", FinanceUi.Mint, 160f);
            disc.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(110f, 0f), new Vector2(160f, 160f));
            _dreamIcon = FinanceUi.Picture(disc.transform, "Dream", null);
            _dreamIcon.rectTransform.Stretch(22f, 22f, 22f, 22f);

            _savedText = FinanceUi.Label(goal.transform, "Saved", "", 58, FinanceUi.Ink, TextAnchor.MiddleLeft, FontWeight.Heavy);
            _savedText.rectTransform.Stretch(220f, 22f, 30f, 110f);
            _bar = FinanceBar.Create(goal.transform, "Bar", new Color(0f, 0f, 0f, 0.1f), FinanceUi.Leaf, 18f);
            _bar.Root.anchorMin = new Vector2(0f, 0f);
            _bar.Root.anchorMax = new Vector2(1f, 0f);
            _bar.Root.pivot = new Vector2(0.5f, 0f);
            _bar.Root.offsetMin = new Vector2(220f, 70f);
            _bar.Root.offsetMax = new Vector2(-40f, 106f);
            _leftText = FinanceUi.Label(goal.transform, "Left", "", 34, FinanceUi.Muted, TextAnchor.MiddleLeft, FontWeight.Bold);
            _leftText.rectTransform.anchorMin = Vector2.zero;
            _leftText.rectTransform.anchorMax = new Vector2(1f, 0f);
            _leftText.rectTransform.pivot = new Vector2(0.5f, 0f);
            _leftText.rectTransform.offsetMin = new Vector2(220f, 16f);
            _leftText.rectTransform.offsetMax = new Vector2(-40f, 64f);
        }

        void BuildField()
        {
            var field = FinanceUi.Box(GameLayer, "Field", new Color(0f, 0f, 0f, 0f), 0f);
            field.raycastTarget = true;
            _field = field.rectTransform.Stretch(0f, FieldTop, 0f, 0f);
            field.gameObject.AddComponent<RectMask2D>();
            field.gameObject.AddComponent<FinanceDragSurface>().Moved = OnPointer;

            var ground = FinanceUi.Box(_field, "Ground", new Color(0f, 0f, 0f, 0.18f), 30f);
            ground.rectTransform.BottomBand(40f, 40f, 80f);

            _piggy = FinancePiggy.Build(_field, "Piggy", PiggyWidth);
            _piggy.anchorMin = _piggy.anchorMax = new Vector2(0.5f, 0f);
            _piggy.pivot = new Vector2(0.5f, 0f);
            _piggy.anchoredPosition = new Vector2(0f, 60f);
        }

        void OnPointer(PointerEventData eventData)
        {
            if (!_playing)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_field, eventData.position, eventData.pressEventCamera, out Vector2 local))
                _targetX = ClampX(local.x);
        }

        float ClampX(float x)
        {
            float half = _field.rect.width * 0.5f - PiggyWidth * 0.5f - 10f;
            return Mathf.Clamp(x, -half, half);
        }

        void StartLevel(int index)
        {
            _levelIndex = index;
            Level level = Current;
            ClearFallers();

            _saved = 0;
            _caughtCoins = 0;
            _caughtTemptations = 0;
            _timeLeft = level.Duration;
            _spawnTimer = 0.4f;
            _piggyX = 0f;
            _targetX = 0f;
            _piggy.anchoredPosition = new Vector2(0f, 60f);

            _levelNumber.text = "УРОВЕНЬ " + (index + 1) + " ИЗ " + Levels.Length;
            _levelName.text = "Мечта: " + level.Dream;
            _dreamIcon.sprite = DreamSprite(index);
            _dreamIcon.enabled = _dreamIcon.sprite != null;
            RefreshGoal();
            RefreshTimer();
            FinanceTween.Pop(this, _piggy, 0.6f, 0.35f);
            _playing = true;
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

        void Update()
        {
            if (!_playing)
                return;

            float dt = Time.deltaTime;
            _timeLeft -= dt;
            RefreshTimer();

            _spawnTimer -= dt;
            if (_spawnTimer <= 0f)
            {
                Spawn();
                _spawnTimer = Current.SpawnInterval * Random.Range(0.75f, 1.25f);
            }

            _piggyX = Mathf.Lerp(_piggyX, _targetX, 1f - Mathf.Exp(-dt * 16f));
            _piggy.anchoredPosition = new Vector2(_piggyX, 60f);
            float tilt = Mathf.Clamp((_targetX - _piggyX) * -0.05f, -12f, 12f);
            _piggy.localRotation = Quaternion.Euler(0f, 0f, tilt);

            float halfHeight = _field.rect.height * 0.5f;
            float catchY = -halfHeight + 60f + PiggyWidth * 0.76f;
            for (int i = _fallers.Count - 1; i >= 0; i--)
            {
                Faller faller = _fallers[i];
                faller.Y -= faller.Speed * dt;
                faller.Phase += dt;
                float x = faller.X + Mathf.Sin(faller.Phase * 3f) * (faller.Kind == Kind.Banknote ? 26f : 8f);
                faller.Rect.anchoredPosition = new Vector2(x, faller.Y);
                if (faller.Spin != 0f)
                    faller.Rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(faller.Phase * faller.Spin) * 18f);

                bool inCatchBand = faller.Y <= catchY + 30f && faller.Y >= catchY - 90f;
                if (inCatchBand && Mathf.Abs(x - _piggyX) < PiggyWidth * 0.5f)
                {
                    Catch(faller);
                    _fallers.RemoveAt(i);
                    Destroy(faller.Rect.gameObject);
                }
                else if (faller.Y < -halfHeight - 150f)
                {
                    _fallers.RemoveAt(i);
                    Destroy(faller.Rect.gameObject);
                }
            }

            if (_saved >= Current.Goal)
                Win();
            else if (_timeLeft <= 0f)
                Lose();
        }

        void Spawn()
        {
            Level level = Current;
            float roll = Random.value;
            var faller = new Faller();

            if (roll < level.TemptationChance && temptations.Count > 0)
            {
                Temptation temptation = temptations[Random.Range(0, temptations.Count)];
                faller.Kind = Kind.Temptation;
                faller.Value = temptation.price;
                faller.Title = temptation.title;
                faller.Rect = BuildTemptation(temptation);
                faller.Spin = 2.5f;
            }
            else if (roll < level.TemptationChance + level.BonusChance)
            {
                faller.Kind = Kind.Bonus;
                faller.Rect = BuildBonus();
                faller.Spin = 4f;
            }
            else if (roll < level.TemptationChance + level.BonusChance + level.BanknoteChance)
            {
                faller.Kind = Kind.Banknote;
                faller.Value = _levelIndex == 0 || Random.value < 0.6f ? 50 : 100;
                faller.Rect = BuildMoney(faller.Value, new Vector2(230f, 88f));
                faller.Spin = 3f;
            }
            else
            {
                faller.Kind = Kind.Coin;
                faller.Value = PickCoin();
                faller.Rect = BuildMoney(faller.Value, new Vector2(118f, 118f));
            }

            float half = _field.rect.width * 0.5f - 130f;
            faller.X = Random.Range(-half, half);
            faller.Y = _field.rect.height * 0.5f + 120f;
            faller.Speed = Random.Range(level.MinSpeed, level.MaxSpeed);
            faller.Phase = Random.value * 6f;
            faller.Rect.anchoredPosition = new Vector2(faller.X, faller.Y);
            _fallers.Add(faller);
        }

        static int PickCoin()
        {
            float roll = Random.value;
            for (int i = 0; i < CoinValues.Length; i++)
            {
                roll -= CoinWeights[i];
                if (roll <= 0f)
                    return CoinValues[i];
            }

            return CoinValues[CoinValues.Length - 1];
        }

        RectTransform BuildMoney(int value, Vector2 size)
        {
            Sprite sprite = theme != null ? theme.GetMoneySprite(value) : null;
            Image image;
            if (sprite != null)
            {
                image = FinanceUi.Picture(_field, "Money_" + value, sprite);
            }
            else
            {
                image = FinanceUi.Dot(_field, "Money_" + value, FinanceUi.Gold, size.y);
                FinanceUi.Label(image.transform, "Value", value.ToString(), 44, FinanceUi.Ink).rectTransform.Stretch();
            }

            image.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, size);
            return image.rectTransform;
        }

        RectTransform BuildTemptation(Temptation temptation)
        {
            var root = FinanceUi.Node(_field, "Temptation_" + temptation.title);
            root.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160f, 190f));
            var halo = FinanceUi.Dot(root, "Halo", new Color(0.92f, 0.32f, 0.27f, 0.35f), 160f);
            halo.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(160f, 160f));
            FinanceUi.Picture(halo.transform, "Image", temptation.sprite).rectTransform.Stretch(14f, 14f, 14f, 14f);
            var tag = FinanceUi.Box(root, "Price", FinanceUi.Coral, 22f);
            tag.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(150f, 50f));
            FinanceUi.Label(tag.transform, "Text", "−" + temptation.price, 36, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy)
                .rectTransform.Stretch();
            return root;
        }

        RectTransform BuildBonus()
        {
            var dot = FinanceUi.Dot(_field, "Bonus", FinanceUi.Gold, 130f);
            dot.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130f, 130f));
            var ring = dot.gameObject.AddComponent<Outline>();
            ring.effectColor = new Color(1f, 1f, 1f, 0.9f);
            ring.effectDistance = new Vector2(5f, -5f);
            FinanceUi.Label(dot.transform, "Text", "%", 80, Color.white, TextAnchor.MiddleCenter, FontWeight.Heavy)
                .Outlined(new Color(0.6f, 0.4f, 0f, 0.6f)).rectTransform.Stretch();
            return dot.rectTransform;
        }

        void Catch(Faller faller)
        {
            switch (faller.Kind)
            {
                case Kind.Coin:
                case Kind.Banknote:
                    _saved += faller.Value;
                    _caughtCoins++;
                    FinanceAudio.Money(faller.Value);
                    Float("+" + faller.Value, Color.white, _piggy, new Vector2(0f, 300f), faller.Kind == Kind.Banknote ? 76 : 58);
                    FinanceTween.Punch(this, _piggy, 0.08f, 0.18f);
                    break;

                case Kind.Bonus:
                    int interest = Mathf.Max(5, Mathf.RoundToInt(_saved * 0.1f));
                    _saved += interest;
                    FinanceAudio.Good();
                    Float("Проценты +" + interest, FinanceUi.Gold, _piggy, new Vector2(0f, 320f), 64);
                    FinanceTween.Punch(this, _piggy, 0.18f, 0.3f);
                    break;

                case Kind.Temptation:
                    int loss = Mathf.Min(_saved, faller.Value);
                    _saved -= loss;
                    _caughtTemptations++;
                    FinanceAudio.Wrong();
                    Float(faller.Title + " −" + faller.Value, FinanceUi.Coral, _piggy, new Vector2(0f, 320f), 58);
                    FinanceTween.Shake(this, _goalCard, _goalCard.anchoredPosition, 14f, 0.3f);
                    if (_caughtTemptations == 1)
                        Screens.Toast("Импульсная покупка!", "Соблазны отнимают деньги у мечты", FinanceUi.Coral, 1.4f);
                    break;
            }

            RefreshGoal();
        }

        void Win()
        {
            _playing = false;
            ClearFallers();
            FinanceAudio.Good();
            MiniGamePayout.GrantForActiveScene();

            Level level = Current;
            float fraction = _timeLeft / level.Duration;
            int stars = fraction >= 0.35f ? 3 : fraction >= 0.15f ? 2 : 1;
            _totalStars += stars;
            bool last = _levelIndex >= Levels.Length - 1;

            var result = new ResultInfo
            {
                Title = "Мечта накоплена!",
                TitleColor = FinanceUi.Leaf,
                Stars = stars,
                Body = level.Dream + " — " + FinanceUi.Rub(level.Goal)
                       + "\nОсталось времени: " + Mathf.CeilToInt(_timeLeft) + " сек"
                       + "\nПоймано соблазнов: " + _caughtTemptations,
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
            FinanceTween.Delay(this, 0.3f, () => Screens.ShowResult(result));
        }

        void Lose()
        {
            _playing = false;
            ClearFallers();
            FinanceAudio.Wrong();

            Level level = Current;
            var result = new ResultInfo
            {
                Title = "Время вышло",
                TitleColor = FinanceUi.Coral,
                Body = "Накоплено " + FinanceUi.Rub(_saved) + " из " + FinanceUi.Rub(level.Goal)
                       + "\nНе хватило " + FinanceUi.Rub(level.Goal - _saved),
                Lesson = _caughtTemptations > 2
                    ? "Соблазны съели часть копилки. Пропускай ненужные покупки — так копить быстрее."
                    : "Копить — это терпение. Каждая пойманная монетка приближает мечту."
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
                Title = "Все мечты сбылись!",
                TitleColor = FinanceUi.Leaf,
                Stars = Mathf.RoundToInt(_totalStars / (float)Levels.Length),
                Body = "Звёзд собрано: " + _totalStars + " из " + Levels.Length * 3,
                Lesson = "Чтобы накопить: поставь цель, откладывай регулярно, пропускай импульсные покупки и пусть банк добавляет проценты."
            };
            result.Buttons.Add(RetryButton(() =>
            {
                _totalStars = 0;
                StartLevel(0);
            }));
            result.Buttons.Add(ExitButton());
            Screens.ShowResult(result);
        }

        void RefreshGoal()
        {
            Level level = Current;
            _savedText.text = FinanceUi.Rub(_saved) + " / " + level.Goal;
            _leftText.text = _saved >= level.Goal ? "Цель достигнута!" : "осталось накопить " + FinanceUi.Rub(level.Goal - _saved);
            _bar.Animate(this, _saved / (float)level.Goal, 0.2f);
        }

        void RefreshTimer()
        {
            int seconds = Mathf.Max(0, Mathf.CeilToInt(_timeLeft));
            _timerText.text = (seconds / 60) + ":" + (seconds % 60).ToString("00");
            _timerText.color = seconds <= 5 ? FinanceUi.Coral : FinanceUi.Ink;
        }

        void ClearFallers()
        {
            foreach (Faller faller in _fallers)
            {
                if (faller.Rect != null)
                    Destroy(faller.Rect.gameObject);
            }

            _fallers.Clear();
        }
    }
}
