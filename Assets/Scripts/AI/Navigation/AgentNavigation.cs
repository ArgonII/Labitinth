using System.Collections.Generic;
using System.IO;
using UnityEngine;


[RequireComponent(typeof(ContextSensor))]
public class AgentNavigation : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private int bestPathsCount = 5;
    [SerializeField] private float safeDistance = 0.5f;

    private ContextSensor contextSensor;
    private readonly List<float> weightsBuffer = new List<float>(16);

    public void Awake()
    {
        contextSensor = GetComponent<ContextSensor>();
    }
    public Vector3 ChooseWeightedDirection()
    {
        List<PathOption> paths = contextSensor.ScanEnvironment();

        return ChooseWeightedDirection(paths);
    }

    public Vector3 ChooseWeightedDirection(List<PathOption> paths)
    {
        if (paths.Count == 0)
        {
            return transform.forward;
        }

        List<PathOption> safePaths = new List<PathOption>();

        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i].FreeDistance >= safeDistance)
            {
                safePaths.Add(paths[i]);
            }
        }

        if (safePaths.Count == 0)
        {
            PathOption safestPath = paths[0];

            for (int i = 1; i < paths.Count; i++)
            {
                if (paths[i].FreeDistance > safestPath.FreeDistance)
                {
                    safestPath = paths[i];
                }
            }

            return safestPath.Direction;
        }

        safePaths.Sort((a, b) => b.Score.CompareTo(a.Score));

        int count = Mathf.Min(bestPathsCount, safePaths.Count);

        float minScore = safePaths[count - 1].Score;

        float offset = minScore < 0f
            ? Mathf.Abs(minScore) + 0.1f
            : 0.1f;

        weightsBuffer.Clear();

        float totalWeight = 0f;

        for (int i = 0; i < count; i++)
        {
            float weight = safePaths[i].Score + offset;

            weightsBuffer.Add(weight);
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);

        float accumulatedWeight = 0f;

        Vector3 chosenDirection = safePaths[0].Direction;

        for (int i = 0; i < count; i++)
        {
            accumulatedWeight += weightsBuffer[i];

            if (accumulatedWeight > randomValue)
            {
                chosenDirection = safePaths[i].Direction;
                break;
            }
        }

        contextSensor.DrawChosenDirection(chosenDirection);
        return chosenDirection;
    }
    /*
    public Vector3 SelectDirectionTowards(Vector3 targetPosition)
    {
        List<PathOption> paths = contextSensor.ScanEnvironment();

        if (paths == null || paths.Count == 0)
        {
            return transform.forward;
        }

        Vector3 directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude < 0.001f)
        {
            return Vector3.zero;
        }

        directionToTarget.Normalize();

        float bestScore = float.NegativeInfinity;
        Vector3 bestDirection = transform.forward;

        float maxFreeDistance = float.NegativeInfinity;
        Vector3 safestFallbackDirection = transform.forward;

        for (int i = 0; i < paths.Count; i++)
        {
            PathOption path = paths[i];

            if (path.FreeDistance > maxFreeDistance)
            {
                maxFreeDistance = path.FreeDistance;
                safestFallbackDirection = path.Direction;
            }

            if (path.FreeDistance < safeDistance)
            {
                continue;
            }

            float alignment = Vector3.Dot(path.Direction, directionToTarget);

            float distanceFactor = Mathf.Clamp01(path.FreeDistance / (safeDistance * 2f));
            float score = (alignment * 0.75f) + (distanceFactor * 0.25f);

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = path.Direction;
            }
        }

        if (bestScore == float.NegativeInfinity)
        {
            return safestFallbackDirection;
        }

        return bestDirection;
    }
    */

    public Vector3 SelectDirectionAlignedWith(Vector3 desiredDirection)
    {
        List<PathOption> paths = contextSensor.ScanEnvironment();

        if (paths == null || paths.Count == 0)
        {
            return transform.forward;
        }

        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude > 0.0001f)
        {
            desiredDirection.Normalize();
        }

        float bestAlignment = float.NegativeInfinity;
        Vector3 bestDirection = transform.forward;

        bool foundSafePath = false;

        PathOption safestPath = paths[0];

        for (int i = 0; i < paths.Count; i++)
        {
            PathOption path = paths[i];

            if (path.FreeDistance > safestPath.FreeDistance)
            {
                safestPath = path;
            }

            if (path.FreeDistance < safeDistance)
            {
                continue;
            }

            float alignment = Vector3.Dot(path.Direction, desiredDirection);

            if (alignment > bestAlignment)
            {
                bestAlignment = alignment;
                bestDirection = path.Direction;
                foundSafePath = true;
            }
        }

        if (foundSafePath)
        {
            return bestDirection;
        }

        return safestPath.Direction;
    }

    public Vector3 SelectDirectionTowards(Vector3 targetPosition)
    {
        Vector3 desiredDirection = targetPosition - transform.position;
        return SelectDirectionAlignedWith(desiredDirection);
    }
    public Vector3 SelectDirectionAwayFrom(Vector3 targetPosition)
    {
        Vector3 desiredDirection = transform.position - targetPosition;
        return SelectDirectionAlignedWith(desiredDirection);
    }
}