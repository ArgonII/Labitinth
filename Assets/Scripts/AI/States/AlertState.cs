using UnityEngine;

public class AlertState : IState
{
    private readonly AgentContext context;
    private readonly Renderer renderer;
    private readonly float alertDuration;

    private float timer;
    private Color originalColor;

    public bool IsFinished { get; private set; }

    public AlertState(
        AgentContext context,
        Renderer renderer,
        float alertDuration)
    {
        this.context = context;
        this.renderer = renderer;
        this.alertDuration = alertDuration;

        originalColor = renderer.material.color;
    }

    public void Enter()
    {
        timer = alertDuration;
        IsFinished = false;

        context.DesiredDirection = Vector3.zero;

        renderer.material.color = Color.red;
    }

    public void Tick()
    {
        context.DesiredDirection = Vector3.zero;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            IsFinished = true;
        }
    }

    public void Exit()
    {
        IsFinished = false;

        renderer.material.color = originalColor;
    }
}