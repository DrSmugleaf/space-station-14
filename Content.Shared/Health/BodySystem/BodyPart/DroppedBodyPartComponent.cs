﻿using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using Content.Shared.BodySystem;
using Robust.Shared.ViewVariables;
using System.Globalization;

namespace Content.Shared.BodySystem {

    /// <summary>
    ///    Component containing the data for a dropped BodyPart entity.
    /// </summary>	
    [RegisterComponent]
    public class DroppedBodyPartComponent : Component {

        public sealed override string Name => "DroppedBodyPart";

        [ViewVariables]
        private BodyPart _containedMechanism;

        public void TransferBodyPartData(BodyPart data)
        {
            _containedMechanism = data;
            Owner.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_containedMechanism.Name);
            //TODO: figure out how to do this while in shared since SpriteComponent doesn't exist in it (maybe move to server?)
            /*if (Owner.TryGetComponent<SpriteComponent>(out SpriteComponent component))
            {
                component.LayerSetRSI(0, data.RSIPath);
                component.LayerSetState(0, data.RSIState);
            }*/
        }
    }
}
