using UnityEngine;

public class BoidEvadeState : IState
{
    private BoidAgent _boid;
    private Color _originalColor;
    private Renderer _renderer;

    public BoidEvadeState(BoidAgent boid)
    {
        _boid = boid;
        _renderer = _boid.GetComponent<Renderer>();
    }

    public void Enter()
    {
        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
            _renderer.material.color = Color.red;
        }
    }

    public void UpdateState()
    {
        if (!_boid.DetectHunter(out Transform hunter))
        {
            _boid.StateMachine.ChangeState(_boid.FlockingState);
            return;
        }

        Vector3 evadeSteering = _boid.CalculateEvade(hunter);
        Vector3 separationSteering = _boid.CalculateSeparationForce();
        _boid.ApplyMovement(evadeSteering + separationSteering);
    }

    public void Exit()
    {
        if (_renderer != null) _renderer.material.color = _originalColor;
    }
}