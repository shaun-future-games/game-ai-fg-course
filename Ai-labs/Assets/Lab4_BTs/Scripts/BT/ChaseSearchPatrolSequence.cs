using System;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase Search Patrol", story: "executes right sequences", category: "Flow", id: "196630004393f9b6f3b2218ad22b210c")]
public partial class ChaseSearchPatrolSequence : Composite
{
    [SerializeReference] public Node Chase;
    [SerializeReference] public Node Search;
    [SerializeReference] public Node Patrol;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

