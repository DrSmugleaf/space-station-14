using System;
using System.Collections.Generic;
using Content.Shared.Chemistry.Components;
using Content.Shared.Eui;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration;

[Serializable, NetSerializable]
public class EditSolutionsEuiState : EuiStateBase
{
    public readonly EntityUid Target;
    public readonly Dictionary<string, Solution>? Solutions;

    public EditSolutionsEuiState(EntityUid target, Dictionary<string, Solution>? solutions)
    {
        Target = target;
        Solutions = solutions;
    }
}

public static class EditSolutionsEuiMsg
{
    [Serializable, NetSerializable]
    public sealed class Close : EuiMessageBase { }
}