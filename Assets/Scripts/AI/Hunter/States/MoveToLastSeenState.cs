using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MoveToLastSeenState : IState
{
    private readonly AgentContext context;
    public MoveToLastSeenState(
        AgentContext context)
    {
        this.context = context;
    }

    public void Enter(){}
    public void Tick()
    {
        context.DesiredDirection = context.Navigation.SelectDirectionTowards(context.LastSeenPlayerPosition);
    }
    public void Exit(){}
}