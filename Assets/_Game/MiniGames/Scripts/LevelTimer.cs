using UnityEngine;
using UnityEngine.UI;

public class LevelTimer : MonoBehaviour
{
    [Header("Звёзды")]
    public GameObject starsContainer;      // Контейнер для звёзд
    public Image star1;                    // Ссылки на компоненты Image
    public Image star2;
    public Image star3;
    
    [Header("Спрайты звёзд")]
    public Sprite starFull;    // Золотая звезда (полная)
    public Sprite starEmpty;   // Серая звезда (пустая)
    
    [Header("Настройки времени для звёзд (в секундах)")]
    public float timeFor3Stars = 10f;   // < 10 сек → 3 звезды
    public float timeFor2Stars = 20f;   // < 20 сек → 2 звезды
    public float timeFor1Star = 30f;    // < 30 сек → 1 звезда
    
    private float currentTime = 0f;
    private bool isRunning = false;
    private int earnedStars = 0;
    
    void Start()
    {
        if (starsContainer != null)
            starsContainer.SetActive(false);
    }
    
    void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
        }
    }
    
    // === УПРАВЛЕНИЕ ТАЙМЕРОМ ===
    
    public void StartTimer()
    {
        currentTime = 0f;
        isRunning = true;
        
        if (starsContainer != null)
            starsContainer.SetActive(false);
        
        Debug.Log("Таймер запущен!");
    }
    
    public void StopTimer()
    {
        isRunning = false;
        Debug.Log($"Таймер остановлен. Время: {currentTime:F1} сек");
    }
    
    public float GetCurrentTime()
    {
        return currentTime;
    }
    
    // === РАСЧЁТ ЗВЁЗД ===
    
    public int CalculateStars(float time)
    {
        if (time <= timeFor3Stars)
            return 3;
        else if (time <= timeFor2Stars)
            return 2;
        else if (time <= timeFor1Star)
            return 1;
        else
            return 0;
    }
    
    // === ОБНОВЛЕНИЕ ЗВЁЗД НА ПАНЕЛИ ===
    
    public void ShowStars(float time)
    {
        earnedStars = CalculateStars(time);
        
        if (starsContainer != null)
            starsContainer.SetActive(true);
        
        UpdateStarUI(earnedStars);
        
        Debug.Log($"Звёзд получено: {earnedStars} (время: {time:F1} сек)");
    }
    
    private void UpdateStarUI(int starsCount)
    {
        if (star1 == null || star2 == null || star3 == null) 
        {
            Debug.LogWarning("Звёзды не назначены в LevelTimer!");
            return;
        }
        
        // Обновляем спрайты звёзд
        star1.sprite = starsCount >= 1 ? starFull : starEmpty;
        star2.sprite = starsCount >= 2 ? starFull : starEmpty;
        star3.sprite = starsCount >= 3 ? starFull : starEmpty;
        
        
    }
    
    public int GetEarnedStars()
    {
        return earnedStars;
    }
}