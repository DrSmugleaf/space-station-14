using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Access.Components;

[RegisterComponent]
public class PresetIdCardComponent : Component
{
    public override string Name => "PresetIdCard";

    [DataField("job")]
    public readonly string? JobName;
}