using System.Collections.Generic;
using UnityEngine;

public struct PathOption
{
    public Vector3 Direction;
    public float Score;

    public PathOption(Vector3 direction, float score)
    {
        Direction = direction;
        Score = score;
    }
}

[RequireComponent(typeof(CapsuleCollider))]
public class ContextSensor : MonoBehaviour
{
    [Header("Vision")]
    [SerializeField] private float viewAngle = 160f;
    [SerializeField] private float angleStep = 1f;
    [SerializeField] private float viewDistance = 5f;

    [Header("Scoring")]
    [SerializeField] private float turnPenalty = 0.2f;

    [Header("Collision Detection")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Visualization")]
    [SerializeField] private Gradient scoreGradient;
    [SerializeField] private bool drawDebugRays = true;

    private CapsuleCollider capsule;

    private readonly List<PathOption> paths =
        new List<PathOption>(360);

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();
    }

    public List<PathOption> ScanEnvironment()
    {
        paths.Clear();

        GetWorldCapsule(
            out Vector3 point1,
            out Vector3 point2,
            out float radius,
            out Vector3 center
        );

        float halfAngle = viewAngle * 0.5f;

        // Проверяем, заблокирован ли путь прямо перед агентом.
        bool isPathBlocked = Physics.CapsuleCast(
            point1,
            point2,
            radius,
            transform.forward,
            out _,
            1f,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        for (
            float angle = -halfAngle;
            angle <= halfAngle;
            angle += angleStep
        )
        {
            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f)
                * transform.forward;

            float freeDistance = GetFreeDistance(
                point1,
                point2,
                radius,
                direction
            );

            float score = CalculateScore(
                freeDistance,
                angle,
                isPathBlocked
            );

            paths.Add(
                new PathOption(
                    direction,
                    score
                )
            );

            DrawPath(
                center,
                direction,
                freeDistance
            );
        }

        return paths;
    }

    private float GetFreeDistance(
        Vector3 point1,
        Vector3 point2,
        float radius,
        Vector3 direction
    )
    {
        if (Physics.CapsuleCast(
            point1,
            point2,
            radius,
            direction,
            out RaycastHit hit,
            viewDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            return hit.distance;
        }

        return viewDistance;
    }

    private float CalculateScore(
        float freeDistance,
        float angle,
        bool isPathBlocked
    )
    {
        float anglePenalty =
            Mathf.Sqrt(Mathf.Abs(angle))
            * turnPenalty;

        if (isPathBlocked)
            return freeDistance;

        return freeDistance - anglePenalty;
    }

    private void GetWorldCapsule(
        out Vector3 point1,
        out Vector3 point2,
        out float radius,
        out Vector3 center
    )
    {
        center = transform.TransformPoint(capsule.center);

        Vector3 scale = transform.lossyScale;

        Vector3 axis;
        float axisScale;
        float radiusScale;

        // CapsuleCollider может быть направлен по X, Y или Z.
        switch (capsule.direction)
        {
            case 0: // X
                axis = transform.right;
                axisScale = Mathf.Abs(scale.x);

                radiusScale = Mathf.Max(
                    Mathf.Abs(scale.y),
                    Mathf.Abs(scale.z)
                );
                break;

            case 2: // Z
                axis = transform.forward;
                axisScale = Mathf.Abs(scale.z);

                radiusScale = Mathf.Max(
                    Mathf.Abs(scale.x),
                    Mathf.Abs(scale.y)
                );
                break;

            default: // Y
                axis = transform.up;
                axisScale = Mathf.Abs(scale.y);

                radiusScale = Mathf.Max(
                    Mathf.Abs(scale.x),
                    Mathf.Abs(scale.z)
                );
                break;
        }

        radius = capsule.radius * radiusScale;

        float height =
            capsule.height * axisScale;

        // Высота капсулы физически не может быть меньше диаметра.
        height = Mathf.Max(
            height,
            radius * 2f
        );

        // Расстояние от центра капсулы
        // до центра верхней/нижней полусферы.
        float halfSegment =
            height * 0.5f - radius;

        point1 =
            center + axis * halfSegment;

        point2 =
            center - axis * halfSegment;
    }

    private void DrawPath(
        Vector3 origin,
        Vector3 direction,
        float freeDistance
    )
    {
        if (!drawDebugRays)
            return;

        float t = Mathf.InverseLerp(
            0f,
            viewDistance,
            freeDistance
        );

        Color color = scoreGradient != null
            ? scoreGradient.Evaluate(t)
            : Color.white;

        if (freeDistance < 0.5f)
            color = Color.red;

        Debug.DrawRay(
            origin,
            direction * freeDistance,
            color
        );
    }

    public void DrawChosenDirection(
        Vector3 direction
    )
    {
        if (!drawDebugRays)
            return;

        Debug.DrawRay(
            capsule.bounds.center,
            direction * viewDistance,
            Color.black
        );
    }
}