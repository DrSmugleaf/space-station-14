using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Ghost.Components;

[RegisterComponent]
[ComponentReference(typeof(IGhostOnMove))]
public class GhostOnMoveComponent : Component,IGhostOnMove
{
    public override string Name => "GhostOnMove";

    [DataField("canReturn")] public bool CanReturn { get; set; } = true;
}