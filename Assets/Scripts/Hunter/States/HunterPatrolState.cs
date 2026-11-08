using UnityEngine;
using System.Collections.Generic;

public class HunterPatrolState : IState
{
    private List<Vector3> _patrolPoints;
    private HunterAgent _hunter;

    private int _currentIndex = 0;
    private int _direction = 1;
    private float _waypointThreshold = 1.5f;

    public HunterPatrolState(HunterAgent hunter, List<Vector3> patrolPoints)
    {
        _hunter = hunter;
        _patrolPoints = patrolPoints;
    }

    public void Enter()
    {
        if (_patrolPoints == null || _patrolPoints.Count == 0)
        {
            _patrolPoints = new List<Vector3> { _hunter.transform.position };
        }
        
        _currentIndex = 0;
        _direction = 1;
    }

    public void UpdateState()
    {
        if (_hunter.DetectLiveBoid(out Transform liveBoid))
        {
            _hunter.TargetBoid = liveBoid;
            _hunter.StateMachine.ChangeState(_hunter.HuntState);
            return;
        }

        if (_hunter.DetectDeadBoid(out Transform deadBoid))
        {
            _hunter.TargetBoid = deadBoid;
            _hunter.StateMachine.ChangeState(_hunter.GatherState);
            return;
        }

        Vector3 targetPoint = _patrolPoints[_currentIndex];
        Vector3 steering = _hunter.CalculateArrive(targetPoint);
        _hunter.ApplyMovement(steering);

        if (Vector3.Distance(_hunter.transform.position, targetPoint) <= _waypointThreshold)
        {
            if (_currentIndex == _patrolPoints.Count - 1) _direction = -1;
            else if (_currentIndex == 0) _direction = 1;

            _currentIndex += _direction;
        }
    }

    public void Exit()
    {
        
    }
}