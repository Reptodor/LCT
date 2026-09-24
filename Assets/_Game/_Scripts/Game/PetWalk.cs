using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PetWalk : MonoBehaviour
{
    const float FocusY = 0.4f;
    const float Pitch = 56f;
    const float Distance = 14f;
    const float Speed = 2.4f;
    const float SwipePixels = 70f;
    const float CamStart = 0.78f;

    Camera _cam;
    float _livingX;
    float _bathX;
    float _fromX;
    float _toX;
    float _focusX;
    float _camHoldX;
    float _camGoalX;
    float _duration;
    float _walkT;
    bool _walking;
    bool _dragging;
    Vector2 _dragStart;

    public static void Attach(GameObject pet, Camera cam, float livingX, float bathX)
    {
        var walk = pet.GetComponent<PetWalk>();
        if (walk == null)
        {
            walk = pet.AddComponent<PetWalk>();
        }

        walk.Setup(cam, livingX, bathX);
    }

    void Setup(Camera cam, float livingX, float bathX)
    {
        _cam = cam;
        _livingX = livingX;
        _bathX = bathX;
        _focusX = transform.position.x;
        _walking = false;
        FollowCamera();
    }

    void Update()
    {
        ReadKeys();
        ReadPointer();
        Step();
    }

    void LateUpdate()
    {
        FollowCamera();
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

    void ReadPointer()
    {
        var touch = Touchscreen.current;
        if (touch != null)
        {
            var press = touch.primaryTouch.press;
            if (press.wasPressedThisFrame)
            {
                BeginDrag(touch.primaryTouch.position.ReadValue(), touch.primaryTouch.touchId.ReadValue());
            }
            else if (_dragging && press.wasReleasedThisFrame)
            {
                EndDrag(touch.primaryTouch.position.ReadValue());
            }
        }

        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            BeginDrag(mouse.position.ReadValue(), -1);
        }
        else if (_dragging && mouse.leftButton.wasReleasedThisFrame)
        {
            EndDrag(mouse.position.ReadValue());
        }
    }

    void BeginDrag(Vector2 position, int pointerId)
    {
        if (IsOverUi(pointerId))
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
        if (Mathf.Abs(dx) < SwipePixels)
        {
            return;
        }

        GoTo(dx < 0f ? _bathX : _livingX);
    }

    static bool IsOverUi(int pointerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    void GoTo(float x)
    {
        _fromX = transform.position.x;
        _toX = x;
        float span = Mathf.Abs(_toX - _fromX);
        if (span < 0.05f)
        {
            _walking = false;
            return;
        }

        _duration = Mathf.Max(0.35f, span / Speed);
        _walkT = 0f;
        _camHoldX = _focusX;
        _camGoalX = x;
        _walking = true;
    }

    void Step()
    {
        if (!_walking)
        {
            return;
        }

        _walkT += Time.deltaTime / _duration;
        float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_walkT));
        var position = transform.position;
        position.x = Mathf.Lerp(_fromX, _toX, u);
        transform.position = position;

        float span = Mathf.Abs(_toX - _fromX);
        float along = span < 0.001f ? 1f : Mathf.Abs(position.x - _fromX) / span;
        if (along < CamStart)
        {
            _focusX = _camHoldX;
        }
        else
        {
            float camU = Mathf.SmoothStep(0f, 1f, (along - CamStart) / (1f - CamStart));
            _focusX = Mathf.Lerp(_camHoldX, _camGoalX, camU);
        }

        if (_walkT >= 1f)
        {
            _walking = false;
        }
    }

    void FollowCamera()
    {
        if (_cam == null)
        {
            return;
        }

        float rad = Pitch * Mathf.Deg2Rad;
        var lookDir = new Vector3(0f, -Mathf.Sin(rad), Mathf.Cos(rad));
        var focus = new Vector3(_focusX, FocusY, 0f);
        _cam.transform.SetPositionAndRotation(focus - lookDir * Distance, Quaternion.Euler(Pitch, 0f, 0f));
    }
}
