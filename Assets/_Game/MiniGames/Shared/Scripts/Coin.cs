using UnityEngine;
using UnityEngine.EventSystems;

namespace LCT.MiniGames.Shop
{
    public class Coin : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public int denomination;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 startPos;
        private Transform startParent;
        private bool isInDropZone;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        void Start()
        {
            startPos = rectTransform.anchoredPosition;
            startParent = transform.parent;
            isInDropZone = MoneyPlayfield.DropZone != null && transform.parent == MoneyPlayfield.DropZone;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.7f;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            Transform dropZone = MoneyPlayfield.DropZone;
            GameObject target = eventData.pointerCurrentRaycast.gameObject;
            bool overDrop = dropZone != null && target != null && target.transform.IsChildOf(dropZone);

            if (isInDropZone && overDrop)
            {
                transform.SetParent(dropZone);
                return;
            }

            if (isInDropZone && !overDrop)
            {
                ReturnToStart();
                return;
            }

            if (!isInDropZone && overDrop)
            {
                MoneyPlayfield.CoinDropped?.Invoke(this);
                isInDropZone = dropZone != null && transform.parent == dropZone;
            }
            else if (!overDrop)
            {
                ReturnToStart();
            }
        }

        public void ReturnToStart()
        {
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startPos;
            MoneyPlayfield.CoinRemoved?.Invoke(this);
            isInDropZone = false;
        }
    }
}
