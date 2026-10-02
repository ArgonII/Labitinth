using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public enum HunterState
{
    Wander,
    Chase,
    Search
}

[RequireComponent(typeof(AgentMotor))]
[RequireComponent(typeof(ContextSensor))]
[RequireComponent(typeof(PlayerSensor))]
public class AgentAI : MonoBehaviour
{
    [Header("Decision Making")]
    [SerializeField] private int bestPathsCount = 5;
    [SerializeField] private HunterState currentState = HunterState.Wander;
    [SerializeField] private float searchDuration = 10f;
    [SerializeField] private float searchAreaRadius = 4f;

    private AgentMotor motor;
    private ContextSensor contextSensor;
    private PlayerSensor playerSensor;
    private Renderer objRenderer;
    private Vector3 targetDirection;
    private Vector3 lastSeenPlayerPosition;
    private bool searchStarted = false;
    private float searchTimer;
    private readonly List<float> weightsBuffer = new List<float>(16);

    private void Awake()
    {
        motor = GetComponent<AgentMotor>();
        contextSensor = GetComponent<ContextSensor>();
        playerSensor = GetComponent<PlayerSensor>();
        objRenderer = GetComponent<Renderer>();

        targetDirection = transform.forward;
    }
    private void Update()
    {
        UpdateState();
        targetDirection = SelectDirection();
    }

    private void FixedUpdate()
    {
        motor.MoveTowards(targetDirection);
    }
    private Vector3 SelectDirection()
    {
        switch (currentState)
        {
            case HunterState.Wander:
                return SelectWanderDirection();
            case HunterState.Chase:
                return SelectChaseDirection();
            case HunterState.Search:
                return SelectSearchDirection();
            default:
                return transform.forward;
        }
    }
    private Vector3 SelectWanderDirection()
    {
        List<PathOption> paths = contextSensor.ScanEnvironment();
        return ChooseWeightedDirection(paths);
    }
    private Vector3 SelectChaseDirection()
    {
        Vector3 toPlayer = playerSensor.PlayerPosition - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f)
            return Vector3.zero;
        return toPlayer.normalized;
    }
    private Vector3 SelectSearchDirection()
    {
        Vector3 toCenter =
            lastSeenPlayerPosition - transform.position;

        toCenter.y = 0f;

        float distanceToCenter =
            toCenter.magnitude;

        Vector3 directionToCenter =
            toCenter.normalized;

        if (!searchStarted)
        {
            return directionToCenter;
        }

        List<PathOption> pathOptions =
            contextSensor.ScanEnvironment();

        for (int i = 0; i < pathOptions.Count; i++)
        {
            PathOption path = pathOptions[i];

            float baseScore = path.Score;

            float alignment = Vector3.Dot(
                path.Direction,
                directionToCenter
            );

            float attractionStrength =
                distanceToCenter / searchAreaRadius;

            float attractionBonus =
                alignment * attractionStrength;

            float finalScore =
                baseScore + attractionBonus;

            pathOptions[i] = new PathOption(
                path.Direction,
                finalScore
            );
        }
        return ChooseWeightedDirection(pathOptions);
    }

    private void UpdateState()
    {
        bool CanSeePlayer = playerSensor.CanSeePlayer();
        if (CanSeePlayer)
        {
            objRenderer.material.color = Color.red;
            lastSeenPlayerPosition = playerSensor.PlayerPosition;

            currentState = HunterState.Chase;
            searchStarted = false;
            searchTimer = 0f;
        }
        else if (currentState == HunterState.Chase)
        {
            objRenderer.material.color = Color.orange;
            currentState = HunterState.Search;
            searchStarted = false;
        }
        else if (currentState == HunterState.Search)
        {
            float distanceToCenter = Vector3.Distance(transform.position, lastSeenPlayerPosition);
            if (!searchStarted && distanceToCenter <= searchAreaRadius)
            {
                searchStarted = true;
                searchTimer = searchDuration;
            }
            if (searchStarted)
            {
                objRenderer.material.color = Color.blue;

                searchTimer -= Time.deltaTime;
                if (searchTimer <= 0f)
                {
                    objRenderer.material.color = Color.green;
                    currentState = HunterState.Wander;
                    searchStarted = false;
                    searchTimer = 0f;
                }
            }

        }
    }

    private Vector3 ChooseWeightedDirection(List<PathOption> paths)
    {
        if (paths.Count == 0)
        {
            return transform.forward;
        }
        paths.Sort((a, b) => b.Score.CompareTo(a.Score));

        int count = Mathf.Min(bestPathsCount, paths.Count);

        float minScore = paths[count - 1].Score;

        float offset = minScore < 0f
            ? Mathf.Abs(minScore) + 0.1f
            : 0.1f;

        weightsBuffer.Clear();

        float totalWeight = 0f;

        for (int i = 0; i < count; i++)
        {
            float weight = paths[i].Score + offset;

            weightsBuffer.Add(weight);
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);

        float accumulatedWeight = 0f;

        Vector3 chosenDirection = paths[0].Direction;

        for (int i = 0; i < count; i++)
        {
            accumulatedWeight += weightsBuffer[i];

            if (accumulatedWeight > randomValue)
            {
                chosenDirection = paths[i].Direction;
                break;
            }
        }

        contextSensor.DrawChosenDirection(chosenDirection);

        return chosenDirection;
    }
}