using UnityEngine;

public class HunterRestState : IState
{
    private HunterAgent _hunter;
    private float _restTimer;
    private float _restDuration = 5f;

    public HunterRestState(HunterAgent hunter)
    {
        _hunter = hunter;
    }

    public void Enter()
    {
        _restTimer = 0f;
        _hunter.StopMovement();
    }

    public void UpdateState()
    {
        _restTimer += Time.deltaTime;
        if (_restTimer >= _restDuration) _hunter.StateMachine.ChangeState(_hunter.PatrolState);
    }

    public void Exit()
    {
        
    }
}