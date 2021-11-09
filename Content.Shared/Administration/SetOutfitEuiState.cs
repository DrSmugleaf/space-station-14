using System;
using Content.Shared.Eui;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration;

[Serializable, NetSerializable]
public class SetOutfitEuiState : EuiStateBase
{
    public EntityUid TargetEntityId;
}