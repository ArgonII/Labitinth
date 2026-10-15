public class StateMachine : IState
{
    public IState CurrentState { get; private set; }
    private IState initialState;
    public void SetInitialState(IState state)
    {
        initialState = state;
    }
    public void ChangeState(IState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }
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
        CurrentState?.Tick();
    }
    public void Exit()
    {
        CurrentState?.Exit();
        CurrentState = null;
    }
}