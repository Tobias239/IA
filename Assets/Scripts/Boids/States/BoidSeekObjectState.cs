using UnityEngine;

public class BoidSeekObjectState : IState
{
    private BoidAgent _boid;
    private Color _originalColor;
    private Renderer _renderer;

    public BoidSeekObjectState(BoidAgent boid)
    {
        _boid = boid;
        _renderer = _boid.GetComponent<Renderer>();
    }

    public void Enter()
    {
        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
            _renderer.material.color = Color.yellow;
        }
    }

    public void UpdateState()
    {
        if (_boid.DetectHunter(out _))
        {
            _boid.StateMachine.ChangeState(_boid.EvadeState);
            return;
        }

        if (_boid.TargetObject == null)
        {
            _boid.StateMachine.ChangeState(_boid.FlockingState);
            return;
        }

        Vector3 arriveSteering = _boid.CalculateArrive(_boid.TargetObject.position);
        _boid.ApplyMovement(arriveSteering);
        float distanceToTarget = Vector3.Distance(_boid.transform.position, _boid.TargetObject.position);
        if (distanceToTarget <= _boid.MinDistanceToConsume) _boid.StateMachine.ChangeState(_boid.ConsumeState);
    }

    public void Exit()
    {
        if (_renderer != null) _renderer.material.color = _originalColor;
    }
}