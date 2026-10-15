using UnityEngine;
[RequireComponent(typeof(AgentMotor))]
[RequireComponent(typeof(ContextSensor))]
[RequireComponent(typeof(PlayerSensor))]
[RequireComponent(typeof(AgentNavigation))]

public class RunnerAI : MonoBehaviour
{
    [SerializeField] private float fleeDistance = 5f;
    private AgentContext context;
    private StateMachine stateMachine;
    private WanderState wanderState;
    private FleeState fleeState;
    private MoveFromLastSeenState moveFromLastSeenState;
    private Renderer objRenderer;

    private void Awake()
    {
        AgentMotor motor = GetComponent<AgentMotor>();
        ContextSensor contextSensor = GetComponent<ContextSensor>();
        PlayerSensor playerSensor = GetComponent<PlayerSensor>();
        AgentNavigation navigation = GetComponent<AgentNavigation>();
        objRenderer = GetComponent<Renderer>();

        context = new AgentContext(
            transform,
            motor,
            contextSensor,
            playerSensor,
            navigation
        );

        stateMachine = new StateMachine();

        wanderState = new WanderState(context);

        fleeState = new FleeState(context);

        moveFromLastSeenState = new MoveFromLastSeenState(context, fleeDistance);

    }
    private void Start()
    {
        stateMachine.ChangeState(wanderState);
    }
    private void Update()
    {
        UpdateTransitions();
        stateMachine.Tick();
    }
    private void FixedUpdate()
    {
        context.Motor.MoveTowards(
            context.DesiredDirection
        );
    }

    private void UpdateTransitions()
    {
        bool canSeePlayer = context.PlayerSensor.CanSeePlayer();
        if (canSeePlayer)
        {
            context.LastSeenPlayerPosition = context.PlayerSensor.PlayerPosition;
            stateMachine.ChangeState(fleeState);
            objRenderer.material.color = Color.red;
            return;
        }
        if (stateMachine.CurrentState == fleeState)
        {
            stateMachine.ChangeState(moveFromLastSeenState);
            objRenderer.material.color = Color.yellow;
            return;
        }
        if (stateMachine.CurrentState == moveFromLastSeenState && moveFromLastSeenState.IsFinished)
        {
            stateMachine.ChangeState(wanderState);
            objRenderer.material.color = Color.blue;
            return;
        }
        return;
    }
}