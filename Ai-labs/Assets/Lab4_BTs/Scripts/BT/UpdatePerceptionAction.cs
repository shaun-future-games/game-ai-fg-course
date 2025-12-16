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
        if (TimeSinceLastSeen != null && TimeSinceLastSeen.Value < 0f)
            TimeSinceLastSeen.Value = 9999f;

        return Node.Status.Running;
    }

    protected override Node.Status OnUpdate()
    {
        var sensors = GameObject != null ? GameObject.GetComponent<GuardSensor>() : null;

        if (sensors == null)
        {
            if (HasLineOfSight != null) HasLineOfSight.Value = false;
            if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value += Time.deltaTime;

            return Node.Status.Running;
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
            if (LastKnownPosition != null) LastKnownPosition.Value = sensedPos;
            if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value = 0f;
        }
        else
        {
            if (HasLineOfSight != null) HasLineOfSight.Value = false;
            if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value += Time.deltaTime;
        }

        return Node.Status.Running;
    }
}