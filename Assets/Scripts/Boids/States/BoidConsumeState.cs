using UnityEngine;

public class BoidConsumeState : IState
{
    private BoidAgent _boid;
    private float _consumeTimer;
    private Color _originalColor;
    private Renderer _renderer;

    public BoidConsumeState(BoidAgent boid)
    {
        _boid = boid;
        _renderer = _boid.GetComponent<Renderer>();
    }

    public void Enter()
    {
        _consumeTimer = 0f;

        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
            _renderer.material.color = new Color(1f, 0.64f, 0f);
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

        _boid.StopMovement();        
        _consumeTimer += Time.deltaTime;
        if (_consumeTimer >= _boid.ConsumeInterval)
        {
            _consumeTimer = 0f;
            bool isDestroyed = _boid.DamageTargetObject(_boid.TargetObject); 
            if (isDestroyed)
            {
                _boid.TargetObject = null;
                _boid.StateMachine.ChangeState(_boid.FlockingState);
            }
        }
    }

    public void Exit()
    {
        if (_renderer != null) _renderer.material.color = _originalColor;
    }
}