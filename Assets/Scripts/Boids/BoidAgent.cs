using UnityEngine;
using System.Collections.Generic;

public class BoidAgent : Agent
{
    [Header("Boid Stats")]
    [SerializeField] private float _maxSpeed = 4f;
    [SerializeField] private float _maxSteering = 3f;
    [SerializeField] private int _maxHealth = 2;
    private int _currentHealth;

    [Header("Perception Radii & Layers")]
    [SerializeField] private float _separationRadius = 2f;
    [SerializeField] private float _alignmentCohesionRadius = 6f;
    [SerializeField] private float _visionRadius = 10f;
    [SerializeField] private float _minDistanceToConsume = 1f;
    [SerializeField] private LayerMask _hunterLayer;
    [SerializeField] private LayerMask _objectLayer;

    [Header("Flocking Weights")]
    [SerializeField, Range(0, 5f)] private float separationWeight = 1.5f;
    [SerializeField, Range(0, 5f)] private float cohesionWeight = 1f;
    [SerializeField, Range(0, 5f)] private float alignmentWeight = 1f;

    [Header("Behavior Timings")]
    [SerializeField] private float _consumeInterval = 1f;
    [SerializeField] private float _respawnTime = 3f;

    public static List<BoidAgent> boidsList = new List<BoidAgent>();
    
    public StateMachine StateMachine { get; private set; }
    public BoidFlockingState FlockingState { get; private set; }
    public BoidEvadeState EvadeState { get; private set; }
    public BoidSeekObjectState SeekObjectState { get; private set; }
    public BoidConsumeState ConsumeState { get; private set; }
    public BoidDeadState DeadState { get; private set; }
    public BoidRespawnState RespawnState { get; private set; }
    public Transform TargetObject { get; set; }

    public float MinDistanceToConsume => _minDistanceToConsume;
    public float ConsumeInterval => _consumeInterval;
    public float RespawnTime => _respawnTime;
    public int CurrentHealth => _currentHealth;

    private Collider[] _colls;
    private Renderer _renderer;

