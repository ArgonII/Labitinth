using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class WanderState : IState
{
    private readonly AgentContext context;
    public WanderState(
        AgentContext context)
    {
        this.context = context;
    }

    public void Enter(){}
    public void Tick()
    {
        context.DesiredDirection = context.Navigation.ChooseWeightedDirection();
    }
    public void Exit(){}
}