using Unity.VisualScripting;
using UnityEngine;

public class PlayerSensor : MonoBehaviour

{
    [Header("Player Detection")]
    [SerializeField] private Transform player;
    [SerializeField] private float viewDistance = 6f;
    [SerializeField] private float viewAngle = 160f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask playerMask;
    public Vector3 PlayerPosition => player.position;
    public bool CanSeePlayer()
    {
        if (player == null)
        {
            return false;
        }
        Vector3 origin = transform.position;
        Vector3 toPlayer = player.position - origin;
        float distance = toPlayer.magnitude;

        if(distance > viewDistance)
        {
            return false;
        }

        Vector3 directionToPlayer = toPlayer.normalized;
        float angle = Vector3.Angle(
            transform.forward,
            directionToPlayer
        );

        if(angle > viewAngle * 0.5f)
        {
            return false;
        }        
        int detectionMask = obstacleMask | playerMask;
        if (Physics.Raycast(
            origin,
            directionToPlayer,
            out RaycastHit hit,
            distance,
            detectionMask,
            QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        return false;   
    }

}
