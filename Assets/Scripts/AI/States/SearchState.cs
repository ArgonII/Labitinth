using System.Collections.Generic;
using UnityEngine;

public class SearchState : IState
{
    private float searchTimer;
    public bool IsFinished { get; private set; }
    private readonly float searchDuration;
    private readonly float searchAttractionWeight;
    private readonly float searchAreaRadius;
    private readonly AgentContext context;
    public SearchState(
        AgentContext context,
        float searchDuration,
        float searchAttractionWeight,
        float searchAreaRadius)
    {
        this.context = context;
        this.searchDuration = searchDuration;
        this.searchAttractionWeight = searchAttractionWeight;
        this.searchAreaRadius = searchAreaRadius;
    }

    public void Enter()
    {
        searchTimer = searchDuration;
        IsFinished = false;
    }
    public void Tick()
    {
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            IsFinished = true;
            context.DesiredDirection = Vector3.zero;
            return;
        }
        context.DesiredDirection = SelectSearchDirection();
    }
    public void Exit()
    {
        IsFinished = false;
    }
    private Vector3 SelectSearchDirection()
    {
        Vector3 toCenter =
            context.LastSeenPlayerPosition - context.Transform.position;

        toCenter.y = 0f;

        float distanceToCenter =
            toCenter.magnitude;

        Vector3 directionToCenter =
            toCenter.normalized;

        List<PathOption> pathOptions =
            context.ContextSensor.ScanEnvironment();

        for (int i = 0; i < pathOptions.Count; i++)
        {
            PathOption path = pathOptions[i];

            float baseScore = path.Score;

            float alignment = Vector3.Dot(
                path.Direction,
                directionToCenter
            );

            float attractionStrength =
                distanceToCenter / searchAreaRadius * searchAttractionWeight;

            float attractionBonus =
                alignment * attractionStrength;

            float finalScore =
                baseScore + attractionBonus;

            pathOptions[i] = new PathOption(
                path.Direction,
                finalScore,
                path.FreeDistance
            );
        }
        return context.Navigation.ChooseWeightedDirection(pathOptions);
    }


}
