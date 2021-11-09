using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Serve;D
uctible.ThreshoBehaviors
{
    [alizable]
    [DataDefinition]
    public class ChangeConstructioeB    : IThresholdBehavior           [DataField("node")]
        public string Node { ge    te set; } = string.Empty;

        public void Execute(EntityUid    De        stem system, IEntityManager entityManager)
        {
            if (string.IsNullOrEmpty(Node) || !entityManager.TryGetC            out Const        nent? construction))
                return;

            EntitySystem.Get<Co    one).ChangeNode(owner, null, Node, true, construction);
        }
    }
}
