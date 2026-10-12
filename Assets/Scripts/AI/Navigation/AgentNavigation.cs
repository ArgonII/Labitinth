using System.Collections.Generic;
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
    public Vector3 SelectDirectionTowards(Vector3 targetPosition)
    {
        List<PathOption> paths = contextSensor.ScanEnvironment();

        if (paths == null || paths.Count == 0)
        {
            return transform.forward;
        }

        Vector3 directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0f;

        // Если цель прямо под агентом, останавливаемся или сохраняем текущее направление
        if (directionToTarget.sqrMagnitude < 0.001f)
        {
            return Vector3.zero;
        }

        directionToTarget.Normalize();

        float bestScore = float.NegativeInfinity;
        Vector3 bestDirection = transform.forward;

        // Переменные для безопасного фоллбека, если все пути небезопасны
        float maxFreeDistance = float.NegativeInfinity;
        Vector3 safestFallbackDirection = transform.forward;

        for (int i = 0; i < paths.Count; i++)
        {
            PathOption path = paths[i];

            // 1. Всегда ищем самый "просторный" путь на случай полного окружения
            if (path.FreeDistance > maxFreeDistance)
            {
                maxFreeDistance = path.FreeDistance;
                safestFallbackDirection = path.Direction;
            }

            // 2. Жесткий отсев опасных путей
            if (path.FreeDistance < safeDistance)
            {
                continue;
            }

            // 3. Вычисляем совпадение с целью (от -1 до 1)
            float alignment = Vector3.Dot(path.Direction, directionToTarget);

            // 4. Добавляем коэффициент свободного пространства для плавности (Context Steering)
            // Чем больше свободного места, тем выше приоритет пути при прочих равных
            float distanceFactor = Mathf.Clamp01(path.FreeDistance / (safeDistance * 2f));
            float score = (alignment * 0.75f) + (distanceFactor * 0.25f);

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = path.Direction;
            }
        }

        // Если ни один путь не прошел проверку безопасности (все < safeDistance)
        if (bestScore == float.NegativeInfinity)
        {
            // Возвращаем наименее опасный (самый свободный) путь вместо слепого forward
            return safestFallbackDirection;
        }

        return bestDirection;
    }
}