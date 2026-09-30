using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentMotor))]
[RequireComponent(typeof(ContextSensor))]
public class AgentAI : MonoBehaviour
{
    [Header("Decision Making")]
    [SerializeField] private int bestPathsCount = 5;

    private AgentMotor motor;
    private ContextSensor sensor;

    private Vector3 targetDirection;

    private readonly List<float> weightsBuffer = new List<float>(16);

    private void Awake()
    {
        motor = GetComponent<AgentMotor>();
        sensor = GetComponent<ContextSensor>();

        targetDirection = transform.forward;
    }

    private void Update()
    {
        targetDirection = SelectDirection();
    }

    private void FixedUpdate()
    {
        motor.MoveTowards(targetDirection);
    }

    private Vector3 SelectDirection()
    {
        List<PathOption> paths = sensor.ScanEnvironment();

        if (paths.Count == 0)
            return transform.forward;

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

        sensor.DrawChosenDirection(chosenDirection);

        return chosenDirection;
    }
}