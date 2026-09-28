using UnityEngine;
using System.Collections.Generic;
using LCT.MiniGames.Shop;
namespace LCT.MiniGames.Exchange
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Настройки игры")]
        public int targetTotal = 5000;

        [Header("Количество монет")]
        public int coinCount1 = 1;
        public int coinCount2 = 2;
        public int coinCount5 = 2;
        public int coinCount10 = 2;

        [Header("Настройка выигрыша купюр")]
        public int banknote5Count = 3;
        public int banknote10Count = 3;
        public int banknote50Count = 3;
        public int banknote100Count = 3;
        public int banknote200Count = 3;
        public int banknote500Count = 3;
        public int banknote1000Count = 3;
        public int banknote5000Count = 0;

        private List<int> allWonBanknotes = new List<int>();
        private List<int> banknotes = new List<int> { 5, 10, 50, 100, 200, 500, 1000, 5000 };
        private int currentLevel = 0;
        private int currentBanknoteValue;
        private int currentSum = 0;
        private int totalWins = 0;
        public List<Coin> coinsInDrop = new List<Coin>();
        public List<BanknoteCard> banknotesInDrop = new List<BanknoteCard>();
        public List<int> availableBanknotes = new List<int>();

        private List<LevelData> levelData = new List<LevelData>
        {
            new LevelData { levelName = "Уровень 1: Первые шаги", hint = "Разменяй 5 рублей монетами!" },
            new LevelData { levelName = "Уровень 2: Легкий счет", hint = "Используй монеты и банкноты!" },
            new LevelData { levelName = "Уровень 3: Время подумать", hint = "50 ₽ можно разменять монетами и банкнотами!" },
            new LevelData { levelName = "Уровень 4: Серьезный расчет", hint = "Используй все доступные банкноты!" },
            new LevelData { levelName = "Уровень 5: Сложная задача", hint = "Комбинируй монеты и банкноты!" },
            new LevelData { levelName = "Уровень 6: Почти мастер", hint = "500 ₽ - серьезная сумма!" },
            new LevelData { levelName = "Уровень 7: Мастер счета", hint = "1000 ₽ - это вызов!" },
            new LevelData { levelName = "УРОВЕНЬ 8: ФИНАЛ!", hint = "Разменяй 5000 ₽ и стань чемпионом!" }
        };

        [System.Serializable]
        public class LevelData
        {
            public string levelName;
            public string hint;
        }

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            BindPlayfield();
            allWonBanknotes.Clear();
            availableBanknotes.Clear();
        }

        void OnEnable()
        {
            Instance = this;
            BindPlayfield();
        }

        void OnDisable()
        {
            if (UIManager.Instance != null)
                MoneyPlayfield.Unbind(UIManager.Instance.dropZone);
            if (Instance == this)
                Instance = null;
        }

        void BindPlayfield()
        {
            if (UIManager.Instance == null)
                return;
            MoneyPlayfield.Bind(
                UIManager.Instance.dropZone,
                OnCoinDropped,
                RemoveCoinFromDrop,
                OnBanknoteDropped,
                RemoveBanknoteFromDrop);
        }

        public void StartGame()
        {
            BindPlayfield();
            allWonBanknotes.Clear();
            availableBanknotes.Clear();
            totalWins = 0;
            currentLevel = 0;
            UIManager.Instance.ResetStats();
            StartLevel();
        }

        void StartLevel()
        {
            if (totalWins >= targetTotal)
            {
                UIManager.Instance.ShowFinalWin(totalWins);
                return;
            }

            if (currentLevel >= banknotes.Count)
            {
                UIManager.Instance.ShowFinalWin(totalWins);
                return;
            }
            if (currentLevel == 0)
            {
                GameObject obj = GameObject.Find("ptr");
                if (obj != null)
                {
                    // Спрятали через CanvasGroup
                    CanvasGroup cg = obj.GetComponent<CanvasGroup>();
                    if (cg == null) cg = obj.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;

                    // Запускаем анимацию
                    Animator anim = obj.GetComponent<Animator>();
                    if (anim != null)
                    {
                        anim.enabled = true;
                        anim.Play("Image_Appear");
                    }
                }
            }

            UIManager.Instance.ClearDropZone();
            coinsInDrop.Clear();
            banknotesInDrop.Clear();

            currentBanknoteValue = banknotes[currentLevel];
            currentSum = 0;

            UIManager.Instance.UpdateBanknoteImage(currentBanknoteValue);
            UIManager.Instance.UpdateSum(currentSum, currentBanknoteValue);
            UIManager.Instance.UpdateScore(totalWins);

            availableBanknotes.Clear();
            foreach (int value in allWonBanknotes)
            {
                availableBanknotes.Add(value);
            }

            CreateCoins();
            UIManager.Instance.UpdateAvailableBanknotes(availableBanknotes);

            if (UIManager.Instance.levelTimer != null)
            {
                UIManager.Instance.levelTimer.StartTimer();
            }

            if (currentLevel < levelData.Count)
            {
                UIManager.Instance.UpdateLevelInfo(
                    levelData[currentLevel].levelName,
                    levelData[currentLevel].hint
                );

                AudioManager.Instance.PlayLevelVoiceByIndex(currentLevel);
            }
        }

        void CreateCoins()
        {
            var coinCounts = new Dictionary<int, int>
            {
                { 1, coinCount1 },
                { 2, coinCount2 },
                { 5, coinCount5 },
                { 10, coinCount10 }
            };
            UIManager.Instance.CreateCoins(coinCounts);
        }

        void CheckWin()
        {
            if (currentSum == currentBanknoteValue)
            {
                AudioManager.Instance.PlayLevelCompleteSound();

                if (UIManager.Instance.levelTimer != null)
                {
                    UIManager.Instance.levelTimer.StopTimer();

                    float time = UIManager.Instance.levelTimer.GetCurrentTime();
                    int stars = UIManager.Instance.levelTimer.CalculateStars(time);
                    UIManager.Instance.AddStars(stars);
                }

                totalWins += currentBanknoteValue;
                UIManager.Instance.UpdateScore(totalWins);

                int count = GetBanknoteCount(currentBanknoteValue);
                for (int i = 0; i < count; i++)
                    allWonBanknotes.Add(currentBanknoteValue);

                currentLevel++;

                UIManager.Instance.ClearDropZone();
                coinsInDrop.Clear();
                banknotesInDrop.Clear();

                if (totalWins >= targetTotal)
                {
                    UIManager.Instance.ShowFinalWin(totalWins);
                }
                else
                {
                    int nextTarget = currentLevel < banknotes.Count ? banknotes[currentLevel] : 0;
                    UIManager.Instance.ShowLevelComplete(
                        currentLevel - 1, 
                        banknotes.Count, 
                        currentBanknoteValue, 
                        totalWins, 
                        nextTarget
                    );
                }
            }
        }

        public void NextLevel()
        {
            UIManager.Instance.HideAllPanels();
            StartLevel();
        }

        public void RestartGame()
        {
            totalWins = 0;
            allWonBanknotes.Clear();
            currentLevel = 0;
            UIManager.Instance.ResetStats();
            UIManager.Instance.HideAllPanels();
            StartLevel();
        }

        int GetBanknoteCount(int value)
        {
            switch (value)
            {
                case 5: return banknote5Count;
                case 10: return banknote10Count;
                case 50: return banknote50Count;
                case 100: return banknote100Count;
                case 200: return banknote200Count;
                case 500: return banknote500Count;
                case 1000: return banknote1000Count;
                case 5000: return banknote5000Count;
                default: return 0;
            }
        }

        public void OnCoinDropped(Coin coin)
        {
            if (currentSum == currentBanknoteValue) 
            { 
                coin.ReturnToStart(); 
                return; 
            }

            if (currentSum + coin.denomination > currentBanknoteValue) 
            {
                int need = currentBanknoteValue - currentSum;
                UIManager.Instance.ShowDialog(
                    $"❌ {coin.denomination} ₽ — слишком много!",
                    $"Нужно ещё {need} ₽"
                );
                AudioManager.Instance.PlayWrongSound();
                coin.ReturnToStart();
                return; 
            }

            coin.transform.SetParent(UIManager.Instance.dropZone);
            coinsInDrop.Add(coin);
            currentSum += coin.denomination;
            UIManager.Instance.UpdateSum(currentSum, currentBanknoteValue);
            AudioManager.Instance.PlayCoinDropSound();
            UIManager.Instance.AddCoinUsed();

            CheckWin();
        }

        public void OnBanknoteDropped(BanknoteCard banknote)
        {
            if (currentSum == currentBanknoteValue) 
            { 
                banknote.ReturnToStart(); 
                return; 
            }

            if (currentSum + banknote.value > currentBanknoteValue) 
            {
                int need = currentBanknoteValue - currentSum;
                UIManager.Instance.ShowDialog(
                    $"❌ {banknote.value} ₽ — слишком много!",
                    $"Нужно ещё {need} ₽"
                );
                AudioManager.Instance.PlayWrongSound();
                banknote.ReturnToStart(); 
                return; 
            }

            banknote.transform.SetParent(UIManager.Instance.dropZone);
            banknotesInDrop.Add(banknote);
            currentSum += banknote.value;
            UIManager.Instance.UpdateSum(currentSum, currentBanknoteValue);
            availableBanknotes.Remove(banknote.value);
            UIManager.Instance.UpdateAvailableBanknotes(availableBanknotes);

            AudioManager.Instance.PlayBanknoteUseSound();
            UIManager.Instance.AddBanknoteUsed();

            CheckWin();
        }

        public void RemoveCoinFromDrop(Coin coin)
        {
            if (coinsInDrop.Contains(coin))
            {
                coinsInDrop.Remove(coin);
                currentSum -= coin.denomination;
                UIManager.Instance.UpdateSum(currentSum, currentBanknoteValue);
            }
        }

        public void RemoveBanknoteFromDrop(BanknoteCard banknote)
        {
            if (banknotesInDrop.Contains(banknote))
            {
                banknotesInDrop.Remove(banknote);
                currentSum -= banknote.value;
                UIManager.Instance.UpdateSum(currentSum, currentBanknoteValue);
                availableBanknotes.Add(banknote.value);
                UIManager.Instance.UpdateAvailableBanknotes(availableBanknotes);
            }
        }

        public void ExitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
