using System;
using System.Collections.Generic;

public class HFSMStateMachine : IState
{

    public IState CurrentState { get; private set; }

    private IState initialState;
    private readonly List<Transition> transitions = new List<Transition>();

    private class Transition
    {
        public IState From;
        public IState To;
        public Func<bool> Condition;
        public Transition(
            IState from,
            IState to,
             Func<bool> condition
        )
        {
            this.From = from;
            this.To = to;
            this.Condition = condition;
        }
    }
    public void SetInitialState(IState state)
    {
        initialState = state;
    }
    public void AddTransition(
        IState from,
        IState to,
        Func<bool> condition)
    {
        transitions.Add(new Transition(from, to, condition));
    }

    public void ChangeState(IState newState)
    {
        if (CurrentState == newState)
            return;

        if(newState == null)
            return;

        CurrentState?.Exit();

        CurrentState = newState;

        CurrentState?.Enter();
    }
    public void Enter()
    {
        if (initialState == null)
        {
            return;
        }
        ChangeState(initialState);
    }
    public void Tick()
    {
        CheckTransitions();
        CurrentState?.Tick();
    }
    public void Exit()
    {
        CurrentState?.Exit();
        CurrentState = null;
    }
    private void CheckTransitions()
    {
        foreach (Transition transition in transitions)
        {
            if (transition.From != CurrentState)
                continue;

            if (!transition.Condition())
                continue;

            ChangeState(transition.To);
            return;
        }
    }
}