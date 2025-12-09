using UnityEngine;
using UnityEngine.AI;


public enum GuardState
{
    Patrolling,
    Chasing,
    ReturningToPatrol
}

public class GuardPatrol : MonoBehaviour
{
    public Transform[] Waypoints;
    public float WaypointTolerance = 0.5f;

    // state machine variables
    public GuardState CurrentState = GuardState.Patrolling;
    public Transform Target;
    public float ChaseRange = 5f;
    public float LostRange = 7f;

    int _currentIndex = 0;
    NavMeshAgent _agent;

    // messing around with some flavor
    private Color _colorRed = Color.red;
    private Color _colorYellow = Color.yellow;
    private Color _colorOrange = new Color(1f, 0.5f, 0f);

    [SerializeField]
    private MeshRenderer _meshRendererDot;
    [SerializeField]
    private MeshRenderer _meshRendererExclamation;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        
    }


    /*
     * If the guard has a navmesh agent but there is no baked navmesh, 
     * the agent will not be able to know where it can move or calculate a path
     * 
     * the difference between a path and the movement along the path is the path is the line or curve the agent will move along
     * the movement along the path is the distance the agent will move along the path and the point along the path the agent is currently at
     */
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Waypoints.Length > 0)
        {
            _agent.SetDestination(Waypoints[_currentIndex].position);
        }
    }

    // Update is called once per frame
    void Update()
    {

        switch (CurrentState)
        {
            case GuardState.Patrolling:
                Patrol();
                break;
            case GuardState.Chasing:
                Chase();
                break;
            case GuardState.ReturningToPatrol:
                ReturnToPatrol();
                break;
        }
    }

    void Patrol()
    {
        if (Waypoints.Length == 0)
        {
            return;
        }
        if (_meshRendererDot != null && _meshRendererExclamation != null && 
            _meshRendererDot.material.color != _colorYellow && 
            _meshRendererExclamation.material.color != _colorYellow)
        {
            _meshRendererDot.material.color = _colorYellow;
            _meshRendererExclamation.material.color = _colorYellow;
        }    
        // check if we've reached the current waypoint
        if (!_agent.pathPending && _agent.remainingDistance < WaypointTolerance)
        {
            // Go to the next waypoint
            /*
             * Why do we check !_agent.pathfinding before reading remainingDistance?
             * The NavMeshAgent will stop moving if the pathfinding is disabled or the path is invalid.
             * What happens if we forget to use % waypoints.Length when incrementing the index
             * If % is not used we'll get an out of range exception
             */
            _currentIndex = (_currentIndex + 1) % Waypoints.Length;
            _agent.SetDestination(Waypoints[_currentIndex].position);
        }

        // check if target is within chase range
        if (Target != null && Vector3.Distance(Target.position, transform.position) < ChaseRange)
        {
            CurrentState = GuardState.Chasing;
        }
    }

    void Chase()
    {
        if (Target != null && !_agent.pathPending)
        {
            _agent.SetDestination(Target.position);
        }
        if (_meshRendererDot != null && _meshRendererExclamation != null &&
            _meshRendererDot.material.color != _colorRed &&
            _meshRendererExclamation.material.color != _colorRed)
        {
            _meshRendererDot.material.color = _colorRed;
            _meshRendererExclamation.material.color = _colorRed;
        }
        // if target gets out of range
        if (Target != null && Vector3.Distance(Target.position, transform.position) > LostRange)
        {
            CurrentState = GuardState.ReturningToPatrol;
        }
    }

    void ReturnToPatrol()
    {
        if (Waypoints.Length > 0 && !_agent.pathPending)
        {
            _agent.SetDestination(Waypoints[_currentIndex].position);
        }
        if (_meshRendererDot != null && _meshRendererExclamation != null &&
            _meshRendererDot.material.color != _colorOrange &&
            _meshRendererExclamation.material.color != _colorOrange)
        {
            _meshRendererDot.material.color = _colorOrange;
            _meshRendererExclamation.material.color = _colorOrange;
        }
        // return to closest waypoint then return to patrol
        if (!_agent.pathPending && _agent.remainingDistance < WaypointTolerance)
        {
            CurrentState = GuardState.Patrolling;
        }
    }
}
