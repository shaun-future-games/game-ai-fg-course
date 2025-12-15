using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Clear Target", story: "Clears Target and resets LOS memory", category: "Action/Find", id: "3e66e3f3dfdd922f52ad15deb51808cc")]
public partial class ClearTargetAction : Action
{

    [SerializeReference]
    public BlackboardVariable<GameObject> Target;
    [SerializeReference]
    public BlackboardVariable<bool> HasLineOfSight;
    [SerializeReference]
    public BlackboardVariable<float> TimeSinceLastSeen;
    protected override Node.Status OnUpdate()
    {
        if (Target != null) Target.Value = null;
        if (HasLineOfSight != null) HasLineOfSight.Value = false;
        if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value = 9999f;
        return Node.Status.Success;
    }
}

