using UnityEngine;

namespace LCT.MiniGames.Finance
{
    public class FinanceTheme : MonoBehaviour
    {
        [Header("Шрифты")]
        public Font regularFont;
        public Font boldFont;
        public Font heavyFont;

        [Header("Фон и кнопки")]
        public Sprite background;
        public Sprite playButton;
        public Sprite nextButton;
        public Sprite restartButton;
        public Sprite exitButton;
        public Sprite starFull;
        public Sprite starEmpty;
        public Sprite checkMark;
        public Sprite moneyPile;

        [Header("Монеты")]
        public Sprite coin1;
        public Sprite coin2;
        public Sprite coin5;
        public Sprite coin10;

        [Header("Купюры")]
        public Sprite banknote50;
        public Sprite banknote100;
        public Sprite banknote200;
        public Sprite banknote500;
        public Sprite banknote1000;
        public Sprite banknote5000;

        public static bool IsCoin(int value)
        {
            return value <= 10;
        }

        public Sprite GetMoneySprite(int value)
        {
            switch (value)
            {
                case 1: return coin1;
                case 2: return coin2;
                case 5: return coin5;
                case 10: return coin10;
                case 50: return banknote50;
                case 100: return banknote100;
                case 200: return banknote200;
                case 500: return banknote500;
                case 1000: return banknote1000;
                case 5000: return banknote5000;
                default: return null;
            }
        }
    }
}
