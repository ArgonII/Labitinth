using UnityEngine;
using System.Collections.Generic;

public class AgentAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float turnSpeed = 5f;

    [Header("Visualization")]
    public Gradient scoreGradient;

    [Header("Vision")]
    public float viewAngle = 160f;
    public float angleStep = 1f;
    public float viewDistance = 5f;
    public float turnPenalty = 0.2f;
    public float sensorRadiusMultiplier = 0.5f;
    public LayerMask obstacleMask;
    public float bestPathsCount = 5f;
    private CapsuleCollider capsule;
    private Rigidbody rb;
    private Vector3 targetDirection;

    struct PathOption
    {
        public Vector3 direction;
        public float score;
        public PathOption(Vector3 direction, float score)
        {
            this.direction = direction;
            this.score = score;
        }
    }
    void Start()
    {
        capsule = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        targetDirection = ScanEnvironment();
    }

    void FixedUpdate()
    {
        MoveAndRotate(targetDirection);
    }

  Vector3 ScanEnvironment(){
    float halfAngle = viewAngle / 2f;
    List<PathOption> allPaths = new List<PathOption>();

    float radius = capsule.radius * sensorRadiusMultiplier;
    Vector3 origin = capsule.bounds.center;

    bool isPathBlocked = Physics.SphereCast(
        origin, radius, transform.forward, 
        out RaycastHit blockedCheckHit, 1f, obstacleMask, QueryTriggerInteraction.Ignore
    );

    for (float angle = -halfAngle; angle <= halfAngle; angle += angleStep)
    {
        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        Vector3 direction = rotation * transform.forward;

        float rayDistance = viewDistance;
        float sphereDistance = viewDistance;

        if (Physics.Raycast(origin, direction, out RaycastHit rayHit, viewDistance, obstacleMask, QueryTriggerInteraction.Ignore))
            rayDistance = rayHit.distance;

        if (Physics.SphereCast(origin, radius, direction, out RaycastHit sphereHit, viewDistance, obstacleMask, QueryTriggerInteraction.Ignore))
            sphereDistance = sphereHit.distance;

        float freeDistance = Mathf.Min(rayDistance, sphereDistance);
        float anglePenalty = Mathf.Sqrt(Mathf.Abs(angle)) * turnPenalty;
        float currentScore = isPathBlocked ? freeDistance : (freeDistance - anglePenalty);

        float t = Mathf.InverseLerp(0f, viewDistance, freeDistance);
        Color rayColor = scoreGradient != null ? scoreGradient.Evaluate(t) : Color.white;
        if (freeDistance < 0.5f) rayColor = Color.red;

        Debug.DrawRay(origin, direction * freeDistance, rayColor);

        allPaths.Add(new PathOption(direction, currentScore));
    }

    if (allPaths.Count == 0) return transform.forward;

    allPaths.Sort((a, b) => b.score.CompareTo(a.score));

    int k = Mathf.Min((int)bestPathsCount, allPaths.Count);
    List<PathOption> topPaths = allPaths.GetRange(0, k);

    float minScore = topPaths[topPaths.Count - 1].score;
    float offset = minScore < 0 ? Mathf.Abs(minScore) + 0.1f : 0.1f;

    double totalWeight = 0;
    double[] weights = new double[topPaths.Count];

    for (int i = 0; i < topPaths.Count; i++)
    {
        weights[i] = topPaths[i].score + offset;
        totalWeight += weights[i];
    }

    double r = Random.Range(0f, (float)totalWeight);
    double acc = 0;
    Vector3 chosenDirection = topPaths[0].direction;

    for (int i = 0; i < topPaths.Count; i++)
    {
        acc += weights[i];
        if (acc > r)
        {
            chosenDirection = topPaths[i].direction;
            break;
        }
    }

    Debug.DrawRay(origin, chosenDirection * viewDistance, Color.black);
    return chosenDirection;
}
    void MoveAndRotate(Vector3 targetDirection)
    {
        if (targetDirection == Vector3.zero) return;

        targetDirection.y = 0f;
        targetDirection.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
        float currentForwardAlignment = Mathf.Max(0f, Vector3.Dot(transform.forward, targetDirection));
        float speedFactor = Mathf.Clamp(currentForwardAlignment, 0.2f, 1f);
        Vector3 newPosition = rb.position + transform.forward * (moveSpeed * speedFactor) * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }
}