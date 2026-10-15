using System.Threading;
using UnityEngine;

public class ObserveState : IState
{
    private readonly AgentContext context;
    private readonly float observeDuration;
    private float timer;
    public bool isFinished { get; private set; }
    public ObserveState(
        AgentContext context,
        float time)
    {
        this.context = context;
        this.observeDuration = time;
    }

    public void Enter()
    {
        timer = observeDuration;
        isFinished = false;
        context.DesiredDirection = Vector3.zero;
    }
    public void Tick()
    {
        context.DesiredDirection = Vector3.zero;
        
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