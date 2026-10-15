using UnityEngine;
public class MoveToLastSeenState : IState
{
    private static float toleranceDistance = 0.2f;
    private readonly AgentContext context;
    public bool IsFinished { get; private set; }
    public MoveToLastSeenState(
        AgentContext context)
    {
        this.context = context;
    }

    public void Enter()
    {
        IsFinished = false;
    }
    public void Tick()
    {
        Vector3 toLastSeen = context.LastSeenPlayerPosition - context.Transform.position;
        toLastSeen.y = 0f;

        float distanceToLastSeen = toLastSeen.magnitude;
        if (distanceToLastSeen <= toleranceDistance)
        {
            IsFinished = true;
            context.DesiredDirection = Vector3.zero;
            return;
        }
        context.DesiredDirection = context.Navigation.SelectDirectionTowards(context.LastSeenPlayerPosition);
    }
    public void Exit()
    {
        IsFinished = false;
    }
}