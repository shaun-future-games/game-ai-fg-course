using UnityEngine;

namespace Lab5
{
    public class MoveToWeaponAction : GoapActionBase
    {
        public float arriveDistance = 0.7f;
        void Reset()
        {
            actionName = "Move To Weapon";
            cost = 1f;
            preMask = GoapBits.Mask(GoapFact.WeaponExists);
            addMask = GoapBits.Mask(GoapFact.AtWeapon);
            delMask = 0;
        }
        public override bool CheckProcedural(GoapContext ctx)
        {
            return ctx.Weapon != null &&
            ctx.Weapon.gameObject.activeInHierarchy;
        }
        public override void OnEnter(GoapContext ctx)
        {
            ctx.Agent.SetDestination(ctx.Weapon.position);
        }
        public override GoapStatus Tick(GoapContext ctx)
        {
            if (ctx.Weapon == null ||
            !ctx.Weapon.gameObject.activeInHierarchy)
                return GoapStatus.Failure;
            if (ctx.Agent.pathPending) return GoapStatus.Running;
            if (ctx.Agent.remainingDistance <= arriveDistance)
                return GoapStatus.Success;
            return GoapStatus.Running;
        }
    }
}


