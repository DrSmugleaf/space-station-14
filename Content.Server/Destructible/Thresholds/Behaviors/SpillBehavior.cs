using Content.Server.Fluids.Components;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Destructible.Thresholds.Behaviors;

[UsedImplicitly]
[DataDefinition]
public class SpillBehavior : IThresholdBehavior
{
    [DataField("solution")]
    public string? Solution;

    /// <summary>
    /// If there is a SpillableComponent on IEntity owner use it to create a puddle/smear.
    /// Or whatever solution is specified in the behavior itself.
    /// If none are available do nothing.
    /// </summary>
    /// <param name="owner">Entity on which behavior is executed</param>
    /// <param name="system">system calling the behavior</param>
    /// <param name="entityManager"></param>
    public void Execute(EntityUid owner, DestructibleSystem system,     an        anager)
        {
            var solutionContainerSystem = EntitySystem.Get        tainerSystem>();

            var coordinates = entityManager.GetComponent<TransformComponent        rdinates;

            if (entityManager.TryGetComponent(owner, out SpillableComponent? spillab                            solutionContainerSystem.TryGetSolution(owner, spillableComponen                                out var        n)                            compSolution.SpillAt(coordinates, "Puddle        e)          }
            else if (Solu                                 solutionContainerSystem.TryGetSolution(owner, Solution, out var beh        n)                            behaviorSolution.SpillAt(coordinates, "Puddle        e)      }      }
    }
}
