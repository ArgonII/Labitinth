using UnityEngine;

public class FleeState : IState
{
    private readonly AgentContext context;
    public FleeState(
        AgentContext context)
    {
        this.context = context;
    }

    public void Enter(){}
    public void Tick()
    {
        Vector3 playerPosition = context.PlayerSensor.PlayerPosition;
        context.DesiredDirection = context.Navigation.SelectDirectionAwayFrom(playerPosition);
    }
    public void Exit(){}
}