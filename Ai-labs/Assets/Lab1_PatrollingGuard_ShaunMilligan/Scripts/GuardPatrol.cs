using UnityEngine;
using UnityEngine.AI;


// I used a simple enum and switch statement at first but I wanted to use this 
//   statemachine so I have better flow between states


[RequireComponent(typeof(NavMeshAgent))]
public class GuardPatrol : MonoBehaviour
{
    [Header("Navigation")]
    public Transform[] Waypoints;
    public float WaypointTolerance = 0.5f;

    [Header("AI Sensors")]
    public Transform Target;
    public float ViewRadius = 15f;       // How far can the guard see
    [Range(0, 360)]
    public float ViewAngle = 90f;        // FOV width, so guard can't see behind themselves
    public LayerMask ObstructionMask;    // set layaers that obstruct view to player

    [Header("AI Settings")]
    public float SearchDuration = 4f;    // How long to look around before giving up
    public float SearchTurnSpeed = 120f; // How fast to spin while searching

    [Header("Visuals")]
    [SerializeField] private MeshRenderer _meshRendererDot;
    [SerializeField] private MeshRenderer _meshRendererExclamation;

    // guard properties 
    public StateMachine Machine { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public int CurrentWaypointIndex { get; set; } = 0;
    public (MeshRenderer, MeshRenderer) GetMeshTuple => (_meshRendererDot, _meshRendererExclamation);

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Machine = new StateMachine();
    }

    private void Start()
    {
        // initialize state machine and set initial state pass this guard
        Machine.ChangeState(new GuardPatrollingState(this));
    }

    private void Update()
    {
        // state machine executed every update
        if (Machine.CurrentState != null)
        {
            Machine.CurrentState.Execute();
        }
    }

    // bool check if player is in FOV
    public bool CanSeeTarget()
    {
        if (Target == null) return false;

        // distance Check
        float distToTarget = Vector3.Distance(transform.position, Target.position);
        if (distToTarget > ViewRadius) return false;

        // angle Check FOV
        Vector3 dirToTarget = (Target.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToTarget) > ViewAngle / 2) return false;

        // cast a ray from guards position to the target, if a obstuction is hit guard can't see the player
        if (Physics.Raycast(transform.position, dirToTarget, distToTarget, ObstructionMask))
        {
            return false; // blocked by obstruction
        }

        return true; // saw the target
    }

    // updates the color on the exclamation point
    public void UpdateVisuals(Color newColor, (MeshRenderer dot, MeshRenderer mark) meshes)
    {
        if (meshes.dot != null) meshes.dot.material.color = newColor;
        if (meshes.mark != null) meshes.mark.material.color = newColor;
    }

    // tune in values in the scene view
    private void OnDrawGizmosSelected()
    {
        // draw view radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ViewRadius);

        // draw view angle lines
        Vector3 viewAngleA = DirFromAngle(-ViewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(ViewAngle / 2, false);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * ViewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * ViewRadius);
    }

    // helper function for gizmos
    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}

// states are declared as classes, makes extention easy

// initial and default patrol state
public class GuardPatrollingState : IState
{
    private GuardPatrol _guard;
    private Color _stateColor = Color.yellow;

    public GuardPatrollingState(GuardPatrol guard) => _guard = guard;

    public void Enter()
    {
        // can set exclamation point color on state change
        _guard.UpdateVisuals(_stateColor, _guard.GetMeshTuple);
        _guard.Agent.isStopped = false; // ensure we are moving

        if (_guard.Waypoints.Length > 0)
        {
            _guard.Agent.SetDestination(_guard.Waypoints[_guard.CurrentWaypointIndex].position);
        }
    }

    public void Execute()
    {
        // transition to chase if player is spotted
        if (_guard.CanSeeTarget())
        {
            _guard.Machine.ChangeState(new GuardChasingState(_guard));
            return;
        }

        // patrol around the points infinitely
        if (!_guard.Agent.pathPending && _guard.Agent.remainingDistance < _guard.WaypointTolerance)
        {
            _guard.CurrentWaypointIndex = (_guard.CurrentWaypointIndex + 1) % _guard.Waypoints.Length;
            _guard.Agent.SetDestination(_guard.Waypoints[_guard.CurrentWaypointIndex].position);
        }
    }

    public void Exit() { }
}

// state for chasing player
public class GuardChasingState : IState
{
    private GuardPatrol _guard;
    private Color _stateColor = Color.red;

    public GuardChasingState(GuardPatrol guard) => _guard = guard;

    public void Enter()
    {
        // set color on enter
        _guard.UpdateVisuals(_stateColor, _guard.GetMeshTuple);
        _guard.Agent.isStopped = false;
    }

    public void Execute()
    {
        // check to see if we need to transition to searching
        if (!_guard.CanSeeTarget())
        {
            _guard.Machine.ChangeState(new GuardSearchingState(_guard));
            return;
        }

        // otherwise guard is chasing player
        _guard.Agent.SetDestination(_guard.Target.position);
    }

    public void Exit() { }
}

// guard searching for player state
public class GuardSearchingState : IState
{
    private GuardPatrol _guard;
    private Color _stateColor = Color.cyan; // light blue to indicate search state
    private float _timer;

    public GuardSearchingState(GuardPatrol guard) => _guard = guard;

    public void Enter()
    {
        // set color
        _guard.UpdateVisuals(_stateColor, _guard.GetMeshTuple);

        // stop moving while we look around
        _guard.Agent.isStopped = true;
        _timer = 0f;
    }

    public void Execute()
    {
        // caught sight of the player again
        if (_guard.CanSeeTarget())
        {
            _guard.Machine.ChangeState(new GuardChasingState(_guard));
            return;
        }

        // while timer ticks look around for player
        _timer += Time.deltaTime;

        // rotate guard
        _guard.transform.Rotate(0, _guard.SearchTurnSpeed * Time.deltaTime, 0);

        // guard isn't paid enough to care that much
        if (_timer >= _guard.SearchDuration)
        {
            _guard.Machine.ChangeState(new GuardReturningToPatrolState(_guard));
        }
    }

    public void Exit()
    {
        // re-enable movement after stopping to search
        _guard.Agent.isStopped = false;
    }
}

// return to patrol state
public class GuardReturningToPatrolState : IState
{
    private GuardPatrol _guard;
    private Color _stateColor = new Color(1f, 0.5f, 0f); // orange

    public GuardReturningToPatrolState(GuardPatrol guard) => _guard = guard;

    public void Enter()
    {
        // change exclamation point color
        _guard.UpdateVisuals(_stateColor, _guard.GetMeshTuple);
        _guard.Agent.isStopped = false;

        if (_guard.Waypoints.Length > 0)
        {
            _guard.Agent.SetDestination(_guard.Waypoints[_guard.CurrentWaypointIndex].position);
        }
    }

    public void Execute()
    {
        // saw that pesky player again, resume chase
        if (_guard.CanSeeTarget())
        {
            _guard.Machine.ChangeState(new GuardChasingState(_guard));
            return;
        }

        // back to normal patrol
        if (!_guard.Agent.pathPending && _guard.Agent.remainingDistance < _guard.WaypointTolerance)
        {
            _guard.Machine.ChangeState(new GuardPatrollingState(_guard));
        }
    }

    public void Exit() { }
}