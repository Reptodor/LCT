using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class PetWalk : MonoBehaviour
{
    const float FocusY = 0.4f;
    const float Pitch = 56f;
    const float PitchClose = 50f;
    const float Distance = 14f;
    const float SwipePixels = 48f;
    const float CamDuration = 0.45f;
    const float RoomViewWidth = 5.15f;
    const float ZoomIn = 0.92f;
    const float ZoomOut = 1.12f;
    const float PinchFeel = 0.7f;
    const float PitchEase = 0.28f;

    Camera _cam;
    float _livingX;
    float _bathX;
    float _focusX;
    float _camFromX;
    float _camGoalX;
    float _camT;
    float _zoom = 4.25f;
    float _fitZoom = 4.25f;
    float _pitch = Pitch;
    float _pitchVel;
    bool _camMoving;
    bool _dragging;
    bool _pinching;
    float _pinchStartDist;
    float _pinchStartZoom;
    Vector2 _dragStart;
    int _screenW;
    int _screenH;
    readonly List<RaycastResult> _hits = new List<RaycastResult>();

    public static void Attach(GameObject pet, Camera cam, float livingX, float bathX)
    {
        var walk = pet.GetComponent<PetWalk>();
        if (walk == null)
        {
            walk = pet.AddComponent<PetWalk>();
        }

        walk.Setup(cam, livingX, bathX);
    }

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Input.multiTouchEnabled = true;
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Setup(Camera cam, float livingX, float bathX)
    {
        _cam = cam;
        _livingX = livingX;
        _bathX = bathX;
        _focusX = livingX;
        _camMoving = false;
        FitZoom(true);
        ApplyCamera();
    }

    void Update()
    {
        FitZoom(false);
        var touches = Touch.activeTouches;
        if (touches.Count >= 2)
        {
            ReadPinch(touches);
        }
        else
        {
            _pinching = false;
            ReadKeys();
            ReadSwipe(touches);
            ReadWheel();
        }

        StepCamera();
    }

    void LateUpdate()
    {
        ApplyCamera();
    }

    void FitZoom(bool force)
    {
        if (!force && _screenW == Screen.width && _screenH == Screen.height)
        {
            return;
        }

        _screenW = Screen.width;
        _screenH = Mathf.Max(1, Screen.height);
        float aspect = Mathf.Max(0.35f, (float)_screenW / _screenH);
        _fitZoom = RoomViewWidth / (2f * aspect);
        _zoom = _fitZoom;
        _pitch = Pitch;
        _pitchVel = 0f;
    }

    void ReadPinch(IReadOnlyList<Touch> touches)
    {
        _dragging = false;
        var a = touches[0].screenPosition;
        var b = touches[1].screenPosition;
        float dist = Vector2.Distance(a, b);
        if (!_pinching)
        {
            _pinching = true;
            _pinchStartDist = Mathf.Max(24f, dist);
            _pinchStartZoom = _zoom;
            return;
        }

        float ratio = Mathf.Lerp(1f, dist / _pinchStartDist, PinchFeel);
        _zoom = Mathf.Clamp(_pinchStartZoom / Mathf.Max(0.2f, ratio), _fitZoom * ZoomIn, _fitZoom * ZoomOut);
    }

    void ReadWheel()
    {
        if (Touch.activeTouches.Count > 0)
        {
            return;
        }

        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f || HitsControl(mouse.position.ReadValue()))
        {
            return;
        }

        _zoom = Mathf.Clamp(_zoom - scroll * 0.06f, _fitZoom * ZoomIn, _fitZoom * ZoomOut);
    }

    void ReadKeys()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            GoTo(_bathX);
        }

        if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            GoTo(_livingX);
        }
    }

    void ReadSwipe(IReadOnlyList<Touch> touches)
    {
        if (touches.Count == 1)
        {
            var touch = touches[0];
            if (touch.began)
            {
                BeginDrag(touch.screenPosition);
            }
            else if (_dragging && touch.ended)
            {
                EndDrag(touch.screenPosition);
            }

            return;
        }

        if (Touchscreen.current != null)
        {
            return;
        }

        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            BeginDrag(mouse.position.ReadValue());
        }
        else if (_dragging && mouse.leftButton.wasReleasedThisFrame)
        {
            EndDrag(mouse.position.ReadValue());
        }
    }

    void BeginDrag(Vector2 position)
    {
        if (HitsControl(position))
        {
            _dragging = false;
            return;
        }

        _dragging = true;
        _dragStart = position;
    }

    void EndDrag(Vector2 position)
    {
        if (!_dragging)
        {
            return;
        }

        _dragging = false;
        float dx = position.x - _dragStart.x;
        float dy = position.y - _dragStart.y;
        if (Mathf.Abs(dx) < SwipePixels || Mathf.Abs(dx) < Mathf.Abs(dy))
        {
            return;
        }

        GoTo(dx < 0f ? _bathX : _livingX);
    }

    bool HitsControl(Vector2 screenPos)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };
        _hits.Clear();
        EventSystem.current.RaycastAll(pointer, _hits);
        for (int i = 0; i < _hits.Count; i++)
        {
            if (_hits[i].gameObject.GetComponentInParent<Selectable>() != null)
            {
                return true;
            }
        }

        return false;
    }

    void GoTo(float x)
    {
        if (Mathf.Abs(_focusX - x) < 0.05f && !_camMoving)
        {
            return;
        }

        _camFromX = _focusX;
        _camGoalX = x;
        _camT = 0f;
        _camMoving = true;
    }

    void StepCamera()
    {
        if (!_camMoving)
        {
            return;
        }

        _camT += Time.deltaTime / CamDuration;
        float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_camT));
        _focusX = Mathf.Lerp(_camFromX, _camGoalX, u);
        if (_camT >= 1f)
        {
            _camMoving = false;
        }
    }

    void ApplyCamera()
    {
        if (_cam == null)
        {
            return;
        }

        float close = _fitZoom * ZoomIn;
        float tilt = Mathf.Clamp01(Mathf.InverseLerp(_fitZoom, close, _zoom));
        float targetPitch = Mathf.Lerp(Pitch, PitchClose, tilt);
        _pitch = Mathf.SmoothDamp(_pitch, targetPitch, ref _pitchVel, PitchEase);
        float rad = _pitch * Mathf.Deg2Rad;
        var lookDir = new Vector3(0f, -Mathf.Sin(rad), Mathf.Cos(rad));
        var focus = new Vector3(_focusX, FocusY, 0f);
        _cam.orthographicSize = _zoom;
        _cam.transform.SetPositionAndRotation(focus - lookDir * Distance, Quaternion.Euler(_pitch, 0f, 0f));
    }
}
