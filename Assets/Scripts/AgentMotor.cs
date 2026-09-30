using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AgentMotor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 5f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void MoveTowards(Vector3 targetDirection)
    {
        if (targetDirection == Vector3.zero)
            return;

        targetDirection.y = 0f;
        targetDirection.Normalize();

        RotateTowards(targetDirection);
        MoveForward(targetDirection);
    }

    private void RotateTowards(
        Vector3 targetDirection
    )
    {
        Quaternion targetRotation =
            Quaternion.LookRotation(targetDirection);

        Quaternion newRotation =
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime
            );

        rb.MoveRotation(newRotation);
    }

    private void MoveForward(
        Vector3 targetDirection
    )
    {
        float alignment = Mathf.Max(
            0f,
            Vector3.Dot(
                transform.forward,
                targetDirection
            )
        );

        float speedFactor =
            Mathf.Clamp(
                alignment,
                0.2f,
                1f
            );

        Vector3 newPosition =
            rb.position
            + transform.forward
            * (moveSpeed * speedFactor)
            * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }
}