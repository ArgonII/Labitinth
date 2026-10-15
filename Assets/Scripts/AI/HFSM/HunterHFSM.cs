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
    private AgentContext context;

    private HFSMStateMachine rootMachine;
    private HFSMStateMachine patrolMachine;
    private HFSMStateMachine searchingMachine;

    private MoveState moveState;
    private ObserveState observeState;
    private ChaseState chaseState;
    private MoveToLastSeenState moveToLastSeenState;
    private SearchState searchState;


    private void Awake()
    {
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
        searchingMachine = new HFSMStateMachine();


        moveState = new MoveState(context, moveDuration);
        observeState = new ObserveState(context, observeDuration);
        chaseState = new ChaseState(context);
        searchState = new SearchState(
            context,
            searchDuration,
            searchAttractionWeight,
            searchAreaRadius);
        moveToLastSeenState = new MoveToLastSeenState(context);

        rootMachine = new HFSMStateMachine();

        patrolMachine.SetInitialState(moveState);
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

        searchingMachine.AddTransition(
            moveToLastSeenState,
            searchState,
            () => moveToLastSeenState.IsFinished
        );

        rootMachine.AddTransition(
            patrolMachine,
            chaseState,
            () => context.PlayerSensor.CanSeePlayer()
        );

        rootMachine.AddTransition(
            chaseState,
            searchingMachine,
            () => !context.PlayerSensor.CanSeePlayer()
        );

        rootMachine.AddTransition(
            searchingMachine,
            chaseState,
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