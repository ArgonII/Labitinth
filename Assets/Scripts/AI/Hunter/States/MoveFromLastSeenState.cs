using UnityEngine;
public class MoveFromLastSeenState : IState
{
    private readonly AgentContext context;
    private readonly float fleeDistance;
    public bool IsFinished { get; private set; }
    public MoveFromLastSeenState(
        AgentContext context,
        float fleeDistance)
    {
        this.context = context;
        this.fleeDistance = fleeDistance;
    }

    public void Enter()
    {
        IsFinished = false;
    }
    public void Tick()
    {
        Vector3 toPlayer = context.LastSeenPlayerPosition - context.Transform.position;
        toPlayer.y = 0f;

        float distanceToPlayer = toPlayer.magnitude;
        if (distanceToPlayer >= fleeDistance)
        {
            IsFinished = true;
            context.DesiredDirection = Vector3.zero;
            return;
        }
        if (distanceToPlayer < fleeDistance)
        {
            context.DesiredDirection = context.Navigation.SelectDirectionAwayFrom(context.LastSeenPlayerPosition);
        }
    }
    public void Exit()
    {
        IsFinished = false;
    }
}