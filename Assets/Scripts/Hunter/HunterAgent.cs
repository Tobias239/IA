using UnityEngine;
using System.Collections.Generic;

public class HunterAgent : Agent
{
    [Header("Hunter Stats")]
    [SerializeField] private float _maxSpeed = 4.5f;
    [SerializeField] private float _maxSteering = 3.5f;
    [SerializeField] private int _attackDamage = 1;

    [Header("Perception & Spawning")]
    [SerializeField] private float _visionRadius = 12f;
    [SerializeField] private LayerMask _boidLayer;
    [SerializeField] private GameObject _resourceObjectPrefab;

    [Header("Patrol Settings")]
    [SerializeField] private List<Transform> _patrolWaypoints;

    public StateMachine StateMachine { get; private set; }
    public HunterPatrolState PatrolState { get; private set; }
    public HunterHuntState HuntState { get; private set; }
    public HunterGatherState GatherState { get; private set; }
    public HunterRestState RestState { get; private set; }
    public Transform TargetBoid { get; set; }

    public float VisionRadius => _visionRadius;

    private Renderer _renderer;
    private Collider[] _colls;
    private Color _gizmoColor = Color.green;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _colls = GetComponentsInChildren<Collider>();
        StateMachine = new StateMachine();
        List<Vector3> points = new List<Vector3>();

        if (_patrolWaypoints != null)
        {
            foreach (var t in _patrolWaypoints)
            {
                if (t != null) points.Add(t.position);
            }
        }

        PatrolState = new HunterPatrolState(this, points);
        HuntState = new HunterHuntState(this);
        GatherState = new HunterGatherState(this);
        RestState = new HunterRestState(this);

        // --- INITIAL STATE: PATROL ---
        StateMachine.Initialize(PatrolState);
    }

    private void Update()
    {
        StateMachine.Update();

        if (Bounds.Instance != null)
        {
            transform.position = Bounds.Instance.OutOfBounds(transform.position);
        }

        UpdateGizmoColor();
    }

    public void ApplyMovement(Vector3 steering)
    {
        _velocity += steering;
        _velocity = Vector3.ClampMagnitude(_velocity, _maxSpeed);
        transform.position += _velocity * Time.deltaTime;
        if (_velocity != Vector3.zero) transform.forward = _velocity;
    }

    public void StopMovement() => _velocity = Vector3.zero;

    // --- ARRIVE (PATROL & GATHER STATES) ---
    public Vector3 CalculateArrive(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        float distance = direction.magnitude;
        if (distance < 0.1f) return Vector3.zero;

        float slowingDistance = 3f;
        float targetSpeed = _maxSpeed * (distance / slowingDistance);
        float desiredSpeed = Mathf.Min(targetSpeed, _maxSpeed);
        Vector3 desired = direction.normalized * desiredSpeed;
        return CalculateSteering(desired);
    }

    // --- PURSUIT (HUNT STATE) ---
    public Vector3 CalculatePursuit(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;
        Vector3 targetVelocity = target.TryGetComponent<IVelocityProvider>(out var provider) ? provider.Velocity : Vector3.zero;
        float prediction = distance / (_maxSpeed + targetVelocity.magnitude);
        Vector3 futurePosition = target.position + targetVelocity * prediction;
        Vector3 desired = (futurePosition - transform.position).normalized * _maxSpeed;
        return CalculateSteering(desired);
    }

    private Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxSteering * Time.deltaTime);
        return steering;
    }

    public bool DetectLiveBoid(out Transform boidTransform)
    {
        boidTransform = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, _visionRadius, _boidLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<BoidAgent>(out var boid) && boid.CurrentHealth > 0)
            {
                boidTransform = boid.transform;
                return true;
            }
        }

        return false;
    }

    public bool DetectDeadBoid(out Transform boidTransform)
    {
        boidTransform = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, _visionRadius, _boidLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<BoidAgent>(out var boid) && boid.CurrentHealth <= 0)
            {
                boidTransform = boid.transform;
                return true;
            }
        }

        return false;
    }

    public bool IsBoidAlive(Transform boidTarget)
    {
        if (boidTarget != null && boidTarget.TryGetComponent<BoidAgent>(out var boid)) return boid.CurrentHealth > 0;

        return false;
    }

    // --- HUNT STATE ---
    public void AttackTargetBoid(Transform boidTarget)
    {
        if (boidTarget.TryGetComponent<BoidAgent>(out var boid))
        {
            boid.TakeDamage(_attackDamage);
        }
    }

    // --- GATHER STATE ---
    public void CollectTargetBoid(Transform boidTarget)
    {
        if (boidTarget != null)
        {
            Vector3 dropPosition = boidTarget.position;
            if (boidTarget.TryGetComponent<BoidAgent>(out var boid)) boid.StateMachine.ChangeState(boid.RespawnState);

            if (_resourceObjectPrefab != null) Instantiate(_resourceObjectPrefab, dropPosition, Quaternion.identity);
        }
    }

    private void UpdateGizmoColor()
    {
        if (DetectLiveBoid(out _)) _gizmoColor = Color.red;
        else if (DetectDeadBoid(out _)) _gizmoColor = Color.yellow;
        else _gizmoColor = Color.green;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _gizmoColor;
        Gizmos.DrawWireSphere(transform.position, _visionRadius);
    }
}