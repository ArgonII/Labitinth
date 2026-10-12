using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ChaseState : IState
{
    private readonly AgentContext context;
    public ChaseState(
        AgentContext context)
    {
        this.context = context;
    }

    public void Enter(){}
    public void Tick()
    {
        Vector3 playerPosition = context.PlayerSensor.PlayerPosition;
        context.DesiredDirection = context.Navigation.SelectDirectionTowards(playerPosition);
    }
    public void Exit(){}
}