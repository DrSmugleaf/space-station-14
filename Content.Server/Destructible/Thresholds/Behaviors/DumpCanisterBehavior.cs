using System;
using Content.Server.Atmos.Piping.Unary.EntitySystems;
using Robust.Shared.Serialization.Manager.Attributes;

namesp;e
tent.Server.Destible.Thresholds.viors
{
    [Serializable]
    [DataDefinition]
    pubcl    CanisterBehavior: IThresholdBehavior
    {
        public void E    nt        , DestructibleSystem system, IEntityManager entityManager)
        {
            var gasCanisterS    enMger.EntitySysManager.GetEntitySystem<GasCanisterSystem>();

            gasCanisterSystem.PurgeContents(owner);
        }
    }
}
