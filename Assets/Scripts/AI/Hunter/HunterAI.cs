using UnityEngine;

[RequireComponent(typeof(AgentMotor))]
[RequireComponent(typeof(ContextSensor))]
[RequireComponent(typeof(PlayerSensor))]
[RequireComponent(typeof(AgentNavigation))]

public class HunterAI : MonoBehaviour
{
    [Header("Search Options")]
    [SerializeField] private float searchDuration = 10f;
    [SerializeField] private float searchAttractionWeight = 3f;
    [SerializeField] private float searchAreaRadius = 4f;
    private AgentContext context;
    private StateMachine stateMachine;
    private WanderState wanderState;
    private SearchState searchState;
    private ChaseState chaseState;
    private MoveToLastSeenState moveToLastSeenState;

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

        searchState = new SearchState(
            context,
            searchDuration,
            searchAttractionWeight,
            searchAreaRadius);

        chaseState = new ChaseState(context);

        moveToLastSeenState = new MoveToLastSeenState(context);
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
            stateMachine.ChangeState(chaseState);
            objRenderer.material.color = Color.red;
            return;
        }
        if (stateMachine.CurrentState == chaseState)
        {
            stateMachine.ChangeState(moveToLastSeenState);
            objRenderer.material.color = Color.yellow;
            return;
        }
        if (stateMachine.CurrentState == moveToLastSeenState && ReachedLastSeenPosition())
        {
            stateMachine.ChangeState(searchState);
            objRenderer.material.color = Color.blue;
            return;
        }
        if (stateMachine.CurrentState == searchState && searchState.IsFinished)
        {
            objRenderer.material.color = Color.green;
            stateMachine.ChangeState(wanderState);
        }
        return;
    }
    private bool ReachedLastSeenPosition()
    {
        float distance =
            Vector3.Distance(
                context.Transform.position,
                context.LastSeenPlayerPosition
            );

        return distance <= searchAreaRadius;
    }
}