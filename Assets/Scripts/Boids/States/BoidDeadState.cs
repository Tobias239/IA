using UnityEngine;

public class BoidDeadState : IState
{
    private BoidAgent _boid;
    private Color _originalColor;
    private Renderer _renderer;

    public BoidDeadState(BoidAgent boid)
    {
        _boid = boid;
        _renderer = _boid.GetComponent<Renderer>();
    }

    public void Enter()
    {
        _boid.StopMovement();
        _boid.DisablePhysicsAndCollisions();
        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
            _renderer.material.color = Color.gray;
        }
    }

    public void UpdateState()
    {
        
    }

    public void Exit()
    {
        if (_renderer != null) _renderer.material.color = _originalColor;
    }
}