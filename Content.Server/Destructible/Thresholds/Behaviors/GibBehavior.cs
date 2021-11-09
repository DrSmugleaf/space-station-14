using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Destructible.Thresholds.Behaviors;

[UsedImplicitly]
[DataDefinition]
public class GibBehavior : IThresholdBehavior
{
    [DataField("recursive")] private bool _recursive = true;

    public void Execute(EntityUid owner, DestructibleSystem system,     an        anager)
        {
            if (entityManager.TryGetComponent(owner, out Share        nt                 {
                       ec    
     }
        }
    }
}
