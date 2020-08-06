using System;
using System.Collections.Generic;
using Content.Shared.Damage;
using Content.Shared.GameObjects.Components.Body;
using Content.Shared.GameObjects.EntitySystems;
using Robust.Shared.Interfaces.GameObjects;

namespace Content.Shared.GameObjects.Components.Damage
{
    public interface IDamageableComponent : IComponent, IExAct
    {
        /// <summary>
        ///     Called when the entity's <see cref="IDamageableComponent"/> values change.
        ///     Of note is that a "deal 0 damage" call will still trigger this event
        ///     (including both damage negated by resistance or simply inputting 0 as
        ///     the amount of damage to deal).
        /// </summary>
        event Action<HealthChangedEventArgs> HealthChangedEvent;

        /// <summary>
        ///     List of all <see cref="DamageState">DamageStates</see> that
        ///     <see cref="CurrentDamageState"/> can be.
        /// </summary>
        List<DamageState> SupportedDamageStates { get; }

        /// <summary>
        ///     The <see cref="DamageState"/> currently representing this component.
        /// </summary>
        DamageState CurrentDamageState { get; }

        /// <summary>
        ///     Sum of all damages taken.
        /// </summary>
        int TotalDamage { get; }

        /// <summary>
        ///     Changes the specified <see cref="DamageType"/>, applying resistance values only if it is damage.
        /// </summary>
        /// <param name="damageType">Type of damage being changed.</param>
        /// <param name="amount">Amount of damage being received (positive for damage, negative for heals).</param>
        /// <param name="source">Entity that dealt or healed the damage.</param>
        /// <param name="ignoreResistances">Whether to ignore resistances. Healing always ignores resistances, regardless of this input.</param>
        /// <param name="extraParams">Extra parameters that some components may require, such as a specific limb to target.</param>
        /// <returns>False if the given damageType is not supported or improper HealthChangeParams were provided; true otherwise.</returns>
        bool ChangeDamage(DamageType damageType, int amount, IEntity source, bool ignoreResistances, HealthChangeParams extraParams = null);

        /// <summary>
        ///     Changes the specified <see cref="DamageClass"/>, applying resistance values only if it is damage.
        ///     Spreads amount evenly between the <see cref="DamageType">DamageTypes</see> represented by that class.
        /// </summary>
        /// <param name="damageClass">Class of damage being changed.</param>
        /// <param name="amount">Amount of damage being received (positive for damage, negative for heals).</param>
        /// <param name="source">Entity that dealt or healed the damage.</param>
        /// <param name="ignoreResistances">Whether to ignore resistances. Healing always ignores resistances, regardless of this input.</param>
        /// <param name="extraParams">Extra parameters that some components may require, such as a specific limb to target.</param>
        /// <returns>Returns false if the given damageClass is not supported or improper HealthChangeParams were provided; true otherwise.</returns>
        bool ChangeDamage(DamageClass damageClass, int amount, IEntity source, bool ignoreResistances, HealthChangeParams extraParams = null);

        /// <summary>
        ///     Forcefully sets the specified <see cref="DamageType"/> to the given value, ignoring resistance values.
        /// </summary>
        /// <param name="damageType">Type of damage being changed.</param>
        /// <param name="newValue">New damage value to be set.</param>
        /// <param name="source">Entity that set the new damage value.</param>
        /// <param name="extraParams">Extra parameters that some components may require, such as a specific limb to target.</param>
        /// <returns>Returns false if the given damageType is not supported or improper HealthChangeParams were provided; true otherwise.</returns>
        bool SetDamage(DamageType damageType, int newValue, IEntity source, HealthChangeParams extraParams = null);

        /// <summary>
        ///     Sets all damage values to zero.
        /// </summary>
        void HealAllDamage();

        /// <summary>
        ///     Invokes the HealthChangedEvent with the current values of health.
        /// </summary>
        void ForceHealthChangedEvent();

        void IExAct.OnExplosion(ExplosionEventArgs eventArgs)
        {
            var damage = eventArgs.Severity switch
            {
                ExplosionSeverity.Light => 20,
                ExplosionSeverity.Heavy => 60,
                ExplosionSeverity.Destruction => 250,
                _ => throw new ArgumentOutOfRangeException()
            };

            ChangeDamage(DamageType.Piercing, damage, null, false);
            ChangeDamage(DamageType.Heat, damage, null, false);
        }
    }

    /// <summary>
    ///     Data class with information on how to damage a <see cref="IDamageableComponent"/>.
    ///     While not necessary to damage for all instances, classes such as
    ///     <see cref="SharedBodyManagerComponent"/> may require it for extra data
    ///     (such as selecting which limb to target).
    /// </summary>
    public class HealthChangeParams : EventArgs
    {
    }

    /// <summary>
    ///     Data class with information on how the <see cref="DamageType"/>
    ///     values of a <see cref="IDamageableComponent"/> have changed.
    /// </summary>
    public class HealthChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     Reference to the <see cref="IDamageableComponent"/> that invoked the event.
        /// </summary>
        public readonly IDamageableComponent DamageableComponent;

        /// <summary>
        ///     List containing data on each <see cref="DamageType"/> that was changed.
        /// </summary>
        public readonly List<HealthChangeData> HealthData;

        public HealthChangedEventArgs(IDamageableComponent damageableComponent, List<HealthChangeData> healthData)
        {
            DamageableComponent = damageableComponent;
            HealthData = healthData;
        }
    }

    /// <summary>
    ///     Data class with information on how the value of a
    ///     single <see cref="DamageType"/> has changed.
    /// </summary>
    public struct HealthChangeData
    {
        /// <summary>
        ///     Type of damage that changed.
        /// </summary>
        public DamageType Type;

        /// <summary>
        ///     The new current value for that damage.
        /// </summary>
        public int NewValue;

        /// <summary>
        ///     How much the health value changed from its last value (negative is heals, positive is damage).
        /// </summary>
        public int Delta;

        public HealthChangeData(DamageType type, int newValue, int delta)
        {
            Type = type;
            NewValue = newValue;
            Delta = delta;
        }
    }
}
