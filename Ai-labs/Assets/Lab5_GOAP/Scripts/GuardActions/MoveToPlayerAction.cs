using UnityEngine;

namespace Lab5
{
    public class MoveToPlayerAction : GoapActionBase
    {
        public float arriveDistance = 1.2f;
        void Reset()
        {
            actionName = "Move To Player";
            cost = 1f;
            preMask = GoapBits.Mask(GoapFact.SeesPlayer);
            addMask = GoapBits.Mask(GoapFact.AtPlayer);
            delMask = 0;
        }
        public override bool CheckProcedural(GoapContext ctx)
        {
            return ctx.Player != null;
        }
        public override GoapStatus Tick(GoapContext ctx)
        {
            if (ctx.Player == null) return GoapStatus.Failure;
            // If we lose sight, fail → triggers replanning (as required)
            if (ctx.Sensors != null && !ctx.Sensors.SeesPlayer)
                return GoapStatus.Failure;
            ctx.Agent.SetDestination(ctx.Player.position);
            if (ctx.Agent.pathPending) return GoapStatus.Running;
            if (ctx.Agent.remainingDistance <= arriveDistance)
                return GoapStatus.Success;
            return GoapStatus.Running;
        }
    }
}

