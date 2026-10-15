using UnityEngine;

public class MoveState : IState
{
    private readonly AgentContext context;
    private readonly float moveDuration;
    private float timer;
    public bool isFinished { get; private set; }
    public MoveState(
        AgentContext context,
        float time)
    {
        this.context = context;
        this.moveDuration = time;
    }

    public void Enter()
    {
        timer = moveDuration;
        isFinished = false;
    }
    public void Tick()
    {
        context.DesiredDirection = context.Navigation.ChooseWeightedDirection();
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isFinished = true;
        }
    }
    public void Exit()
    {
        isFinished = false;
    }
}