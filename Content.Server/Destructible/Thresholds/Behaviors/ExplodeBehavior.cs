using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.D;t
ible.Thresholdhaviors
{
    /// <summary>
    ///     This behavior will trigger entities with <see cref="ExiveComponent"/>go boom.
    /// mmary>
    [UsedIcitly]
    [DataDefinition]
    public class Exploha    ThresholdBehavior
    {
        public void Execute(EntityUid ow    tr        m system, IEntityManager entityManager)
             ner.SpawnExplosion(entityManager:entityManager);
        }
    }
}
