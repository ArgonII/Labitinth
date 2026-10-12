using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AgentMotor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 5f;

    [Header("Collision")]
    [SerializeField] private float collisionSkin = 0.02f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private Vector3 currentMoveDirection;

    public void MoveTowards(Vector3 targetDirection)
    {
        if (targetDirection == Vector3.zero) return;

        targetDirection.y = 0f;
        targetDirection.Normalize();

        // Плавное сглаживание вектора движения во времени (чтобы не было резких скачков на 180 или 40 градусов за кадр)
        float smoothFactor = 10f; // Чем выше, тем резче реакция
        currentMoveDirection = Vector3.Slerp(currentMoveDirection, targetDirection, smoothFactor * Time.fixedDeltaTime);
        currentMoveDirection.Normalize();

        RotateTowards(currentMoveDirection);
        MoveInDirection(currentMoveDirection);
    }

    private void RotateTowards(Vector3 targetDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }

    private void MoveInDirection(Vector3 moveDirection)
    {
        float moveDistance = moveSpeed * Time.fixedDeltaTime;

        // Свепим именно по направлению движения, а не по transform.forward
        if (rb.SweepTest(
            moveDirection,
            out RaycastHit hit,
            moveDistance + collisionSkin,
            QueryTriggerInteraction.Ignore))
        {
            moveDistance = Mathf.Max(
                0f,
                hit.distance - collisionSkin
            );
        }

        Vector3 newPosition = rb.position + moveDirection * moveDistance;
        rb.MovePosition(newPosition);
    }
}