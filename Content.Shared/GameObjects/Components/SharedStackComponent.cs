﻿using System;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Reflection;
using Robust.Shared.Prototypes;
using Robust.Shared.Prototypes.DataClasses.Attributes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Shared.GameObjects.Components
{
    [DataClass(typeof(SharedStackComponentDataClass))]
    public abstract class SharedStackComponent : Component
    {
        private const string SerializationCache = "stack";

        public sealed override string Name => "Stack";
        public sealed override uint? NetID => ContentNetIDs.STACK;

        [YamlField("count")]
        private int _count = 50;
        [YamlField("max")]
        private int _maxCount = 50;

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

        [ViewVariables]
        [DataClassTarget("stacktype")]
        public object StackType { get => _stackType == null ? Owner.Prototype.ID : _stackType; private set => _stackType = value; }

        private object _stackType;

        public override ComponentState GetComponentState()
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

    public enum StackType
    {
        Metal,
        Glass,
        ReinforcedGlass,
        Plasteel,
        Cable,
        Wood,
        MVCable,
        HVCable,
        Gold,
        Plasma,
        Ointment,
        Gauze,
        Brutepack,
        FloorTileSteel,
        FloorTileCarpet,
        FloorTileWhite,
        FloorTileDark,
        FloorTileWood,
        MetalRod
    }
}
