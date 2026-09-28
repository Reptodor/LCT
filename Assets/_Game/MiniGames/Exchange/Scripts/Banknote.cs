using UnityEngine;
using UnityEngine.UI;
namespace LCT.MiniGames.Exchange
{
    public class Banknote : MonoBehaviour
    {
        [Header("Настройки")]
        public int denomination = 5;

        [Header("Компоненты")]
        private Image banknoteImage;
        private Text valueText;
        private RectTransform rectTransform;

        void Start()
        {
            banknoteImage = GetComponent<Image>();
            valueText = GetComponentInChildren<Text>();
            rectTransform = GetComponent<RectTransform>();

            UpdateVisual();
        }

        public void SetDenomination(int value)
        {
            denomination = value;
            UpdateVisual();
        }

        public void SetColor(Color color)
        {
            if (banknoteImage != null)
                banknoteImage.color = color;
        }

        public void SetSize(Vector2 size)
        {
            if (rectTransform != null)
                rectTransform.sizeDelta = size;
        }

        void UpdateVisual()
        {
            if (valueText != null)
                valueText.text = denomination + " ₽";
        }
    }
}
