using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LCT.MiniGames.Finance
{
    public class FinanceDragSurface : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public Action<PointerEventData> Moved;

        public void OnPointerDown(PointerEventData eventData)
        {
            Moved?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Moved?.Invoke(eventData);
        }
    }
}
