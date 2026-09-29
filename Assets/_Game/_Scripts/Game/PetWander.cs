using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PetWander : MonoBehaviour
{
    static readonly string[] Hangouts =
    {
        "televisionModern",
        "loungeSofaCorner",
        "loungeChairRelax",
        "tableCoffee",
        "bookcaseClosed",
        "kitchenFridge",
        "bedSingle",
        "deskCorner",
        "toilet",
        "bathtub",
        "tableCrossCloth"
    };

    static NavMeshDataInstance _nav;

    const float WanderSpeed = 0.75f;
    const float RushSpeed = 1.85f;
    const string StandingParam = "is_standing";
    const string WalkingParam = "is_walking";

    NavMeshAgent _agent;
    Animator _animator;
    PetWalk _walk;
    float _wait;
    bool _moving;
    string _room = "";
    DoorGate _door;
    int _trip;
    float _hold;
    readonly List<DoorGate> _doors = new List<DoorGate>();

    public static void BuildNav(GameObject house)
    {
        if (house == null)
        {
            return;
        }

        if (_nav.valid)
        {
            NavMesh.RemoveNavMeshData(_nav);
        }

        var sources = new List<NavMeshBuildSource>();
        var renderers = house.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(house.transform.position, Vector3.one);
        for (int i = 0; i < renderers.Length; i++)
        {
            Bounds box = renderers[i].bounds;
            bounds.Encapsulate(box);
            if (box.size.x < 2f || box.size.z < 2f)
            {
                continue;
            }

            AddFloor(sources, box.center.x, box.min.y + 0.04f, box.center.z, box.size.x * 0.92f, box.size.z * 0.92f);
        }

        var all = house.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (!all[i].name.StartsWith("doorway"))
            {
                continue;
            }

            Vector3 door = all[i].position;
            AddFloor(sources, door.x, door.y + 0.04f, door.z, 1.5f, 1.5f);
        }

        if (sources.Count == 0)
        {
            return;
        }

        var settings = NavMesh.GetSettingsByID(0);
        settings.agentRadius = 0.12f;
        settings.agentHeight = 0.4f;
        settings.agentSlope = 40f;
        settings.agentClimb = 0.2f;
        settings.overrideVoxelSize = true;
        settings.voxelSize = 0.05f;

        bounds.Expand(2f);
        var local = new Bounds(Vector3.zero, bounds.size);
        var data = NavMeshBuilder.BuildNavMeshData(settings, sources, local, bounds.center, Quaternion.identity);
        if (data != null)
        {
            _nav = NavMesh.AddNavMeshData(data);
        }
    }

    static void AddFloor(List<NavMeshBuildSource> sources, float x, float y, float z, float width, float depth)
    {
        var source = new NavMeshBuildSource();
        source.shape = NavMeshBuildSourceShape.Box;
        source.area = 0;
        source.size = new Vector3(width, 0.08f, depth);
        source.transform = Matrix4x4.TRS(new Vector3(x, y, z), Quaternion.identity, Vector3.one);
        sources.Add(source);
    }

    void Awake()
    {
        _walk = GetComponent<PetWalk>();
        _wait = Random.Range(0.4f, 1.4f);
    }

    void EnsureAgent()
    {
        if (_agent != null || !_nav.valid)
        {
            return;
        }

        _agent = gameObject.AddComponent<NavMeshAgent>();
        _agent.speed = 0.75f;
        _agent.angularSpeed = 280f;
        _agent.acceleration = 8f;
        _agent.stoppingDistance = 0.12f;
        _agent.radius = 0.12f;
        _agent.height = 0.4f;
        _agent.baseOffset = FeetOffset();
        _agent.autoBraking = true;
        _agent.updateRotation = true;
    }

    float FeetOffset()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        bool any = false;
        Bounds bounds = default;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || !renderers[i].enabled)
            {
                continue;
            }

            if (!any)
            {
                bounds = renderers[i].bounds;
                any = true;
            }
            else
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        if (!any)
        {
            return 0.5f;
        }

        float drop = transform.position.y - bounds.min.y;
        return Mathf.Max(0.05f, drop) + 0.06f;
    }

    void Start()
    {
        EnsureAgent();
        SnapToMesh();
        _room = _walk != null ? _walk.CurrentRoomId : "";
    }

    void Update()
    {
        if (!_nav.valid)
        {
            return;
        }

        if (_agent == null)
        {
            EnsureAgent();
            if (_agent == null)
            {
                return;
            }

            SnapToMesh();
        }

        if (!_agent.isOnNavMesh)
        {
            SnapToMesh();
            return;
        }

        string room = _walk != null ? _walk.CurrentRoomId : "";
        StepDoors();
        if (room != _room)
        {
            _room = room;
            BeginRoomTrip();
        }

        if (_trip > 0)
        {
            StepTrip();
            return;
        }

        if (_moving)
        {
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            {
                _moving = false;
                _agent.ResetPath();
                _wait = Random.Range(1.1f, 3.6f);
            }

            return;
        }

        _wait -= Time.deltaTime;
        if (_wait > 0f)
        {
            return;
        }

        if (Random.value < 0.28f)
        {
            _wait = Random.Range(0.7f, 2.1f);
            return;
        }

        if (TryDestination(out Vector3 next))
        {
            _agent.SetDestination(next);
            _agent.speed = WanderSpeed;
            _moving = true;
        }
        else
        {
            _wait = 0.6f;
        }
    }

    void LateUpdate()
    {
        DriveAnimation();
    }

    public void RefreshBody()
    {
        if (_agent == null)
        {
            EnsureAgent();
        }

        if (_agent == null)
        {
            return;
        }

        _agent.baseOffset = FeetOffset();
        SnapToMesh();
    }

    void DriveAnimation()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
            {
                return;
            }
        }

        bool walking = IsWalking();
        bool standing = walking || _trip == 2 || _trip == 4;
        _animator.SetBool(StandingParam, standing);
        _animator.SetBool(WalkingParam, walking);
        float pace = 1f;
        if (walking && _agent != null)
        {
            pace = Mathf.Clamp(_agent.velocity.magnitude / WanderSpeed, 0.85f, 1.7f);
        }

        _animator.speed = pace;
    }

    bool IsWalking()
    {
        if (_agent == null || !_moving || _agent.speed <= 0.05f)
        {
            return false;
        }

        if (_agent.pathPending)
        {
            return true;
        }

        return _agent.velocity.sqrMagnitude > 0.008f
            || (_agent.hasPath && _agent.remainingDistance > _agent.stoppingDistance + 0.15f);
    }

    bool TryDestination(out Vector3 destination)
    {
        Vector3 center = _walk != null ? _walk.RoomCenter : transform.position;
        float reach = _walk != null ? _walk.RoomReach : 2.2f;
        if (Random.value < 0.5f && TryHangout(center, reach, out destination))
        {
            return true;
        }

        for (int i = 0; i < 8; i++)
        {
            Vector2 ring = Random.insideUnitCircle * reach;
            var guess = new Vector3(center.x + ring.x, center.y, center.z + ring.y);
            if (NavMesh.SamplePosition(guess, out NavMeshHit hit, 1.6f, NavMesh.AllAreas) && InsideRoom(hit.position))
            {
                destination = hit.position;
                return true;
            }
        }

        destination = transform.position;
        return false;
    }

    bool TryHangout(Vector3 center, float reach, out Vector3 destination)
    {
        destination = transform.position;
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var picks = new List<Vector3>();
        float limit = reach + 1.4f;
        for (int i = 0; i < all.Length; i++)
        {
            Transform item = all[i];
            bool named = false;
            for (int n = 0; n < Hangouts.Length; n++)
            {
                if (item.name == Hangouts[n])
                {
                    named = true;
                    break;
                }
            }

            if (!named)
            {
                continue;
            }

            float dx = item.position.x - center.x;
            float dz = item.position.z - center.z;
            if (dx * dx + dz * dz > limit * limit)
            {
                continue;
            }

            Vector2 side = Random.insideUnitCircle.normalized * Random.Range(0.35f, 0.7f);
            picks.Add(item.position + new Vector3(side.x, 0f, side.y));
        }

        if (picks.Count == 0)
        {
            return false;
        }

        Vector3 guess = picks[Random.Range(0, picks.Count)];
        if (!NavMesh.SamplePosition(guess, out NavMeshHit hit, 1.2f, NavMesh.AllAreas) || !InsideRoom(hit.position))
        {
            return false;
        }

        destination = hit.position;
        return true;
    }

    void SnapToMesh()
    {
        Vector3 from = _walk != null ? _walk.RoomCenter : transform.position;
        from.y = transform.position.y;
        if (!NavMesh.SamplePosition(from, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            return;
        }

        _agent.Warp(hit.position);
    }

    bool InsideRoom(Vector3 point)
    {
        if (_walk != null && !_walk.Contains(point))
        {
            return false;
        }

        EnsureDoors();
        for (int i = 0; i < _doors.Count; i++)
        {
            Vector3 door = _doors[i].Frame.position;
            float dx = point.x - door.x;
            float dz = point.z - door.z;
            if (dx * dx + dz * dz < 0.75f * 0.75f)
            {
                return false;
            }
        }

        return true;
    }

    void BeginRoomTrip()
    {
        EnsureDoors();
        for (int i = 0; i < _doors.Count; i++)
        {
            _doors[i].WantOpen = false;
        }

        Vector3 center = _walk != null ? _walk.RoomCenter : transform.position;
        _door = ClosestDoor(transform.position, center);
        _agent.speed = RushSpeed;
        _agent.ResetPath();
        _trip = 1;
        _moving = true;
        if (!GoNear(DoorSide(_door, transform.position)))
        {
            _trip = 5;
            GoNear(center);
        }
    }

    void StepTrip()
    {
        Vector3 center = _walk != null ? _walk.RoomCenter : transform.position;
        if (_trip == 1)
        {
            if (!Arrived())
            {
                return;
            }

            _agent.ResetPath();
            _agent.speed = 0f;
            if (_door != null)
            {
                _door.WantOpen = true;
            }

            _trip = 2;
            return;
        }

        if (_trip == 2)
        {
            if (_door != null && !_door.IsOpen)
            {
                return;
            }

            _agent.speed = RushSpeed;
            _trip = 3;
            if (!GoNear(DoorSide(_door, center)))
            {
                _trip = 5;
                GoNear(center);
            }

            return;
        }

        if (_trip == 3)
        {
            if (!Arrived())
            {
                return;
            }

            _agent.ResetPath();
            _agent.speed = 0f;
            _hold = 0.65f;
            if (_door != null)
            {
                _door.WantOpen = false;
            }

            _trip = 4;
            return;
        }

        if (_trip == 4)
        {
            _hold -= Time.deltaTime;
            if (_door != null && !_door.IsClosed)
            {
                return;
            }

            if (_hold > 0f)
            {
                return;
            }

            _agent.speed = WanderSpeed;
            _trip = 5;
            if (!TryDestination(out Vector3 inside))
            {
                inside = center;
            }

            if (!GoNear(inside))
            {
                FinishTrip();
            }

            return;
        }

        if (!Arrived())
        {
            return;
        }

        FinishTrip();
    }

    bool Arrived()
    {
        return !_agent.pathPending && (!_agent.hasPath || _agent.remainingDistance <= _agent.stoppingDistance + 0.08f);
    }

    void FinishTrip()
    {
        _trip = 0;
        _moving = false;
        _agent.speed = WanderSpeed;
        _agent.ResetPath();
        _wait = Random.Range(0.6f, 1.6f);
        if (_door != null)
        {
            _door.WantOpen = false;
            _door = null;
        }
    }

    bool GoNear(Vector3 world)
    {
        if (!NavMesh.SamplePosition(world, out NavMeshHit hit, 1.4f, NavMesh.AllAreas))
        {
            return false;
        }

        _agent.SetDestination(hit.position);
        return true;
    }

    Vector3 DoorSide(DoorGate door, Vector3 toward)
    {
        if (door == null || door.Frame == null)
        {
            return toward;
        }

        Vector3 fromDoor = toward - door.Frame.position;
        fromDoor.y = 0f;
        if (fromDoor.sqrMagnitude < 0.01f)
        {
            fromDoor = door.Frame.forward;
        }

        fromDoor.Normalize();
        return door.Frame.position + fromDoor * 1.2f;
    }

    void EnsureDoors()
    {
        if (_doors.Count > 0)
        {
            return;
        }

        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (!all[i].name.StartsWith("doorway"))
            {
                continue;
            }

            Transform panel = null;
            for (int c = 0; c < all[i].childCount; c++)
            {
                Transform child = all[i].GetChild(c);
                if (IsDoorPanel(child))
                {
                    panel = child;
                    break;
                }
            }

            if (panel == null)
            {
                continue;
            }

            _doors.Add(new DoorGate(all[i], panel));
        }
    }

    DoorGate ClosestDoor(Vector3 from, Vector3 to)
    {
        DoorGate best = null;
        float bestDist = float.MaxValue;
        Vector3 mid = (from + to) * 0.5f;
        for (int i = 0; i < _doors.Count; i++)
        {
            Vector3 pos = _doors[i].Frame.position;
            float along = DistanceToSegment(pos, from, to);
            float midDist = (pos - mid).sqrMagnitude;
            float score = along * 4f + midDist;
            if (score < bestDist)
            {
                bestDist = score;
                best = _doors[i];
            }
        }

        return best;
    }

    static float DistanceToSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        ab.y = 0f;
        Vector3 ap = point - a;
        ap.y = 0f;
        float denom = ab.sqrMagnitude;
        float t = denom < 0.001f ? 0f : Mathf.Clamp01(Vector3.Dot(ap, ab) / denom);
        Vector3 closest = a + ab * t;
        closest.y = point.y;
        return (point - closest).sqrMagnitude;
    }

    void StepDoors()
    {
        for (int i = 0; i < _doors.Count; i++)
        {
            _doors[i].Step();
        }
    }

    static bool IsDoorPanel(Transform t)
    {
        string name = t.name.ToLowerInvariant();
        return name.StartsWith("door") && !name.StartsWith("doorway");
    }

    sealed class DoorGate
    {
        public readonly Transform Frame;
        readonly Transform _panel;
        readonly Quaternion _closed;
        readonly Quaternion _open;
        public bool WantOpen;

        public DoorGate(Transform frame, Transform panel)
        {
            Frame = frame;
            _panel = panel;
            _closed = panel.localRotation;
            _open = _closed * Quaternion.Euler(0f, 85f, 0f);
        }

        public void Step()
        {
            if (_panel == null)
            {
                return;
            }

            Quaternion goal = WantOpen ? _open : _closed;
            _panel.localRotation = Quaternion.RotateTowards(_panel.localRotation, goal, 220f * Time.deltaTime);
        }

        public bool IsOpen => _panel != null && Quaternion.Angle(_panel.localRotation, _open) < 8f;

        public bool IsClosed => _panel != null && Quaternion.Angle(_panel.localRotation, _closed) < 6f;
    }
}
