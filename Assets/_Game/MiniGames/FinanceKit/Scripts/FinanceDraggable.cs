using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LCT.MiniGames.Finance
{
    public class FinanceDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public int Value;
        public bool Locked;
        public Transform Home;

        public Func<FinanceDraggable, FinanceDropTarget, bool> Dropped;
        public Action<FinanceDropTarget> Hovered;

        RectTransform _rect;
        CanvasGroup _group;
        RectTransform _dragLayer;
        MonoBehaviour _host;
        bool _dragging;

        public RectTransform Rect => _rect;

        public void Setup(int value, Transform home, RectTransform dragLayer, MonoBehaviour host)
        {
            Value = value;
            Home = home;
            _dragLayer = dragLayer;
            _host = host;
            _rect = (RectTransform)transform;
            _group = GetComponent<CanvasGroup>();
            if (_group == null)
                _group = gameObject.AddComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Locked || _dragLayer == null)
            {
                eventData.pointerDrag = null;
                return;
            }

            _dragging = true;
            transform.SetParent(_dragLayer, true);
            transform.SetAsLastSibling();
            transform.localScale = Vector3.one * 1.12f;
            _group.blocksRaycasts = false;
            Follow(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            Follow(eventData);
            Hovered?.Invoke(FindTarget(eventData));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            _dragging = false;
            _group.blocksRaycasts = true;
            transform.localScale = Vector3.one;
            Hovered?.Invoke(null);

            FinanceDropTarget target = FindTarget(eventData);
            bool accepted = target != null && Dropped != null && Dropped(this, target);
            if (!accepted)
                ReturnHome();
        }

        public void ReturnHome()
        {
            transform.SetParent(Home, true);
            _rect.anchorMin = _rect.anchorMax = new Vector2(0.5f, 0.5f);
            FinanceTween.Move(_host, _rect, Vector2.zero, 0.22f);
        }

        public void SnapHome()
        {
            _dragging = false;
            if (_group != null)
                _group.blocksRaycasts = true;
            transform.SetParent(Home, false);
            transform.localScale = Vector3.one;
            _rect.anchorMin = _rect.anchorMax = new Vector2(0.5f, 0.5f);
            _rect.anchoredPosition = Vector2.zero;
        }

        void Follow(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_dragLayer, eventData.position, eventData.pressEventCamera, out Vector3 world))
                _rect.position = world;
        }

        static FinanceDropTarget FindTarget(PointerEventData eventData)
        {
            GameObject hit = eventData.pointerCurrentRaycast.gameObject;
            return hit != null ? hit.GetComponentInParent<FinanceDropTarget>() : null;
        }
    }
}
