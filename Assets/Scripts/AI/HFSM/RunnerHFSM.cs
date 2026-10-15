using UnityEngine;

[RequireComponent(typeof(AgentMotor))]
[RequireComponent(typeof(ContextSensor))]
[RequireComponent(typeof(PlayerSensor))]
[RequireComponent(typeof(AgentNavigation))]

public class RunnerHSFM : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float observeDuration = 2f;
    [SerializeField] private float moveDuration = 5f;

    [Header("Flee")]
    [SerializeField] private float fleeDistance = 5f;
    private AgentContext context;

    private HFSMStateMachine rootMachine;
    private HFSMStateMachine patrolMachine;
    private HFSMStateMachine fleeMachine;

    private MoveState moveState;
    private ObserveState observeState;
    private FleeState fleeState;
    private MoveFromLastSeenState moveFromLastSeenState;

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
        fleeMachine = new HFSMStateMachine();


        moveState = new MoveState(context, moveDuration);
        observeState = new ObserveState(context, observeDuration);
        fleeState = new FleeState(context);
        moveFromLastSeenState = new MoveFromLastSeenState(context, fleeDistance);

        rootMachine = new HFSMStateMachine();

        patrolMachine.SetInitialState(moveState);
        fleeMachine.SetInitialState(fleeState);
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

        fleeMachine.AddTransition(
            fleeState,
            moveFromLastSeenState,
            () => !context.PlayerSensor.CanSeePlayer()
        );

        fleeMachine.AddTransition(
            moveFromLastSeenState,
            fleeState,
            () => context.PlayerSensor.CanSeePlayer()
        );

        rootMachine.AddTransition(
            patrolMachine,
            fleeMachine,
            () => context.PlayerSensor.CanSeePlayer()
        );

        rootMachine.AddTransition(
            fleeMachine,
            patrolMachine,
            () => moveFromLastSeenState.IsFinished
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