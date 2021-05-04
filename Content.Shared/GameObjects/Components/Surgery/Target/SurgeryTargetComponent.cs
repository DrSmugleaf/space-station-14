﻿using System.Collections.Generic;
using Content.Shared.GameObjects.Components.Surgery.Operation;
using Content.Shared.GameObjects.Components.Surgery.Surgeon;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Players;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Shared.GameObjects.Components.Surgery.Target
{
    [RegisterComponent]
    public class SurgeryTargetComponent : Component
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public override string Name => "Surgery";
        public override uint? NetID => ContentNetIDs.SURGERY_TARGET;

        [ViewVariables]
        [DataField("tags")]
        private readonly List<SurgeryTag> _surgeryTags = new();

        private EntityUid? _surgeon;

        [ViewVariables]
        public SurgeonComponent? Surgeon
        {
            get => _surgeon == null
                ? null
                : Owner.EntityManager.GetEntity(_surgeon.Value).GetComponent<SurgeonComponent>();
            set
            {
                if (_surgeon == value?.Owner.Uid)
                {
                    return;
                }

                _surgeon = value?.Owner.Uid;
                Dirty();
            }
        }

        [ViewVariables]
        [DataField("current")]
        private string? _operationId;

        [ViewVariables]
        public SurgeryOperationPrototype? Operation
        {
            get => _operationId == null
                ? null
                : _prototypeManager.Index<SurgeryOperationPrototype>(_operationId);
            set
            {
                if (_operationId == value?.ID)
                {
                    return;
                }

                _operationId = value?.ID;
                Dirty();
            }
        }

        [ViewVariables]
        public IEnumerable<SurgeryOperationPrototype> PossibleSurgeries =>
            _prototypeManager.EnumeratePrototypes<SurgeryOperationPrototype>();

        public override ComponentState GetComponentState(ICommonSession player)
        {
            return new SurgeryTargetComponentState(_surgeon, _operationId);
        }

        public override void HandleComponentState(ComponentState? curState, ComponentState? nextState)
        {
            base.HandleComponentState(curState, nextState);

            if (curState is not SurgeryTargetComponentState state)
            {
                return;
            }

            _surgeon = state.Surgeon == null
                ? null
                : Owner.EntityManager
                    .GetEntity(state.Surgeon.Value)
                    .EnsureComponent<SurgeonComponent>()
                    .Owner
                    .Uid;

            _operationId = state.Operation;
        }

        public bool CanAddSurgeryTag(SurgeryTag tag)
        {
            if (Operation == null ||
                Operation.Steps.Count <= _surgeryTags.Count)
            {
                return false;
            }

            var nextStep = Operation.Steps[_surgeryTags.Count];
            if (!nextStep.Necessary(this) || nextStep.Id != tag.Id)
            {
                return false;
            }

            return true;
        }

        public bool TryAddSurgeryTag(SurgeryTag tag)
        {
            if (!CanAddSurgeryTag(tag))
            {
                return false;
            }

            _surgeryTags.Add(tag);
            CheckCompletion();

            return true;
        }

        public bool HasSurgeryTag(SurgeryTag tag)
        {
            return _surgeryTags.Contains(tag);
        }

        public bool TryRemoveSurgeryTag(SurgeryTag tag)
        {
            if (_surgeryTags.Count == 0 ||
                _surgeryTags[^1] != tag)
            {
                return false;
            }

            _surgeryTags.RemoveAt(_surgeryTags.Count - 1);
            return true;
        }

        private void CheckCompletion()
        {
            if (Operation == null ||
                Operation.Steps.Count > _surgeryTags.Count)
            {
                return;
            }

            var offset = 0;

            for (var i = 0; i < _surgeryTags.Count; i++)
            {
                var step = Operation.Steps[i + offset];

                if (!step.Necessary(this))
                {
                    offset++;
                    step = Operation.Steps[i + offset];
                }

                var tag = _surgeryTags[i];

                if (tag != step.Id)
                {
                    return;
                }
            }

            Operation.Effect?.Execute(this);
        }
    }
}
