using Content.Server.Traitor.Uplink.Components;
using Robust.Shared.GameObjects;

namespace Content.Server.Traitor.Uplink;

public class UplinkInitEvent : EntityEventArgs
{
    public UplinkComponent Uplink;

    public UplinkInitEvent(UplinkComponent uplink)
    {
        Uplink = uplink;
    }
}

public class UplinkRemovedEvent : EntityEventArgs
{
}