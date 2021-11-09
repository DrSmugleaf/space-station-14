using Content.Server.Alert;
using Content.Shared.Alert;
using Content.Shared.MobState.State;
using Content.Shared.StatusEffect;
using Robust.Shared.GameObjects;

namespace Content.Server.MobState.States;

public class DeadMobState : SharedDeadMobState
{
    public override void EnterState(EntityUid uid, IEntityManager entityManager)
    {
        base.EnterState(uid, entityManager);

        if (entityManager.TryGetComponent(uid, out ServerAlertsComponent? status))
        {
            status.ShowAlert(AlertType.HumanDead);
        }

        if (entityManager.TryGetComponent(uid, out StatusEffectsComponent? stun))
        {
            EntitySystem.Get<StatusEffectsSystem>().TryRemoveStatusEffect(uid, "Stun");
        }
    }
}