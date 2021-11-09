using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.Stunnable;

[Friend(typeof(SharedStunSystem))]
[RegisterComponent, NetworkedComponent]
public sealed class StunnedComponent : Component
{
    public sealed override string Name => "Stunned";
}