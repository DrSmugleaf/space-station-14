using System;
using Content.Shared.Stacks;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Players;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.ViewVariables;

namespace Content.Shared.GameObjects.Components
{
    public abstract class SharedStackComponent : Component
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager;

        private const string SerializationCache = "stack";

        public sealed override string Name => "Stack";
        public sealed override uint? NetID => ContentNetIDs.STACK;

        private int _count;
        private int _maxCount;

        [ViewVariables(VVAccess.ReadWrite)]
        public virtual int Count
        {
            get => _count;
            set
            {
                _count = value;
                if (_count <= 0)
                {
                    Owner.Delete();
                }

                Dirty();
            }
        }

        [ViewVariables]
        public int MaxCount
        {
            get => _maxCount;
            private set
            {
                _maxCount = value;
                Dirty();
            }
        }

        [ViewVariables] public int AvailableSpace => MaxCount - Count;

        [ViewVariables] public string StackTypeId { get; private set; }

        public StackPrototype StackType => IoCManager.Resolve<IPrototypeManager>().Index<StackPrototype>(StackTypeId);

        public override void ExposeData(ObjectSerializer serializer)
        {
            serializer.DataFieldCached(ref _maxCount, "max", 50);
            serializer.DataFieldCached(ref _count, "count", MaxCount);

            if (serializer.Writing)
            {
                return;
            }

            if (serializer.TryGetCacheData(SerializationCache, out string stackType))
            {
                StackTypeId = stackType;
                return;
            }

            stackType = serializer.TryReadDataFieldCached("stackType", out string raw)
                ? raw
                : Owner.Prototype.ID;

            serializer.SetCacheData(SerializationCache, stackType);
            StackTypeId = stackType;
        }

        protected override void Startup()
        {
            base.Startup();

            _prototypeManager.Index<StackPrototype>(StackTypeId);
        }

        public override ComponentState GetComponentState(ICommonSession player)
        {
            return new StackComponentState(Count, MaxCount);
        }

        public override void HandleComponentState(ComponentState curState, ComponentState nextState)
        {
            if (curState is not StackComponentState cast)
            {
                return;
            }

            Count = cast.Count;
            MaxCount = cast.MaxCount;
        }


        [Serializable, NetSerializable]
        private sealed class StackComponentState : ComponentState
        {
            public int Count { get; }
            public int MaxCount { get; }

            public StackComponentState(int count, int maxCount) : base(ContentNetIDs.STACK)
            {
                Count = count;
                MaxCount = maxCount;
            }
        }
    }
}
