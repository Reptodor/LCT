using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LCT.MiniGames.Shop;
namespace LCT.MiniGames.Exchange
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("Стартовая панель")]
        public GameObject panelStart;
        public Button playButton;

        [Header("HUD Панель")]
        public GameObject panelHUD;
        public Text levelNameText;  
        public Text hintText;      
        public Text scoreText;
        public Text sumText;
        public Image banknoteImage;

        [Header("Игровые панели")]
        public Transform panelCoins;
        public Transform panelBanknotes;
        public Transform dropZone;

        [Header("Панель завершения уровня")]
        public GameObject panelLevelComplete;
        public Text levelText;
        public Text rewardText;
        public Text totalScoreText;
        public Text nextBanknoteText;
        public Button nextLevelButton;

        [Header("Ссылка на таймер")]  
        public LevelTimer levelTimer;

        [Header("Панель финальной победы")]
        public GameObject panelFinalWin;
        public Text finalScoreText;
        public Text finalStarsText;
        public Text finalCoinsUsedText;
        public Text finalBanknotesUsedText;
        public Button restartButton;
        public Button exitButton;

        [Header("Подсказки")]
        public Text hintPopup;  
        public float popupDuration = 1.5f;  
        private float popupTimer = 0f;
        private bool isPopupActive = false;

        private int totalStars = 0;
        private int totalCoinsUsed = 0;
        private int totalBanknotesUsed = 0;

        [Header("Диалоговая панель")]
        public GameObject dialogPanel;
        public Text dialogTitle;
        public Text dialogDetails;
        public float dialogDuration = 1.5f;
        private float dialogTimer = 0f;
        private bool isDialogActive = false;

        [Header("Префабы")]
        public GameObject coinPrefab;
        public GameObject banknoteCardPrefab;

        [Header("Спрайты")]
        public List<CoinSprite> coinSprites;
        public List<BanknoteSprite> banknoteSprites;

        [System.Serializable]
        public class CoinSprite
        {
            public int denomination;
            public Sprite sprite;
        }

        [System.Serializable]
        public class BanknoteSprite
        {
            public int value;
            public Sprite sprite;
        }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        void Start()
        {
            HideAllPanels();
            ShowStartPanel();

            if (dialogPanel != null)
                dialogPanel.SetActive(false);
        }

        public void ShowStartPanel()
        {
            if (panelStart != null)
                panelStart.SetActive(true);

            if (panelHUD != null)
                panelHUD.SetActive(false);

            if (panelLevelComplete != null)
                panelLevelComplete.SetActive(false);

            if (panelFinalWin != null)
                panelFinalWin.SetActive(false);
        }

        public void HideStartPanel()
        {
            if (panelStart != null)
                panelStart.SetActive(false);

            if (panelHUD != null)
                panelHUD.SetActive(true);
        }

        public void OnPlayClicked()
        {
            HideStartPanel();

            if (GameManager.Instance != null)
                GameManager.Instance.StartGame();
        }

        void Update()
        {
            if (isPopupActive)
            {
                popupTimer -= Time.deltaTime;
                if (popupTimer <= 0f)
                {
                    if (hintPopup != null)
                        hintPopup.gameObject.SetActive(false);
                    isPopupActive = false;
                }
            }

            if (isDialogActive)
            {
                dialogTimer -= Time.deltaTime;
                if (dialogTimer <= 0f)
                {
                    HideDialog();
                }
            }
        }

        public void AddStars(int stars)
        {
            totalStars += stars;
        }

        public void AddCoinUsed()
        {
            totalCoinsUsed++;
        }

        public void AddBanknoteUsed()
        {
            totalBanknotesUsed++;
        }

        public void ResetStats()
        {
            totalStars = 0;
            totalCoinsUsed = 0;
            totalBanknotesUsed = 0;
        }

        public void ShowDialog(string title, string details = "")
        {
            if (dialogPanel != null)
            {
                if (dialogTitle != null)
                    dialogTitle.text = title;

                if (dialogDetails != null)
                    dialogDetails.text = details;

                dialogPanel.SetActive(true);
                isDialogActive = true;
                dialogTimer = dialogDuration;
            }
        }

        public void HideDialog()
        {
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
                isDialogActive = false;
            }
        }

        public void UpdateLevelInfo(string levelName, string hint)
        {
            if (levelNameText != null)
                levelNameText.text = levelName;

            if (hintText != null)
                hintText.text = hint;
        }

        public void HideAllPanels()
        {
            if (panelLevelComplete != null)
                panelLevelComplete.SetActive(false);

            if (panelFinalWin != null)
                panelFinalWin.SetActive(false);
        }

        public void ShowLevelComplete(int currentLevel, int totalLevels, int reward, int totalWins, int nextTarget)
        {
            HideAllPanels();

            if (panelLevelComplete != null)
            {
                panelLevelComplete.SetActive(true);

                if (levelText != null)
                    levelText.text = $"Уровень {currentLevel + 1} из {totalLevels}";

                if (rewardText != null)
                    rewardText.text = $"+{reward} ₽";

                if (totalScoreText != null)
                    totalScoreText.text = $"Всего заработано: {totalWins} ₽";

                if (nextBanknoteText != null)
                {
                    if (currentLevel + 1 < totalLevels)
                        nextBanknoteText.text = $"Следующая цель: {nextTarget} ₽";
                    else
                        nextBanknoteText.text = "🎯 Последний уровень!";
                }

                if (levelTimer != null)
                {
                    float time = levelTimer.GetCurrentTime();
                    levelTimer.ShowStars(time);
                }
            }
        }

        public void ShowFinalWin(int totalWins)
        {
            HideAllPanels();

            if (panelFinalWin != null)
            {
                panelFinalWin.SetActive(true);
                AudioManager.Instance.PlayGameOverSound();

                if (finalScoreText != null)
                    finalScoreText.text = $"💰 Общий выигрыш: {totalWins} ₽";

                if (finalStarsText != null)
                    finalStarsText.text = $"⭐ Звёзд заработано: {totalStars}";

                if (finalCoinsUsedText != null)
                    finalCoinsUsedText.text = $"🪙 Использовано монет: {totalCoinsUsed}";

                if (finalBanknotesUsedText != null)
                    finalBanknotesUsedText.text = $"💵 Использовано банкнот: {totalBanknotesUsed}";
            }
        }

        public void UpdateScore(int totalWins)
        {
            if (scoreText != null)
                scoreText.text = $"💰 {totalWins} ₽";
        }

        public void UpdateSum(int currentSum, int target)
        {
            if (sumText != null)
                sumText.text = $"{currentSum} / {target} ₽";
        }

        public void UpdateBanknoteImage(int value)
        {
            if (banknoteImage != null)
            {
                Sprite sprite = GetBanknoteSprite(value);
                if (sprite != null)
                    banknoteImage.sprite = sprite;
            }
        }

        public void CreateCoins(Dictionary<int, int> coinCounts)
        {
            if (panelCoins == null || coinPrefab == null) return;

            foreach (Transform child in panelCoins)
                Destroy(child.gameObject);

            foreach (var coinData in coinSprites)
            {
                if (coinCounts.ContainsKey(coinData.denomination))
                {
                    int count = coinCounts[coinData.denomination];
                    for (int i = 0; i < count; i++)
                    {
                        GameObject coinObj = Instantiate(coinPrefab, panelCoins);
                        Coin coin = coinObj.GetComponent<Coin>();
                        coin.denomination = coinData.denomination;
                        coinObj.GetComponent<Image>().sprite = coinData.sprite;
                    }
                }
            }
        }

        public void UpdateAvailableBanknotes(List<int> availableBanknotes)
        {
            if (panelBanknotes == null || banknoteCardPrefab == null) return;

            foreach (Transform child in panelBanknotes)
                Destroy(child.gameObject);

            foreach (int value in availableBanknotes)
            {
                GameObject card = Instantiate(banknoteCardPrefab, panelBanknotes);
                Image img = card.GetComponent<Image>();
                if (img != null)
                    img.sprite = GetBanknoteSprite(value);

                BanknoteCard cardComponent = card.GetComponent<BanknoteCard>();
                if (cardComponent != null)
                    cardComponent.value = value;
            }
        }

        public void ClearDropZone()
        {
            if (dropZone == null) return;

            for (int i = dropZone.childCount - 1; i >= 0; i--)
            {
                Transform child = dropZone.GetChild(i);
                if (child != null && (child.GetComponent<Coin>() != null || child.GetComponent<BanknoteCard>() != null))
                    Destroy(child.gameObject);
            }
        }

        public Sprite GetBanknoteSprite(int value)
        {
            foreach (var item in banknoteSprites)
            {
                if (item.value == value)
                    return item.sprite;
            }
            return null;
        }

        public Sprite GetCoinSprite(int denomination)
        {
            foreach (var item in coinSprites)
            {
                if (item.denomination == denomination)
                    return item.sprite;
            }
            return null;
        }

        public void OnNextLevelClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.NextLevel();
        }

        public void OnRestartClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RestartGame();
        }

        public void OnExitClicked()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
