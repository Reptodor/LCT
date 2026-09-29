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
    Vector3[] _rooms = System.Array.Empty<Vector3>();
    float[] _roomWidths = System.Array.Empty<float>();
    string[] _roomIds = System.Array.Empty<string>();
    int _roomIndex;
    Vector3 _focus;
    Vector3 _camFrom;
    Vector3 _camGoal;
    float _widthFrom;
    float _widthGoal;
    float _camT;
    float _viewWidth = RoomViewWidth;
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

    public string CurrentRoomId
    {
        get
        {
            if (_roomIds == null || _roomIndex < 0 || _roomIndex >= _roomIds.Length)
            {
                return "";
            }

            return _roomIds[_roomIndex] ?? "";
        }
    }

    public Vector3 RoomCenter => _rooms.Length > 0 ? _rooms[_roomIndex] : transform.position;

    public float RoomReach => _rooms.Length > 0 ? Mathf.Max(1.35f, _roomWidths[_roomIndex] * 0.28f) : 1.8f;

    public bool Contains(Vector3 point)
    {
        if (_rooms.Length == 0)
        {
            return true;
        }

        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < _rooms.Length; i++)
        {
            float dx = point.x - _rooms[i].x;
            float dz = point.z - _rooms[i].z;
            float dist = dx * dx + dz * dz;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }

        return best == _roomIndex;
    }

    public static void Attach(GameObject pet, Camera cam, Vector3[] rooms, float[] viewWidths, int startIndex, string[] roomIds)
    {
        var walk = pet.GetComponent<PetWalk>();
        if (walk == null)
        {
            walk = pet.AddComponent<PetWalk>();
        }

        walk.Setup(cam, rooms, viewWidths, startIndex, roomIds);
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

    void Setup(Camera cam, Vector3[] rooms, float[] viewWidths, int startIndex, string[] roomIds)
    {
        _cam = cam;
        _rooms = rooms != null && rooms.Length > 0 ? rooms : System.Array.Empty<Vector3>();
        _roomIds = roomIds != null && roomIds.Length == _rooms.Length
            ? roomIds
            : new string[_rooms.Length];
        _roomWidths = viewWidths != null && viewWidths.Length == _rooms.Length
            ? viewWidths
            : new float[_rooms.Length];
        if (viewWidths == null || viewWidths.Length != _rooms.Length)
        {
            for (int i = 0; i < _roomWidths.Length; i++)
            {
                _roomWidths[i] = RoomViewWidth;
            }
        }

        _roomIndex = _rooms.Length == 0 ? 0 : Mathf.Clamp(startIndex, 0, _rooms.Length - 1);
        _viewWidth = _rooms.Length > 0 ? Mathf.Max(3f, _roomWidths[_roomIndex]) : RoomViewWidth;
        _focus = _rooms.Length > 0 ? _rooms[_roomIndex] : new Vector3(0f, FocusY, 0f);
        _camMoving = false;
        FitZoom(true);
        ApplyCamera();
        if (GetComponent<PetWander>() == null)
        {
            gameObject.AddComponent<PetWander>();
        }
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
        _fitZoom = _viewWidth / (2f * aspect);
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
            GoToIndex(_roomIndex - 1);
        }

        if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            GoToIndex(_roomIndex + 1);
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

        GoToIndex(dx < 0f ? _roomIndex + 1 : _roomIndex - 1);
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

    void GoToIndex(int index)
    {
        if (_rooms.Length == 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, _rooms.Length - 1);
        if (index == _roomIndex && !_camMoving)
        {
            return;
        }

        _roomIndex = index;
        _camFrom = _focus;
        _camGoal = _rooms[index];
        _widthFrom = _viewWidth;
        _widthGoal = Mathf.Max(3f, _roomWidths[index]);
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
        _focus = Vector3.Lerp(_camFrom, _camGoal, u);
        _viewWidth = Mathf.Lerp(_widthFrom, _widthGoal, u);
        float aspect = Mathf.Max(0.35f, (float)_screenW / Mathf.Max(1, _screenH));
        float zoomScale = _fitZoom > 0.001f ? _zoom / _fitZoom : 1f;
        _fitZoom = _viewWidth / (2f * aspect);
        if (!_pinching)
        {
            _zoom = Mathf.Clamp(_fitZoom * zoomScale, _fitZoom * ZoomIn, _fitZoom * ZoomOut);
        }
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
        var focus = _focus;
        _cam.orthographicSize = _zoom;
        _cam.transform.SetPositionAndRotation(focus - lookDir * Distance, Quaternion.Euler(_pitch, 0f, 0f));
    }
}
