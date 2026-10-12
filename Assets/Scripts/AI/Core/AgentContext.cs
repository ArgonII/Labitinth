using UnityEngine;

public class AgentContext
{
    public Transform Transform { get; }
    public AgentMotor Motor { get; }
    public ContextSensor ContextSensor { get; }
    public PlayerSensor PlayerSensor { get; }
    public AgentNavigation Navigation { get; }
    public Vector3 DesiredDirection { get; set; }
    public Vector3 LastSeenPlayerPosition { get; set; }
    
    public AgentContext(
        Transform transform,
        AgentMotor motor,
        ContextSensor contextSensor,
        PlayerSensor playerSensor,
        AgentNavigation navigation)
    {
        Transform = transform;
        Motor = motor;
        ContextSensor = contextSensor;
        PlayerSensor = playerSensor;
        Navigation = navigation;

        DesiredDirection = transform.forward;
    }
}