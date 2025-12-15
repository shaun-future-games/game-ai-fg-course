using System.Collections.Generic;
using UnityEngine;

public class SteeringAgent : MonoBehaviour
{

    [Header("Movement")]
    public float maxSpeed = 5.0f;
    public float maxForce = 10.0f; // limit how fast the direction changes (turning radius)

    [Header("Arrive")]
    public float slowingRadius = 3.0f;

    [Header("Separation")]
    public float separationRadius = 1.5f;
    public float serarationStrenght = 5.0f;

    [Header("Weights")]
    public float arriveWeight = 1.0f;
    public float separationWeight = 1.0f;

    [Header("Debug")]
    public bool drawDebug = true;

    private Vector3 velocity = Vector3.zero;
    
    // optional target for seek / arrive
    public Transform target = null;

    // static list so agenst can find each other
    public static List<SteeringAgent> allAgents = new List<SteeringAgent>();


    private void OnEnable()
    {
        allAgents.Add(this);
    }

    private void OnDisable()
    {
        allAgents.Remove(this);
    }

    private void Awake()
    {
        target = GameObject.Find("Target").transform;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 1. Calculate Steering Force
        Vector3 totalSteering = Vector3.zero;

        
        if (target != null)
        {
            totalSteering += Arrive(target.position, slowingRadius) * arriveWeight;
        }

        // separate only if there are neighbors
        if (allAgents.Count > 1)
        {
            totalSteering += Separation(separationRadius, serarationStrenght) * separationWeight;
        }

        totalSteering = Vector3.ClampMagnitude(totalSteering, maxForce);

        // 3. Apply Steering to Velocitiy (integration)
        // Acceleration = Force / Mass. ( assume mass = 1 )
        // Velocity Change = Acceleration * Time
        velocity += totalSteering * Time.deltaTime;

        // 4. Limit Velocity
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        // 5. Move Agent
        transform.position += velocity * Time.deltaTime;

        // 6. Face Movement Direction
        if (velocity.sqrMagnitude > 0.0001f)
        {
            transform.forward = velocity.normalized;
        }
    }

    // behavior stubs
    public Vector3 Seek(Vector3 targetPos) 
    {
        Vector3 toTarget = targetPos - transform.position;

        // if we are already there, stop steering
        if (toTarget.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        // Desired Velocity: Full speed towards target
        Vector3 desiredVelocity = toTarget.normalized * maxSpeed;

        // Renolds' steering formula
        return desiredVelocity - velocity;
    }

    public Vector3 Arrive(Vector3 targetPos, float slowRadius)
    {
        Vector3 toTarget = targetPos - transform.position;
        float dist = toTarget.magnitude;

        if (dist < 0.0001f)
        {
            return Vector3.zero;
        }

        float desiredSpeed = maxSpeed;

        // Ramp down speed if within radius
        if (dist < slowingRadius)
        {
            desiredSpeed = maxSpeed * (dist / slowingRadius);
        }

        Vector3 desiredVel = toTarget.normalized * desiredSpeed;
        return desiredVel - velocity;
    }

    public Vector3 Separation(float separationRadius, float separationStrength)
    {
        Vector3 force = Vector3.zero;
        int neighborCount = 0;

        foreach (SteeringAgent other in allAgents)
        {
            if (other == this)
                continue;

            Vector3 toMe = transform.position - other.transform.position;
            float dist = toMe.magnitude;

            // if they are within my personal bubble
            if (dist > 0f && dist < separationRadius)
            {
                // weight: 1/dist means closer neigbors push much harder
                force += toMe.normalized / dist;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            force /= neighborCount; // average direction

            // convert "move away" direction into a steering force
            force = force.normalized * maxSpeed;
            force = force - velocity;
            force *= separationStrength;
        }

        return force;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebug)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
    }
}
