using UnityEngine;

public class BoidRespawnState : IState
{
    private BoidAgent _boid;
    private float _respawnTimer;

    public BoidRespawnState(BoidAgent boid)
    {
        _boid = boid;
    }

    public void Enter()
    {
        _respawnTimer = 0f;
        _boid.HideAgent(); 
    }

    public void UpdateState()
    {
        _respawnTimer += Time.deltaTime;
        if (_respawnTimer >= _boid.RespawnTime)
        {
            _boid.RespawnInRandomPosition();
            _boid.StateMachine.ChangeState(_boid.FlockingState);
        }
    }

    public void Exit() 
    {
        _boid.ShowAgent(); 
    }
}