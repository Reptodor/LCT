using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace LCT.MiniGames.Shop
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance;

        [Header("UI — общие")]
        public Text taskText;
        public Text budgetText;

        [Header("UI — витрина (скрытая панель)")]
        public GameObject panelProducts;
        public Transform productsPanel;
        public GameObject productPrefab;

        [Header("UI — покупка (Panel_BuyZone)")]
        public Image selectedProductIcon;
        public Sprite basketSprite;
        public Text selectedProductName;
        public Text selectedProductPrice;
        public Text paidText;
        public Button buyButton;
        public Button retryButton;

        [Header("UI — кошелёк")]
        public Transform panelCoins;
        public Transform panelBanknotes;
        public Transform dropZone;

        [Header("Префабы")]
        public GameObject coinPrefab;
        public GameObject banknoteCardPrefab;

        [Header("Спрайты")]
        public List<CoinSprite> coinSprites;
        public List<BanknoteSprite> banknoteSprites;

        [Header("Монеты в кошельке")]
        public int coinCount1 = 2;
        public int coinCount2 = 4;
        public int coinCount5 = 2;
        public int coinCount10 = 3;

        [Header("Уровни (магазины)")]
        public List<ShopLevel> shopLevels;

        [Header("Диалоговая панель")]
        public GameObject dialogPanel;
        public Text dialogTitle;
        public Text dialogDetails;
        public float dialogDuration = 1.5f;

        [Header("Панель завершения уровня")]
        public GameObject panelLevelComplete;
        public Text titleText;
        public Text levelText;
        public Text rewardText;
        public Text totalScoreText;
        public Button nextButton;
        public Button exitButton;

        [Header("Стартовая панель")]
        public GameObject panelStart;
        public Button playButton;

        [Header("Панель победы")]
        public GameObject panelFinalWin;
        public Text finalScoreText;
        public Button restartButton;
        public Button exitFinalButton;

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

        private ShopLevel currentShop;
        private int currentShopIndex = 0;
        private List<ProductView> allProducts = new List<ProductView>();
        private List<int> purchasedProductIDs = new List<int>();   
        private ProductData selectedProduct;
        private ProductView selectedProductView;

        private int budget = 0;
        private int spent = 0;
        private int paid = 0;
        private int totalSaved = 0;

        public List<Coin> coinsInDrop = new List<Coin>();
        public List<BanknoteCard> banknotesInDrop = new List<BanknoteCard>();

        private float dialogTimer = 0f;
        private bool isDialogActive = false;

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            Instance = this;
            MoneyPlayfield.Bind(dropZone, OnCoinDropped, RemoveCoinFromDrop, OnBanknoteDropped, RemoveBanknoteFromDrop);
        }

        void OnDisable()
        {
            MoneyPlayfield.Unbind(dropZone);
            if (Instance == this)
                Instance = null;
        }

        void Start()
        {
            LoadProgress();

            if (dialogPanel != null)
                dialogPanel.SetActive(false);

            if (panelLevelComplete != null)
                panelLevelComplete.SetActive(false);

            if (panelFinalWin != null)
                panelFinalWin.SetActive(false);

            if (panelProducts != null)
                panelProducts.SetActive(false);

            if (panelStart != null)
                panelStart.SetActive(true);
            else
                StartShop(MiniGameProgress.ResumeIndex(shopLevels.Count));
        }

        void Update()
        {
            if (isDialogActive)
            {
                dialogTimer -= Time.deltaTime;
                if (dialogTimer <= 0f)
                    HideDialog();
            }
        }

        // ===== СТАРТОВАЯ ПАНЕЛЬ =====
        public void OnPlayButton()
        {
            if (panelStart != null)
                panelStart.SetActive(false);

            StartShop(MiniGameProgress.ResumeIndex(shopLevels.Count));
        }

        // ===== ДИАЛОГ =====
        public void ShowDialog(string title, string details = "")
        {
            if (dialogPanel == null) return;

            if (dialogTitle != null)
                dialogTitle.text = title;

            if (dialogDetails != null)
                dialogDetails.text = details;

            dialogPanel.SetActive(true);
            isDialogActive = true;
            dialogTimer = dialogDuration;
        }

        public void HideDialog()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);

            isDialogActive = false;
        }

        // ===== ЗАГРУЗКА / СОХРАНЕНИЕ =====
        void LoadProgress()
        {
            totalSaved = PlayerPrefs.GetInt(ProgressKey(), 0);
        }

        void SaveProgress()
        {
            PlayerPrefs.SetInt(ProgressKey(), totalSaved);
            PlayerPrefs.Save();
        }

        static string ProgressKey()
        {
            return GameSession.ShopSavingsKey();
        }

        // ===== ЗАПУСК МАГАЗИНА =====
        public void StartShop(int levelIndex)
        {
            if (levelIndex >= shopLevels.Count)
            {
                ShowFinalWin();
                return;
            }

            currentShopIndex = levelIndex;
            currentShop = shopLevels[levelIndex];
            budget = currentShop.budget;
            spent = 0;
            paid = 0;

            purchasedProductIDs.Clear();   // сброс купленных

            if (panelLevelComplete != null)
                panelLevelComplete.SetActive(false);

            if (panelProducts != null)
                panelProducts.SetActive(false);

            taskText.text = $"ЗАДАНИЕ ОТ {currentShop.parentName.ToUpper()}:\n\"{currentShop.taskDescription}\"";

            // Озвучка уровня
            if (AudioManager.Instance != null)
             AudioManager.Instance.PlayLevelVoiceByIndex(currentShopIndex);

            if (budgetText != null)
            budgetText.text = $"Бюджет: {budget} руб";

            // При старте показываем "Купить", скрываем "Заново"
            if (buyButton != null)
                buyButton.gameObject.SetActive(true);

            if (retryButton != null)
                retryButton.gameObject.SetActive(false);

            CreateCoins();
            CreateBanknotes();

            ClearDropZone();
            ResetBuyZone();
            CheckBudgetForBuying();
        }

        // ===== ОТКРЫТИЕ ПАНЕЛИ ТОВАРОВ =====
        public void OnBuyZoneClick()
        {
            if (panelProducts != null)
            {
                panelProducts.SetActive(true);
                CreateProducts();
            }
        }

        public void CloseProductsPanel()
        {
            if (panelProducts != null)
                panelProducts.SetActive(false);
        }

        // ===== СОЗДАНИЕ ТОВАРОВ =====
        void CreateProducts()
        {
            foreach (Transform child in productsPanel)
                Destroy(child.gameObject);
            allProducts.Clear();

            foreach (ProductData product in currentShop.products)
            {
                GameObject productObj = Instantiate(productPrefab, productsPanel);
                ProductView view = productObj.GetComponent<ProductView>();
                view.Setup(product, this);

                // Если товар уже куплен — показываем галочку
                if (purchasedProductIDs.Contains(product.id))
                    view.MarkAsBought();

                allProducts.Add(view);
            }
        }

        // ===== СОЗДАНИЕ МОНЕТ =====
        void CreateCoins()
        {
            foreach (Transform child in panelCoins)
                Destroy(child.gameObject);

            int[] denoms = { 1, 2, 5, 10 };
            int[] counts = { coinCount1, coinCount2, coinCount5, coinCount10 };

            for (int i = 0; i < denoms.Length; i++)
            {
                for (int j = 0; j < counts[i]; j++)
                {
                    GameObject coinObj = Instantiate(coinPrefab, panelCoins);
                    Coin coin = coinObj.GetComponent<Coin>();
                    coin.denomination = denoms[i];
                    coin.GetComponent<Image>().sprite = GetCoinSprite(denoms[i]);
                }
            }
        }

        // ===== СОЗДАНИЕ КУПЮР =====
        void CreateBanknotes()
        {
            foreach (Transform child in panelBanknotes)
                Destroy(child.gameObject);

            int coinsSum = 
                coinCount1 * 1 + 
                coinCount2 * 2 + 
                coinCount5 * 5 + 
                coinCount10 * 10;

            int banknotesBudget = budget - coinsSum;

            if (banknotesBudget <= 0) return;

            int[] values = { 500, 200, 100, 50, 10, 5 };
            int remaining = banknotesBudget;

            foreach (int value in values)
            {
                int count = remaining / value;
                remaining -= count * value;

                for (int i = 0; i < count; i++)
                {
                    GameObject cardObj = Instantiate(banknoteCardPrefab, panelBanknotes);
                    BanknoteCard card = cardObj.GetComponent<BanknoteCard>();
                    card.value = value;
                    card.GetComponent<Image>().sprite = GetBanknoteSprite(value);
                }
            }
        }

        Sprite GetCoinSprite(int denom)
        {
            foreach (var item in coinSprites)
                if (item.denomination == denom) return item.sprite;
            return null;
        }

        Sprite GetBanknoteSprite(int value)
        {
            foreach (var item in banknoteSprites)
                if (item.value == value) return item.sprite;
            return null;
        }

        // ===== ВЫБОР ТОВАРА =====
        public void SelectProduct(ProductData product, ProductView view)
        {
            selectedProduct = product;
            selectedProductView = view;

            selectedProductIcon.gameObject.SetActive(true);
            selectedProductIcon.sprite = product.icon;
            selectedProductIcon.preserveAspect = true;

            selectedProductName.text = product.productName;
            selectedProductPrice.text = $"Цена: {product.price} руб";

            CloseProductsPanel();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCoinDropSound();

            UpdatePaidText();
        }

        // ===== ДЕНЬГИ В ЗОНЕ =====
        public void OnCoinDropped(Coin coin)
        {
            if (selectedProduct == null)
            {
                coin.ReturnToStart();

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayWrongSound();

                ShowDialog("Выбери товар!", "Кликни на витрину");
                return;
            }

            coin.transform.SetParent(dropZone);
            coinsInDrop.Add(coin);
            paid += coin.denomination;
            UpdatePaidText();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCoinDropSound();
        }

        public void OnBanknoteDropped(BanknoteCard banknote)
        {
            if (selectedProduct == null)
            {
                banknote.ReturnToStart();

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayWrongSound();

                ShowDialog("Выбери товар!", "Кликни на витрину");
                return;
            }

            banknote.transform.SetParent(dropZone);
            banknotesInDrop.Add(banknote);
            paid += banknote.value;
            UpdatePaidText();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayBanknoteUseSound();
        }

        public void RemoveCoinFromDrop(Coin coin)
        {
            if (coinsInDrop.Contains(coin))
            {
                coinsInDrop.Remove(coin);
                paid -= coin.denomination;
                UpdatePaidText();
            }
        }

        public void RemoveBanknoteFromDrop(BanknoteCard banknote)
        {
            if (banknotesInDrop.Contains(banknote))
            {
                banknotesInDrop.Remove(banknote);
                paid -= banknote.value;
                UpdatePaidText();
            }
        }

        void UpdatePaidText()
        {
            if (selectedProduct != null)
                paidText.text = $"Оплачено: {paid} руб / {selectedProduct.price} руб";
            else
                paidText.text = "Оплачено: 0 руб";
        }

        // ===== КНОПКА "КУПИТЬ" =====
        public void OnBuyButton()
        {
            if (selectedProduct == null)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayWrongSound();

                ShowDialog("Выбери товар!", "Кликни на витрину");
                return;
            }

            if (paid < selectedProduct.price)
            {
                int need = selectedProduct.price - paid;

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayWrongSound();

                ShowDialog("Не хватает денег!", $"Нужно ещё {need} руб");
                return;
            }

            int overpay = paid - selectedProduct.price;
            int remainingBudget = budget - spent;

            if (selectedProduct.price > remainingBudget)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayWrongSound();

                ShowDialog("Не хватает бюджета!", $"Остаток: {remainingBudget} руб");
                return;
            }

            spent += selectedProduct.price;

            // Запоминаем что товар куплен
            if (!purchasedProductIDs.Contains(selectedProduct.id))
                purchasedProductIDs.Add(selectedProduct.id);

            selectedProductView.MarkAsBought();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayLevelCompleteSound();

            if (overpay > 0)
            {
                GiveChange(overpay);
                ShowDialog("Куплено!", $"Сдача: {overpay} руб");
            }
            else
            {
                ShowDialog("Куплено!", "Точно без сдачи!");
            }

            ClearDropZone();

            if (budgetText != null)
            budgetText.text = $"Бюджет: {budget - spent} руб"; 

            selectedProduct = null;
            selectedProductView = null;
            paid = 0;
            ResetBuyZone();

            CheckTaskComplete();
            CheckBudgetForBuying();
        }

        // ===== ВЫДАЧА СДАЧИ =====
        void GiveChange(int amount)
        {
            int remaining = amount;

            int[] banknoteValues = { 500, 200, 100, 50, 10, 5 };

            foreach (int value in banknoteValues)
            {
                int count = remaining / value;
                remaining -= count * value;

                for (int i = 0; i < count; i++)
                {
                    GameObject cardObj = Instantiate(banknoteCardPrefab, panelBanknotes);
                    BanknoteCard card = cardObj.GetComponent<BanknoteCard>();
                    card.value = value;
                    card.GetComponent<Image>().sprite = GetBanknoteSprite(value);
                }
            }

            int[] coinValues = { 2, 1 };

            foreach (int value in coinValues)
            {
                int count = remaining / value;
                remaining -= count * value;

                for (int i = 0; i < count; i++)
                {
                    GameObject coinObj = Instantiate(coinPrefab, panelCoins);
                    Coin coin = coinObj.GetComponent<Coin>();
                    coin.denomination = value;
                    coin.GetComponent<Image>().sprite = GetCoinSprite(value);
                }
            }

            if (remaining > 0)
                Debug.Log($"Остаток {remaining} руб не выдан");
        }

        // ===== СБРОС ЗОНЫ ПОКУПКИ =====
        void ResetBuyZone()
        {
            selectedProductIcon.gameObject.SetActive(true);
            selectedProductIcon.sprite = basketSprite;
            selectedProductIcon.preserveAspect = true;

            selectedProductName.text = "Выбери товар";
            selectedProductPrice.text = "";
            paidText.text = "Оплачено: 0 руб";
        }

        // ===== ПРОВЕРКА ЗАДАНИЯ =====
        void CheckTaskComplete()
        {
            bool allBought = true;

            // Проверяем через список купленных ID
            foreach (ProductData product in currentShop.products)
            {
                if (product.isRequired)
                {
                    if (!purchasedProductIDs.Contains(product.id))
                    {
                        allBought = false;
                        break;
                    }
                }
            }

            if (allBought)
            {
                int remaining = budget - spent;
                totalSaved += remaining;
                SaveProgress();

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayLevelCompleteSound();

                ShowLevelComplete();
            }
        }

        // ===== ПРОВЕРКА: ХВАТАЕТ ЛИ ДЕНЕГ НА ЧТО-ТО =====
    void CheckBudgetForBuying()
    {
        if (currentShop == null || currentShop.products.Count == 0) return;

        // Минимальная цена среди НЕ купленных товаров
        int minPrice = int.MaxValue;
        bool hasUnbought = false;

        foreach (ProductData product in currentShop.products)
        {
            if (purchasedProductIDs.Contains(product.id)) continue;

            hasUnbought = true;

            if (product.price < minPrice)
                minPrice = product.price;
        }

        // Если всё куплено — ничего не делаем
        if (!hasUnbought) return;

        int remaining = budget - spent;

        if (remaining < minPrice)
        {
            // Денег не хватает — показываем "Заново", скрываем "Купить"
            if (buyButton != null)
                buyButton.gameObject.SetActive(false);

            if (retryButton != null)
                retryButton.gameObject.SetActive(true);
        }
        else
        {
            // Денег хватает — показываем "Купить"
            if (buyButton != null)
                buyButton.gameObject.SetActive(true);

            if (retryButton != null)
                retryButton.gameObject.SetActive(false);
        }
    }

        // ===== ПАНЕЛЬ ЗАВЕРШЕНИЯ УРОВНЯ =====
        void ShowLevelComplete()
        {
            if (panelLevelComplete == null) return;

            if (!panelLevelComplete.activeSelf)
            {
                MiniGamePayout.GrantForActiveScene();
                MiniGameProgress.AdvanceTo(currentShopIndex + 1, shopLevels.Count);
            }

            int remaining = budget - spent;

            if (titleText != null)
                titleText.text = "ЗАДАНИЕ ВЫПОЛНЕНО!";

            if (levelText != null)
                levelText.text = $"Потрачено: {spent} руб";

            if (rewardText != null)
                rewardText.text = $"В копилку: +{remaining} руб";

            if (totalScoreText != null)
                totalScoreText.text = $"Всего в копилке: {totalSaved} руб";

            panelLevelComplete.SetActive(true);
        }

        public void OnLevelCompleteNextButton()
        {
            if (panelLevelComplete != null)
                panelLevelComplete.SetActive(false);

            int next = currentShopIndex + 1;
            if (next < shopLevels.Count)
                StartShop(next);
            else
                ShowFinalWin();
        }

        // ===== ПАНЕЛЬ ПОБЕДЫ =====
        void ShowFinalWin()
        {
            if (panelFinalWin == null) return;

            if (finalScoreText != null)
                finalScoreText.text = $"В копилке: {totalSaved} руб";

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayLevelCompleteSound();

            panelFinalWin.SetActive(true);
        }

        public void OnRetryButton()
        {
            purchasedProductIDs.Clear();
            StartShop(currentShopIndex);
        }

        public void OnRestartButton()
        {
            MiniGameProgress.ResetActive();
            totalSaved = 0;
            SaveProgress();

            if (panelFinalWin != null)
                panelFinalWin.SetActive(false);

            currentShopIndex = 0;
            StartShop(0);
        }

        public void OnExitButton()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        // ===== ОЧИСТКА ЗОНЫ =====
        void ClearDropZone()
        {
            foreach (Transform child in dropZone)
                Destroy(child.gameObject);
            coinsInDrop.Clear();
            banknotesInDrop.Clear();
            paid = 0;
        }

        // ===== ВЫХОД ИЗ МАГАЗИНА =====
        public void ExitShop()
        {
            ShowDialog("Выход", "Задание не выполнено");
        }
    }
}
