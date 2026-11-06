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

    [Header("Patrol")]
    [SerializeField] private float moveDuration = 5f;
    [SerializeField] private float observeDuration = 2f;

    [Header("Aggressive")]
    [SerializeField] private float alertDuration = 0.7f;
    private AgentContext context;
    private StateMachine stateMachine;
    private MoveState moveState;
    private ObserveState observeState;
    private AlertState alertState;
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

        moveState = new MoveState(context, moveDuration);
        observeState = new ObserveState(context, observeDuration);

        alertState = new AlertState(
            context,
            objRenderer,
            alertDuration
        );

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
        stateMachine.ChangeState(moveState);
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
        }
        if (stateMachine.CurrentState == moveState)
        {
            if (canSeePlayer)
            {
                stateMachine.ChangeState(alertState);
                return;
            }

            if (moveState.isFinished)
            {
                stateMachine.ChangeState(observeState);
                return;
            }
        }
        if (stateMachine.CurrentState == observeState)
        {
            if (canSeePlayer)
            {
                stateMachine.ChangeState(alertState);
                return;
            }

            if (observeState.isFinished)
            {
                stateMachine.ChangeState(moveState);
                return;
            }
        }
        if (stateMachine.CurrentState == alertState)
        {
            if (!canSeePlayer)
            {
                stateMachine.ChangeState(moveToLastSeenState);
                return;
            }

            if (alertState.IsFinished)
            {
                stateMachine.ChangeState(chaseState);
                return;
            }
        }
        if (stateMachine.CurrentState == chaseState)
        {
            if (!canSeePlayer)
            {
                stateMachine.ChangeState(moveToLastSeenState);
                return;
            }
        }
        if (stateMachine.CurrentState == moveToLastSeenState)
        {
            if (canSeePlayer)
            {
                stateMachine.ChangeState(alertState);
                return;
            }

            if (moveToLastSeenState.IsFinished)
            {
                stateMachine.ChangeState(searchState);
                return;
            }
        }
        if (stateMachine.CurrentState == searchState)
        {
            if (canSeePlayer)
            {
                stateMachine.ChangeState(alertState);
                return;
            }

            if (searchState.IsFinished)
            {
                stateMachine.ChangeState(moveState);
                return;
            }
        }
    }
}