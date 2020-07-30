﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Body;
using Content.Server.Body.Network;
using Content.Server.GameObjects.Components.Damage;
using Content.Server.Interfaces.GameObjects.Components.Interaction;
using Content.Server.Mobs;
using Content.Shared.Body;
using Content.Shared.Body.BodyPart;
using Content.Shared.Body.BodyPart.BodyPartProperties.Movement;
using Content.Shared.Body.BodyPart.BodyPartProperties.Other;
using Content.Shared.Body.BodyPreset;
using Content.Shared.Body.BodyTemplate;
using Content.Shared.Damage;
using Content.Shared.GameObjects.Components.Movement;
using Robust.Shared.GameObjects;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.ViewVariables;

namespace Content.Server.GameObjects.Components.Body
{
    /// <summary>
    ///     Component representing a collection of <see cref="BodyPart">BodyParts</see> attached to each other.
    /// </summary>
    [RegisterComponent]
    [ComponentReference(typeof(BaseDamageableComponent))]
    public class BodyManagerComponent : BaseDamageableComponent, IBodyPartContainer
    {
        public sealed override string Name => "BodyManager";
#pragma warning disable CS0649
        [Dependency] private IPrototypeManager _prototypeManager;
#pragma warning restore

        [ViewVariables] private string _presetName;

        [ViewVariables] private readonly Dictionary<Type, BodyNetwork> _networks = new Dictionary<Type, BodyNetwork>();

        /// <summary>
        ///     All <see cref="BodyPart">BodyParts</see> with <see cref="LegProperty">LegProperties</see>
        ///     that are currently affecting move speed, mapped to how big that leg they're on is.
        /// </summary>
        [ViewVariables]
        private readonly Dictionary<BodyPart, float> _activeLegs = new Dictionary<BodyPart, float>();

        /// <summary>
        ///     The <see cref="BodyTemplate"/> that this BodyManagerComponent is adhering to.
        /// </summary>
        [ViewVariables]
        public BodyTemplate Template { get; private set; }

        /// <summary>
        ///     Maps <see cref="BodyTemplate"/> slot name to the <see cref="BodyPart"/> object filling it (if there is one).
        /// </summary>
        [ViewVariables]
        public Dictionary<string, BodyPart> PartDictionary { get; } = new Dictionary<string, BodyPart>();

        /// <summary>
        ///     List of all occupied slots in this body, taken from the values of _parts.
        /// </summary>
        public IEnumerable<string> AllSlots => Template.Slots.Keys;

        /// <summary>
        ///     List of all occupied slots in this body, taken from the values of _parts.
        /// </summary>
        public IEnumerable<string> OccupiedSlots => PartDictionary.Keys;

        /// <summary>
        ///     List of all <see cref="BodyPart">BodyParts</see> in this body, taken from the keys of _parts.
        /// </summary>
        private IEnumerable<BodyPart> Parts => PartDictionary.Values;

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);

            serializer.DataReadWriteFunction(
                "BaseTemplate",
                "bodyTemplate.Humanoid",
                template =>
                {
                    if (!_prototypeManager.TryIndex(template, out BodyTemplatePrototype templateData))
                    {
                        // Invalid prototype
                        throw new InvalidOperationException(
                            $"No {nameof(BodyTemplatePrototype)} found with name {template}");
                    }

                    Template = new BodyTemplate(templateData);
                },
                () => Template.Name);

