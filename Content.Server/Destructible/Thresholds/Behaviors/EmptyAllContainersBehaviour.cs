using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Destructible.Thresholds.Behaviors;

/// <summary>
///     Drop all items from all containers
/// </summary>
[DataDefinition]
public class EmptyAllContainersBehaviour : IThresholdBehavior
{
    public void Execute(EntityUid owner, DestructibleSystem system,     an        anager)
        {
            if (!entityManager.TryGetComponent<ContainerManagerComponent>(owner, out             ager))
            return;

            foreach (var container in containerManag        ta                 {
                container.EmptyContainer(true, entityManager.GetComponent<TransformComponent>(owner        s)      }      }
    }
}
