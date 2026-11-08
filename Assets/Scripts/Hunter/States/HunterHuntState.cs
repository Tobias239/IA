using UnityEngine;

public class HunterHuntState : IState
{
    private HunterAgent _hunter;
    private float _attackRange = 1.5f;

    public HunterHuntState(HunterAgent hunter)
    {
        _hunter = hunter;
    }

    public void Enter()
    {
        
    }

    public void UpdateState()
    {
        if (_hunter.TargetBoid == null || !_hunter.IsBoidAlive(_hunter.TargetBoid))
        {
            _hunter.TargetBoid = null;
            _hunter.StateMachine.ChangeState(_hunter.PatrolState);
            return;
        }

        float distanceToTarget = Vector3.Distance(_hunter.transform.position, _hunter.TargetBoid.position);
        if (distanceToTarget > _hunter.VisionRadius)
        {
            _hunter.TargetBoid = null;
            _hunter.StateMachine.ChangeState(_hunter.PatrolState);
            return;
        }

        Vector3 steering = _hunter.CalculatePursuit(_hunter.TargetBoid);
        _hunter.ApplyMovement(steering);
        if (distanceToTarget <= _attackRange)
        {
            _hunter.AttackTargetBoid(_hunter.TargetBoid);
            if (!_hunter.IsBoidAlive(_hunter.TargetBoid)) _hunter.StateMachine.ChangeState(_hunter.GatherState);
        }
    }

    public void Exit()
    {
        
    }
}