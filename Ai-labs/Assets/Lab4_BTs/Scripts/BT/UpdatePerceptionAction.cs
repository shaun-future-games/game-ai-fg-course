using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update Perception", story: "Guard Searches for Player", category: "Action/Find", id: "b6e94327bf05b8457f2ed7369328cd3b")]
public partial class UpdatePerceptionAction : Action
{

    [SerializeReference]
    public BlackboardVariable<GameObject> Target;
    [SerializeReference]
    public BlackboardVariable<bool> HasLineOfSight;
    [SerializeReference]
    public BlackboardVariable<Vector3> LastKnownPosition;
    [SerializeReference]
    public BlackboardVariable<float> TimeSinceLastSeen;

    protected override Node.Status OnStart()
    {
        // Ensure we have sane defaults.
        if (TimeSinceLastSeen != null && TimeSinceLastSeen.Value < 0f)
            TimeSinceLastSeen.Value = 9999f;
        return Node.Status.Success;
    }
    protected override Node.Status OnUpdate()
    {
        var sensors = GameObject != null ?
        GameObject.GetComponent<GuardSensor>() : null;
        if (sensors == null)
        {
            // No sensors attached -> treat as "can't see anything"
            if (HasLineOfSight != null) HasLineOfSight.Value = false;
            if (TimeSinceLastSeen != null)
                TimeSinceLastSeen.Value += Time.deltaTime;
            return Node.Status.Success;
        }
        bool sensed = sensors.TrySenseTarget(
            out GameObject sensedTarget,
            out Vector3 sensedPos,
            out bool hasLOS
        );
        if (sensed && hasLOS)
        {
            if (Target != null) Target.Value = sensedTarget;
            if (HasLineOfSight != null) HasLineOfSight.Value = true;
            if (LastKnownPosition != null)
                LastKnownPosition.Value = sensedPos;
            if (TimeSinceLastSeen != null)
                TimeSinceLastSeen.Value = 0f;
        }
        else
        {
            // Keep Target as-is (we "remember" who we were chasing),
            // but mark that we don't currently have LOS.
            if (HasLineOfSight != null) HasLineOfSight.Value = false;
            if (TimeSinceLastSeen != null)
                TimeSinceLastSeen.Value += Time.deltaTime;
        }
        // This node is a fast "service-like" update; it finishes immediately each tick.
        return Node.Status.Success;
    }
}

