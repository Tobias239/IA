using UnityEngine;

public class HunterGatherState : IState
{
    private HunterAgent _hunter;
    private float _gatherDistance = 1.5f;
    private float _gatherTimer;
    private float _gatherDuration = 3f;

    public HunterGatherState(HunterAgent hunter)
    {
        _hunter = hunter;
    }

    public void Enter()
    {
        _gatherTimer = 0f;
    }

    public void UpdateState()
    {
        if (_hunter.TargetBoid == null)
        {
            _hunter.StateMachine.ChangeState(_hunter.PatrolState);
            return;
        }

        float distanceToTarget = Vector3.Distance(_hunter.transform.position, _hunter.TargetBoid.position);
        if (distanceToTarget > _gatherDistance)
        {
            Vector3 steering = _hunter.CalculateArrive(_hunter.TargetBoid.position);
            _hunter.ApplyMovement(steering);
        }
        else
        {
            _hunter.StopMovement();
            _gatherTimer += Time.deltaTime;

            if (_gatherTimer >= _gatherDuration)
            {
                _hunter.CollectTargetBoid(_hunter.TargetBoid);
                _hunter.TargetBoid = null;
                _hunter.StateMachine.ChangeState(_hunter.PatrolState);
            }
        }
    }

    public void Exit()
    {
        
    }
}