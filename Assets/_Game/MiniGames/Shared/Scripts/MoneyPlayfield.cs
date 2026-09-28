using System;
using UnityEngine;

namespace LCT.MiniGames.Shop
{
    public static class MoneyPlayfield
    {
        public static Transform DropZone { get; private set; }
        public static Action<Coin> CoinDropped { get; private set; }
        public static Action<Coin> CoinRemoved { get; private set; }
        public static Action<BanknoteCard> BanknoteDropped { get; private set; }
        public static Action<BanknoteCard> BanknoteRemoved { get; private set; }

        public static void Bind(
            Transform dropZone,
            Action<Coin> coinDropped,
            Action<Coin> coinRemoved,
            Action<BanknoteCard> banknoteDropped,
            Action<BanknoteCard> banknoteRemoved)
        {
            DropZone = dropZone;
            CoinDropped = coinDropped;
            CoinRemoved = coinRemoved;
            BanknoteDropped = banknoteDropped;
            BanknoteRemoved = banknoteRemoved;
        }

        public static void Unbind(Transform dropZone)
        {
            if (DropZone != dropZone)
                return;

            DropZone = null;
            CoinDropped = null;
            CoinRemoved = null;
            BanknoteDropped = null;
            BanknoteRemoved = null;
        }
    }
}
