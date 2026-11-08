using UnityEngine;

public class BoidFlockingState : IState
{
    private BoidAgent _boid;
    private Color _originalColor;
    private Renderer _renderer;

    public BoidFlockingState(BoidAgent boid)
    {
        _boid = boid;
        _renderer = _boid.GetComponent<Renderer>();
    }

    public void Enter()
    {
        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
            _renderer.material.color = Color.green;
        }
    }

    public void UpdateState()
    {
        Vector3 steering = _boid.CalculateFlocking();
        _boid.ApplyMovement(steering);
        if (_boid.DetectHunter(out Transform hunter))
        {
            _boid.StateMachine.ChangeState(_boid.EvadeState);
            return;
        }

        if (_boid.DetectObjectOfInterest(out Transform targetObject))
        {
            _boid.TargetObject = targetObject;
            _boid.StateMachine.ChangeState(_boid.SeekObjectState);
            return;
        }
    }

    public void Exit()
    {
        if (_renderer != null) _renderer.material.color = _originalColor;
    }
}