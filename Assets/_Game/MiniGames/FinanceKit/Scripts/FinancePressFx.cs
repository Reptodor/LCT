using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LCT.MiniGames.Finance
{
    public class FinancePressFx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        const float PressedScale = 0.93f;
        const float Speed = 2.5f;

        Selectable _selectable;
        float _target = 1f;
        bool _active;

        void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_selectable != null && !_selectable.IsInteractable())
                return;

            _target = PressedScale;
            _active = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _target = 1f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _target = 1f;
        }

        void OnDisable()
        {
            _target = 1f;
            _active = false;
            transform.localScale = Vector3.one;
        }

        void Update()
        {
            if (!_active)
                return;

            float scale = Mathf.MoveTowards(transform.localScale.x, _target, Time.unscaledDeltaTime * Speed);
            transform.localScale = new Vector3(scale, scale, 1f);
            if (Mathf.Approximately(scale, 1f) && Mathf.Approximately(_target, 1f))
                _active = false;
        }
    }
}
