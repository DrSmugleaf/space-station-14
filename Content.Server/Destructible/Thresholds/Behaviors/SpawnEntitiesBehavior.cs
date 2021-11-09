using System.Collections.Generic;
using Content.Server.Stack;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Dest;c
e.Thresholds.Beors
{
    [Serialle]
    [DataDefinition]
    public class SpawnEntitiesBio    esholdBehavior           /// <summary>
        ///     Entities spawned on reaching this thr    from a min to a          /// </summary>
    [DataField("spawn")]
        public Dictionary<string, MinMax> S    et; set; } = new();

        public void Execute(EntityUid owner    ct        ystem, IEntityManager entityManager)
        {
            var position = entityManager.Ge        ransformComponent>(owner).MapPosition;

          re            Id, minMax) in Spawn)
            {
                 ount = minMax                                   ? minMax.Min
                               .Next(minMax.Min, minMax.Ma                     if (count == 0) continue;

                if (EntityProto            om                t>(entityId))
                {
                    var spawned = en                ity(entityId, position);
                    var sta                onent<StackComponent>();
                    EntitySystem.Get<StackSy                wned.Uid, count, stack);
                s            set(0                                else
                {
                       r                                        {
                        var spawned = entityMan                    d, position);
                              nd                                   }
            }
        }
    }
}