            serializer.DataReadWriteFunction(
                "BasePreset",
                "bodyPreset.BasicHuman",
                preset =>
                {
                    if (!_prototypeManager.TryIndex(preset, out BodyPresetPrototype presetData))
                    {
                        // Invalid prototype
                        throw new InvalidOperationException(
                            $"No {nameof(BodyPresetPrototype)} found with name {preset}");
                    }

                    LoadBodyPreset(new BodyPreset(presetData));
                },
                () => _presetName);
        }

        private void LoadBodyPreset(BodyPreset preset)
        {
            _presetName = preset.Name;

            foreach (var slot in Template.Slots.Keys)
            {
                if (!preset.PartIDs.TryGetValue(slot, out var partId))
                {
                    // For each slot in our BodyManagerComponent's template, try and grab what the ID of what the preset says should be inside it.
                    continue; // If the preset doesn't define anything for it, continue.
                }

                // Get the BodyPartPrototype corresponding to the BodyPart ID we grabbed.
                if (!_prototypeManager.TryIndex(partId, out BodyPartPrototype newPartData))
                {
                    throw new InvalidOperationException($"No {nameof(BodyPart)} prototype found with ID {partId}");
                }

                // Try and remove an existing limb if that exists.
                if (PartDictionary.Remove(slot, out var removedPart))
                {
                    BodyPartRemoved(removedPart, slot);
                }

                // Add a new BodyPart with the BodyPartPrototype as a baseline to our BodyComponent.
                var addedPart = new BodyPart(newPartData);
                PartDictionary.Add(slot, addedPart);
                BodyPartAdded(addedPart, slot);
            }

            OnBodyChanged(); // TODO: Duplicate code
        }

        /// <summary>
        ///     Changes the current <see cref="BodyTemplate"/> to the given <see cref="BodyTemplate"/>. Attempts to keep previous
        ///     <see cref="BodyPart">BodyParts</see>
        ///     if there is a slot for them in both <see cref="BodyTemplate"/>.
        /// </summary>
        public void ChangeBodyTemplate(BodyTemplatePrototype newTemplate)
        {
            foreach (var part in PartDictionary)
            {
                // TODO: Make this work.
            }

            OnBodyChanged();
        }

        /// <summary>
        ///     This function is called by <see cref="BodySystem"/> every tick.
        /// </summary>
        public void Tick(float frameTime)
        {
            foreach (var part in PartDictionary.Values)
            {
                part.Tick(frameTime);
            }
        }

        /// <summary>
        ///     Called when the layout of this body changes.
        /// </summary>
        private void OnBodyChanged()
        {
            // Calculate movespeed based on this body.
            if (Owner.HasComponent<MovementSpeedModifierComponent>())
            {
                _activeLegs.Clear();
                var legParts = Parts.Where(x => x.HasProperty(typeof(LegProperty)));

                foreach (var part in legParts)
                {
                    var footDistance = DistanceToNearestFoot(this, part);

                    if (Math.Abs(footDistance - float.MinValue) > 0.001f)
                    {
                        _activeLegs.Add(part, footDistance);
                    }
                }

                CalculateSpeed();
            }
        }

        private void CalculateSpeed()
        {
            if (Owner.TryGetComponent(out MovementSpeedModifierComponent playerMover))
            {
                float speedSum = 0;
                foreach (var part in _activeLegs.Keys)
                {
                    if (!part.HasProperty<LegProperty>())
                    {
                        _activeLegs.Remove(part);
                    }
                }

                foreach (var (key, value) in _activeLegs)
                {
                    if (key.TryGetProperty(out LegProperty legProperty))
                    {
                        speedSum += legProperty.Speed *
                                    (1 + (float) Math.Log(value,
                                        1024.0)); // Speed of a leg = base speed * (1+log1024(leg length))
                    }
                }

                if (speedSum <= 0.001f || _activeLegs.Count <= 0) // Case: no way of moving. Fall down.
                {
                    StandingStateHelper.Down(Owner);
                    playerMover.BaseWalkSpeed = 0.8f;
                    playerMover.BaseSprintSpeed = 2.0f;
                }
                else // Case: have at least one leg. Set move speed.
                {
                    StandingStateHelper.Standing(Owner);

                    // Extra legs stack diminishingly. Final speed = speed sum/(leg count-log4(leg count))
                    playerMover.BaseWalkSpeed =
                        speedSum / (_activeLegs.Count - (float) Math.Log(_activeLegs.Count, 4.0));

                    playerMover.BaseSprintSpeed = playerMover.BaseWalkSpeed * 1.75f;
                }
            }
        }

        #region DamageableComponent Implementation

        // TODO: all of this

        public override int TotalDamage => 0;

        public override List<DamageState> SupportedDamageStates => null;

        public override DamageState CurrentDamageState { get; protected set; }

        public int TempDamageThing;

        public override bool ChangeDamage(DamageType damageType, int amount, IEntity source, bool ignoreResistances,
            HealthChangeParams extraParams = null)
        {
            if (amount > 0)
            {
                TempDamageThing++;
            }
            else if (amount < 0)
            {
                TempDamageThing--;
            }

            if (TempDamageThing >= 10)
            {
                CurrentDamageState = DamageState.Dead;
            }

            var data = new List<HealthChangeData> {new HealthChangeData(DamageType.Blunt, 0, 0)};
            TryInvokeHealthChangedEvent(new HealthChangedEventArgs(this, data));
            return true;
        }

        public override bool ChangeDamage(DamageClass damageClass, int amount, IEntity source, bool ignoreResistances,
            HealthChangeParams extraParams = null)
        {
            if (amount > 0)
            {
                TempDamageThing++;
            }
            else if (amount < 0)
            {
                TempDamageThing--;
            }

            if (TempDamageThing >= 10)
            {
                CurrentDamageState = DamageState.Dead;
            }

            var data = new List<HealthChangeData> {new HealthChangeData(DamageType.Blunt, 0, 0)};
            TryInvokeHealthChangedEvent(new HealthChangedEventArgs(this, data));
            return true;
        }

        public override bool SetDamage(DamageType damageType, int newValue, IEntity source,
            HealthChangeParams extraParams = null)
        {
            TempDamageThing = newValue;
            if (TempDamageThing > 10)
            {
                CurrentDamageState = DamageState.Dead;
            }

            var data = new List<HealthChangeData> {new HealthChangeData(DamageType.Blunt, TempDamageThing, 1)};
            TryInvokeHealthChangedEvent(new HealthChangedEventArgs(this, data));

            return true;
        }

        public override void HealAllDamage()
        {
            TempDamageThing = 0;
        }

        protected override void ForceHealthChangedEvent()
        {
            var data = new List<HealthChangeData> {new HealthChangeData(DamageType.Blunt, 0, 0)};
            TryInvokeHealthChangedEvent(new HealthChangedEventArgs(this, data));
        }

        #endregion

        #region BodyPart Functions

        /// <summary>
        ///    Recursively searches for if <see cref="target"/> is connected ot the center.
        ///    Not efficient (O(n^2)), but most bodies don't have a ton of <see cref="BodyPart">BodyParts</see>.
        /// </summary>
        /// <param name="target">The body part to find the center for.</param>
        /// <returns>True if it is connected to the center, false otherwise.</returns>
        private bool ConnectedToCenterPart(BodyPart target)
        {
            var searchedSlots = new List<string>();
            if (!TryGetSlotName(target, out var result))
            {
                return false;
            }

            return ConnectedToCenterPartRecursion(searchedSlots, result);
        }

        private bool ConnectedToCenterPartRecursion(List<string> searchedSlots, string slotName)
        {
            TryGetBodyPart(slotName, out var part);

            if (part == null)
            {
                return false;
            }

            if (part == GetCenterBodyPart())
            {
                return true;
            }

            searchedSlots.Add(slotName);

            if (TryGetBodyPartConnections(slotName, out List<string> connections))
            {
                foreach (var connection in connections)
                {
                    if (!searchedSlots.Contains(connection) &&
                        ConnectedToCenterPartRecursion(searchedSlots, connection))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Finds the central <see cref="BodyPart"/>, if any, of this body based on the <see cref="BodyTemplate"/>.
        ///     For humans, this is the torso.
        /// </summary>
        /// <returns>The <see cref="BodyPart"/> if one exists, null otherwise.</returns>
        private BodyPart GetCenterBodyPart()
        {
            PartDictionary.TryGetValue(Template.CenterSlot, out var center);
            return center;
        }

        /// <summary>
        ///     Returns whether the given slot name exists within the current <see cref="BodyTemplate"/>.
        /// </summary>
        private bool SlotExists(string slotName)
        {
            return Template.SlotExists(slotName);
        }

        /// <summary>
        ///    Finds the <see cref="BodyPart"/> in the given <see cref="slotName"/> if one exists.
        /// </summary>
        /// <param name="slotName">The slot to search in.</param>
        /// <param name="result">The body part in that slot, if any.</param>
        /// <returns>True if found, false otherwise.</returns>
        private bool TryGetBodyPart(string slotName, [NotNullWhen(true)] out BodyPart result)
        {
            return PartDictionary.TryGetValue(slotName, out result);
        }

        /// <summary>
        ///    Finds the slotName that the given <see cref="BodyPart"/> resides in.
        /// </summary>
        /// <param name="part">The <see cref="BodyPart"/> to find the slot for.</param>
        /// <param name="result">The slot found, if any.</param>
        /// <returns>True if a slot was found, false otherwise</returns>
        private bool TryGetSlotName(BodyPart part, [NotNullWhen(true)] out string result)
        {
            // We enforce that there is only one of each value in the dictionary,
            // so we can iterate through the dictionary values to get the key from there.
            result = PartDictionary.FirstOrDefault(x => x.Value == part).Key;
            return result != null;
        }

        /// <summary>
        ///     Finds the <see cref="BodyPartType"/> in the given <see cref="slotName"/> if one exists.
        /// </summary>
        /// <param name="slotName">The slot to search in.</param>
        /// <param name="result">The <see cref="BodyPartType"/> of that slot, if any.</param>
        /// <returns>True if found, false otherwise.</returns>
        public bool TryGetSlotType(string slotName, out BodyPartType result)
        {
            return Template.Slots.TryGetValue(slotName, out result);
        }

        /// <summary>
        ///    Finds the names of all slots connected to the given <see cref="slotName"/> for the template.
        /// </summary>
        /// <param name="slotName">The slot to search in.</param>
        /// <param name="connections">The connections found, if any.</param>
        /// <returns>True if the connections are found, false otherwise.</returns>
        private bool TryGetBodyPartConnections(string slotName, [NotNullWhen(true)] out List<string> connections)
        {
            return Template.Connections.TryGetValue(slotName, out connections);
        }

        /// <summary>
        ///     Grabs all occupied slots connected to the given slot, regardless of whether the given slotName is occupied. Returns
        ///     true if successful, false if there was an error or no connected BodyParts were found.
        /// </summary>
        public bool TryGetBodyPartConnections(string slotName, out List<BodyPart> result)
        {
            result = null;
            if (!Template.Connections.TryGetValue(slotName, out var connections))
            {
                return false;
            }

            var toReturn = new List<BodyPart>();
            foreach (var connection in connections)
            {
                if (TryGetBodyPart(connection, out var bodyPartResult))
                {
                    toReturn.Add(bodyPartResult);
                }
            }

            if (toReturn.Count <= 0)
            {
                return false;
            }

            result = toReturn;
            return true;
        }

        /// <summary>
        ///     Grabs all occupied slots connected to the given slot, regardless of whether the given slotName is occupied. Returns
        ///     true if successful, false if there was an error or no connected BodyParts were found.
        /// </summary>
        private bool TryGetBodyPartConnections(BodyPart part, out List<BodyPart> result)
        {
            result = null;
            if (TryGetSlotName(part, out var slotName))
            {
                return TryGetBodyPartConnections(slotName, out result);
            }

            return false;
        }

        /// <summary>
        ///     Grabs all <see cref="BodyPart">BodyParts</see> of the given type in this body.
        /// </summary>
        public List<BodyPart> GetBodyPartsOfType(BodyPartType type)
        {
            var toReturn = new List<BodyPart>();

            foreach (var part in PartDictionary.Values)
            {
                if (part.PartType == type)
                {
                    toReturn.Add(part);
                }
            }

            return toReturn;
        }

        /// <summary>
        ///     Installs the given <see cref="BodyPart"/> into the given slot. Returns true if successful, false otherwise.
        /// </summary>
        public bool InstallBodyPart(BodyPart part, string slotName)
        {
            // Make sure the given slot exists
            if (!SlotExists(slotName))
            {
                return false;
            }

            // And that nothing is in it
            if (TryGetBodyPart(slotName, out _))
            {
                return false;
            }

            PartDictionary.Add(slotName, part);
            BodyPartAdded(part, slotName); // TODO: Sort this duplicate out
            OnBodyChanged();

            return true;
        }

        /// <summary>
        ///     Installs the given <see cref="DroppedBodyPartComponent"/> into the given slot, deleting the <see cref="IEntity"/>
        ///     afterwards. Returns true if successful, false otherwise.
        /// </summary>
        public bool InstallDroppedBodyPart(DroppedBodyPartComponent part, string slotName)
        {
            if (!InstallBodyPart(part.ContainedBodyPart, slotName))
            {
                return false;
            }

            part.Owner.Delete();
            return true;
        }


        /// <summary>
        ///     Disconnects the given <see cref="BodyPart"/> reference, potentially dropping other
        ///     <see cref="BodyPart">BodyParts</see>
        ///     if they were hanging off it. Returns the IEntity representing the dropped BodyPart.
        /// </summary>
        public IEntity DropBodyPart(BodyPart part)
        {
            if (!PartDictionary.ContainsValue(part))
            {
                return null;
            }

            if (part != null)
            {
                var slotName = PartDictionary.FirstOrDefault(x => x.Value == part).Key;
                PartDictionary.Remove(slotName);

                // Call disconnect on all limbs that were hanging off this limb.
                if (TryGetBodyPartConnections(slotName, out List<string> connections))
                {
                    // This loop is an unoptimized travesty. TODO: optimize to be less shit
                    foreach (var connectionName in connections)
                    {
                        if (TryGetBodyPart(connectionName, out var result) && !ConnectedToCenterPart(result))
                        {
                            DisconnectBodyPartByName(connectionName, true);
                        }
                    }
                }

                var partEntity = Owner.EntityManager.SpawnEntity("BaseDroppedBodyPart", Owner.Transform.GridPosition);
                partEntity.GetComponent<DroppedBodyPartComponent>().TransferBodyPartData(part);
                OnBodyChanged();
                return partEntity;
            }

            return null;
        }

        /// <summary>
        ///     Disconnects the given <see cref="BodyPart"/> reference, potentially dropping other
        ///     <see cref="BodyPart">BodyParts</see> if they were hanging off it.
        /// </summary>
        public void DisconnectBodyPart(BodyPart part, bool dropEntity)
        {
            if (!PartDictionary.ContainsValue(part))
            {
                return;
            }

            if (part != null)
            {
                var slotName = PartDictionary.FirstOrDefault(x => x.Value == part).Key;
                if (PartDictionary.Remove(slotName, out var partRemoved))
                {
                    BodyPartRemoved(partRemoved, slotName);
                }

                // Call disconnect on all limbs that were hanging off this limb.
                if (TryGetBodyPartConnections(slotName, out List<string> connections))
                {
                    // This loop is an unoptimized travesty. TODO: optimize to be less shit
                    foreach (var connectionName in connections)
                    {
                        if (TryGetBodyPart(connectionName, out var result) && !ConnectedToCenterPart(result))
                        {
                            DisconnectBodyPartByName(connectionName, dropEntity);
                        }
                    }
                }

                if (dropEntity)
                {
                    var partEntity =
                        Owner.EntityManager.SpawnEntity("BaseDroppedBodyPart", Owner.Transform.GridPosition);
                    partEntity.GetComponent<DroppedBodyPartComponent>().TransferBodyPartData(part);
                }

                OnBodyChanged();
            }
        }

        /// <summary>
        ///     Internal string version of DisconnectBodyPart for performance purposes. Yes, it is actually more performant.
        /// </summary>
        private void DisconnectBodyPartByName(string name, bool dropEntity)
        {
            if (!TryGetBodyPart(name, out var part))
            {
                return;
            }

            if (part != null)
            {
                if (PartDictionary.Remove(name, out var partRemoved))
                {
                    BodyPartRemoved(partRemoved, name);
                }

                if (TryGetBodyPartConnections(name, out List<string> connections))
                {
                    foreach (var connectionName in connections)
                    {
                        if (TryGetBodyPart(connectionName, out var result) && !ConnectedToCenterPart(result))
                        {
                            DisconnectBodyPartByName(connectionName, dropEntity);
                        }
                    }
                }

                if (dropEntity)
                {
                    var partEntity =
                        Owner.EntityManager.SpawnEntity("BaseDroppedBodyPart", Owner.Transform.GridPosition);
                    partEntity.GetComponent<DroppedBodyPartComponent>().TransferBodyPartData(part);
                }

                OnBodyChanged();
            }
        }

        #endregion

        #region BodyNetwork Functions

        /// <summary>
        ///     Attempts to add a <see cref="BodyNetwork"/> of the given type to this body. Returns true if successful, false
        ///     if there was an error (such as passing in an invalid type or a network of that type already existing).
        /// </summary>
        public bool AddBodyNetwork(Type networkType)
        {
            if (!networkType.IsSubclassOf(typeof(BodyNetwork)))
            {
                return false;
            }

            if (!_networks.ContainsKey(networkType))
            {
                var newNetwork = (BodyNetwork) Activator.CreateInstance(networkType);

                if (newNetwork == null)
                {
                    return false;
                }

                _networks.Add(networkType, newNetwork);
                newNetwork.OnCreate();

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Deletes the <see cref="BodyNetwork"/> of the given type in this body, if there is one.
        /// </summary>
        public void DeleteBodyNetwork(Type networkType)
        {
            _networks.Remove(networkType);
        }

        /// <summary>
        ///    Attempts to get the <see cref="BodyNetwork"/> of the given type in this body.
        /// </summary>
        /// <param name="networkType">The type to search for.</param>
        /// <param name="result">The <see cref="BodyNetwork"/> if found, null otherwise.</param>
        /// <returns>True if found, false otherwise.</returns>
        public bool TryGetBodyNetwork(Type networkType, [NotNullWhen(true)] out BodyNetwork result)
        {
            return _networks.TryGetValue(networkType, out result);
        }

        #endregion

        #region Recursion Functions

        /// <summary>
        ///     Returns the combined length of the distance to the nearest <see cref="BodyPart"/> with a
        ///     <see cref="FootProperty"/>. Returns <see cref="float.MinValue"/>
        ///     if there is no foot found. If you consider a <see cref="BodyManagerComponent"/> a node map, then it will look for
        ///     a foot node from the given node. It can
        ///     only search through BodyParts with <see cref="ExtensionProperty"/>.
        /// </summary>
        private static float DistanceToNearestFoot(BodyManagerComponent body, BodyPart source)
        {
            if (source.HasProperty<FootProperty>() && source.TryGetProperty<ExtensionProperty>(out var property))
            {
                return property.ReachDistance;
            }

            return LookForFootRecursion(body, source, new List<BodyPart>());
        }

        private static float LookForFootRecursion(BodyManagerComponent body, BodyPart current,
            List<BodyPart> searchedParts)
        {
            // This function is quite messy but it works as intended.
            if (current.TryGetProperty<ExtensionProperty>(out var extProperty))
            {
                // Get all connected parts if the current part has an extension property
                if (body.TryGetBodyPartConnections(current, out var connections))
                {
                    // If a connected BodyPart is a foot, return this BodyPart's length.
                    foreach (var connection in connections)
                    {
                        if (!searchedParts.Contains(connection) && connection.HasProperty<FootProperty>())
                        {
                            return extProperty.ReachDistance;
                        }
                    }

                    // Otherwise, get the recursion values of all connected BodyParts and store them in a list.
                    var distances = new List<float>();
                    foreach (var connection in connections)
                    {
                        if (searchedParts.Contains(connection))
                        {
                            var result = LookForFootRecursion(body, connection, searchedParts);
                            if (Math.Abs(result - float.MinValue) > 0.001f)
                            {
                                distances.Add(result);
                            }
                        }
                    }

                    // If one or more of the searches found a foot, return the smallest one and add this ones length.
                    if (distances.Count > 0)
                    {
                        return distances.Min<float>() + extProperty.ReachDistance;
                    }

                    return float.MinValue;
                }

                return float.MinValue;
            }

            // No extension property, no go.
            return float.MinValue;
        }

        #endregion

        private void BodyPartAdded(BodyPart part, string slotName)
        {
            var argsAdded = new BodyPartAddedEventArgs(part, slotName);

            foreach (var component in Owner.GetAllComponents<IBodyPartAdded>().ToArray())
            {
                component.BodyPartAdded(argsAdded);
            }
        }

        private void BodyPartRemoved(BodyPart part, string slotName)
        {
            var args = new BodyPartRemovedEventArgs(part, slotName);

            foreach (var component in Owner.GetAllComponents<IBodyPartRemoved>())
            {
                component.BodyPartRemoved(args);
            }
        }
    }

    public class BodyManagerHealthChangeParams : HealthChangeParams
    {
    }
}
