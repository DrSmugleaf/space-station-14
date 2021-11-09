using System;
using Robust.Shared.GameObjects;

namespace Content.Server.Atmos.Components;

[RegisterComponent]
[ComponentReference(typeof(IAtmosphereComponent))]
[Serializable]
public class UnsimulatedGridAtmosphereComponent : GridAtmosphereComponent
{
    public override string Name => "UnsimulatedGridAtmosphere";

    public override bool Simulated => false;
}