using UnityEngine;

[RequireComponent(typeof(AgentMotor))]
[RequireComponent(typeof(ContextSensor))]
[RequireComponent(typeof(PlayerSensor))]
[RequireComponent(typeof(AgentNavigation))]

public class HunterHFSM : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float observeDuration = 2f;
    [SerializeField] private float moveDuration = 5f;

    [Header("Search")]
    [SerializeField] private float searchDuration = 15f;
    [SerializeField] private float searchAttractionWeight = 3f;
    [SerializeField] private float searchAreaRadius = 12f;

    [Header("Aggressive")]
    [SerializeField] private float alertDuration = 0.7f;
    private AgentContext context;

    private HFSMStateMachine rootMachine;
    private HFSMStateMachine patrolMachine;
    private HFSMStateMachine searchingMachine;
    private HFSMStateMachine aggressiveMachine;


    private MoveState moveState;
    private ObserveState observeState;
    private ChaseState chaseState;
    private MoveToLastSeenState moveToLastSeenState;
    private SearchState searchState;
    private AlertState alertState;

    private void Awake()
    {
        Renderer hunterRenderer = GetComponentInChildren<Renderer>();

        AgentMotor motor = GetComponent<AgentMotor>();
        ContextSensor contextSensor = GetComponent<ContextSensor>();
        PlayerSensor playerSensor = GetComponent<PlayerSensor>();
        AgentNavigation navigation = GetComponent<AgentNavigation>();

        context = new AgentContext(
            transform,
            motor,
            contextSensor,
            playerSensor,
            navigation
        );
        patrolMachine = new HFSMStateMachine();
        aggressiveMachine = new HFSMStateMachine();
        searchingMachine = new HFSMStateMachine();


        moveState = new MoveState(context, moveDuration);
        observeState = new ObserveState(context, observeDuration);
        alertState = new AlertState(context, hunterRenderer, alertDuration);
        chaseState = new ChaseState(context);
        chaseState = new ChaseState(context);
        searchState = new SearchState(
            context,
            searchDuration,
            searchAttractionWeight,
            searchAreaRadius);
        moveToLastSeenState = new MoveToLastSeenState(context);

        rootMachine = new HFSMStateMachine();

        patrolMachine.SetInitialState(moveState);
        aggressiveMachine.SetInitialState(alertState);
        searchingMachine.SetInitialState(moveToLastSeenState);

        rootMachine.SetInitialState(patrolMachine);

        patrolMachine.AddTransition(
            moveState,
            observeState,
            () => moveState.isFinished
        );

        patrolMachine.AddTransition(
            observeState,
            moveState,
            () => observeState.isFinished
        );

        aggressiveMachine.AddTransition(
            alertState,
            chaseState,
            () => alertState.IsFinished
        );

        searchingMachine.AddTransition(
            moveToLastSeenState,
            searchState,
            () => moveToLastSeenState.IsFinished
        );

        rootMachine.AddTransition(
            patrolMachine,
            aggressiveMachine,
            () => context.PlayerSensor.CanSeePlayer()
        );

        rootMachine.AddTransition(
            aggressiveMachine,
            searchingMachine,
            () => !context.PlayerSensor.CanSeePlayer()
        );

        rootMachine.AddTransition(
            searchingMachine,
            aggressiveMachine,
            () => context.PlayerSensor.CanSeePlayer()
        );

        rootMachine.AddTransition(
            searchingMachine,
            patrolMachine,
            () => searchState.IsFinished
        );
    }

    private void Start()
    {
        rootMachine.Enter();
    }
    private void Update()
    {
        if (context.PlayerSensor.CanSeePlayer())
        {
            context.LastSeenPlayerPosition = context.PlayerSensor.PlayerPosition;
        }
        rootMachine.Tick();
    }
    private void FixedUpdate()
    {
        context.Motor.MoveTowards(context.DesiredDirection);
    }
}