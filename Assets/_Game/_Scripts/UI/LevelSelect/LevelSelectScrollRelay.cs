using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LevelSelectScrollRelay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollRect _scroll;

    private void Awake()
    {
        if (_scroll == null)
        {
            _scroll = GetComponentInParent<ScrollRect>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_scroll == null)
        {
            return;
        }

        _scroll.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_scroll == null)
        {
            return;
        }

        _scroll.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_scroll == null)
        {
            return;
        }

        _scroll.OnEndDrag(eventData);
    }
}