    private void Awake()
    {
        _currentHealth = _maxHealth;
        _renderer = GetComponent<Renderer>();
        _colls = GetComponentsInChildren<Collider>();

        if (!boidsList.Contains(this)) boidsList.Add(this);

        StateMachine = new StateMachine();
        FlockingState = new BoidFlockingState(this);
        EvadeState = new BoidEvadeState(this);
        SeekObjectState = new BoidSeekObjectState(this);
        ConsumeState = new BoidConsumeState(this);
        DeadState = new BoidDeadState(this);
        RespawnState = new BoidRespawnState(this);

        Vector3 randomVector = new(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _velocity += randomVector.normalized * _maxSpeed;

        // --- INITIAL STATE: FLOCKING ---
        StateMachine.Initialize(FlockingState);
    }

    private void OnDestroy()
    {
        if (boidsList.Contains(this)) boidsList.Remove(this);
    }

    private void Update()
    {
        StateMachine.Update();

        if (Bounds.Instance != null && StateMachine.CurrentState != RespawnState)
        {
            transform.position = Bounds.Instance.OutOfBounds(transform.position);
        }
    }

    public void ApplyMovement(Vector3 steering)
    {
        _velocity += steering;
        _velocity = Vector3.ClampMagnitude(_velocity, _maxSpeed);
        transform.position += _velocity * Time.deltaTime;
        if (_velocity != Vector3.zero) transform.forward = _velocity;
    }

    public void StopMovement() => _velocity = Vector3.zero;

    // --- FLOCKING STATE ---
    public Vector3 CalculateFlocking()
    {
        Vector3 separation = CalculateSeparation(boidsList, _separationRadius) * separationWeight;
        Vector3 alignment = CalculateAlignment(boidsList, _alignmentCohesionRadius) * alignmentWeight;
        Vector3 cohesion = CalculateCohesion(boidsList, _alignmentCohesionRadius) * cohesionWeight;
        return separation + alignment + cohesion;
    }

    public Vector3 CalculateSeparationForce()
    {
        return CalculateSeparation(boidsList, _separationRadius) * separationWeight;
    }

    private Vector3 CalculateSeparation(IEnumerable<BoidAgent> agents, float radius)
    {
        Vector3 desired = default;
        int count = 0;
        foreach (var agent in agents)
        {
            if (agent == this || agent.CurrentHealth <= 0) continue;

            if (InRange(agent.transform.position, radius))
            {
                desired += agent.transform.position - transform.position;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;

        desired /= count;
        return CalculateSteering(-desired.normalized * _maxSpeed);
    }

    private Vector3 CalculateAlignment(IEnumerable<BoidAgent> agents, float radius)
    {
        Vector3 desired = default;
        int count = 0;
        foreach (var agent in agents)
        {
            if (agent == this || agent.CurrentHealth <= 0) continue;

            if (InRange(agent.transform.position, radius))
            {
                desired += agent.Velocity;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;

        desired /= count;
        return CalculateSteering(desired.normalized * _maxSpeed);
    }

    private Vector3 CalculateCohesion(IEnumerable<BoidAgent> agents, float radius)
    {
        Vector3 desired = default;
        int count = 0;
        foreach (var agent in agents)
        {
            if (agent == this || agent.CurrentHealth <= 0) continue;

            if (InRange(agent.transform.position, radius))
            {
                desired += agent.transform.position;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;

        desired /= count;
        Vector3 targetDir = (desired - transform.position).normalized * _maxSpeed;
        return CalculateSteering(targetDir);
    }

    // --- SEEK STATE ---
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

    // --- EVADE STATE ---
    public Vector3 CalculateEvade(Transform hunterTarget)
    {
        Vector3 direction = hunterTarget.position - transform.position;
        float distance = direction.magnitude;
        Vector3 hunterVelocity = hunterTarget.TryGetComponent<IVelocityProvider>(out var provider) ? provider.Velocity : Vector3.zero;
        float prediction = distance / (_maxSpeed + hunterVelocity.magnitude);
        Vector3 futurePosition = hunterTarget.position + hunterVelocity * prediction;
        Vector3 desired = (futurePosition - transform.position).normalized * _maxSpeed;
        return CalculateSteering(-desired);
    }

    private Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxSteering * Time.deltaTime);
        return steering;
    }

    private bool InRange(Vector3 pos, float radius) => (pos - transform.position).sqrMagnitude <= radius * radius;

    public bool DetectHunter(out Transform hunterTransform)
    {
        hunterTransform = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, _visionRadius, _hunterLayer);  
        if (hits.Length > 0)
        {
            hunterTransform = hits[0].transform;
            return true;
        }

        return false;
    }

    public bool DetectObjectOfInterest(out Transform objectTransform)
    {
        objectTransform = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, _visionRadius, _objectLayer);
        if (hits.Length > 0)
        {
            objectTransform = hits[0].transform;
            return true;
        }

        return false;
    }

    // --- DEAD STATE ---
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            StateMachine.ChangeState(DeadState);
        }
    }

    // --- CONSUME STATE ---
    public bool DamageTargetObject(Transform target)
    {
        if (target.TryGetComponent<ResourceObject>(out var resource)) return resource.TakeDamage(1);

        Destroy(target.gameObject);
        return true;
    }

    public void DisablePhysicsAndCollisions()
    {
        foreach (var col in _colls) col.enabled = false;
    }

    // --- RESPAWN STATE ---
    public void HideAgent()
    {
        if (_renderer != null) _renderer.enabled = false;

        foreach (var col in _colls)
        {
            if (col != null) col.enabled = false;
        }
    }

    public void ShowAgent()
    {
        if (_renderer != null) _renderer.enabled = true;

        foreach (var col in _colls)
        {
            if (col != null) col.enabled = true;
        }
    }

    public void RespawnInRandomPosition()
    {
        _currentHealth = _maxHealth;
        if (Bounds.Instance != null)
        {
            float randomX = Random.Range(-20f, 20f); 
            float randomZ = Random.Range(-20f, 20f);
            transform.position = new Vector3(randomX, 1f, randomZ);
        }

        Vector3 randomVector = new(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _velocity = randomVector.normalized * _maxSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _separationRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _alignmentCohesionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _visionRadius);
    }
}