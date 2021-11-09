using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Respiratory;
using Content.Shared.Actions;
using Content.Shared.Actions.Behaviors.Item;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Interaction;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Player;
using Robust.Shared.Serialization.Manager.Attributes;

S
d.Utility;
using RobShared.ViewVariables;

namespace Content.Server.Atmos.Components
{
 RegisterComponent]
    [ComponentReference(typeof(IActivate))]
#pragma warning disable 618
    public class GasTankComponent :po    xamine, IGasMixtureHolder, IUse, IDropped,     e
#pragma warning restore 618
    {
        p    erride string Name => "GasTank";

        private const float MaxExplosio     14f;
        private const f    aultOutputPressure = Atmospherics.OneAtmosphere;

        private int _integrity =         [ComponentDependency] private readonly ItemActionsCompone    mActions = null;

        [ViewVariables] private BoundUserInterface? _userInterface;

        [DataField("ruptureSound    ate SoundSpecifier _ruptureSound = new SoundPathSpecifier("Audio/Effects/spray.og          [DataFie    )] [ViewVariables] public GasM    ir { get; set;     );

        /// <summary>
            Distributed     .
        /// </summary>
        [DataField("outputPressure")]
        [View    s]
        pub    t OutputPressure { get; private set; } =    OutputPressure;      /// <summary>
        ///     Tank is connected to in    
        /// <    >
        [ViewVariables] public bool IsConnected { get; set; }

        /// <    
        ///       ents that tank is functional and can be connected to internals      /// </summar       public bool IsFunctional => GetInternalsComp    != null;

         summary>
        ///     Pressur    ch tanks start leaking.
        /// </summary>
        [DataField("tankLeakPressure"       public floa    akPressure { get; set; }     = 30 * Atmospherics.OneAtmosphere;

        <summary>
            Pressure at which tank spills a    nts into atmosphere.
        /// </summary>
        [DataField("tankRupturePressure"       public floa    pturePressure { get; set; }     Atmospherics.On    ere;

        /// <summary>
            Base 3x3 explosion.
        /// </summary>
        [DataField("tankFragmentPressure"       public floa    agmentPressure { get; set; } = 50 * Atmospherics.OneAtmosphere;
     /// <summary>
    ///     Increases explosion for e    e kPa above threshold.
        /// </summary>
        [DataField("tankFragmentScale"       public float TankFragmentScale { g     }        tmospherics.OneAtmo             protected override void Initialize()
        {
                tialize();
            _user        Ow            SharedGasTankUiKey.Key);
            if (_userInterface != null)
                        nterface.OnReceiveMessage += UserInterfaceOnOnRece    ge          }
        }

        public v        rface(IPlayerSession sessio                 _userInterface?.Open(session);
            UpdateUserInterfac    
               public void Examine(FormattedMessage message, bool inDetailsRange)
        {
            message.Ad        GetString("comp-g        in             Math.Round(Air?.Pressu                     if (IsConnected)
            {
                messa        \n            message.AddMarkup(Loc.GetString("c    ta        "));
                    
        protected override    utd          {
            base.Shutdown();
          sc        ternals();
        }

        p        ture? RemoveAir        t)
        {           s = Air?.Remove(amount);
            CheckStatus               gas;
        }

            asMixture RemoveAirVolume(float             {
            if (Air == null              return new GasMixture(volume)                      e = Air.Pressure;
            i            < OutputPressure)
                        tputPressure = tankPressure;
                UpdateUserInterface();
            }          var molesNeeded = OutputPressure         Atmospherics.R *             ;

            var ai        r(mol                    if (air != null)
               .Volume = vo           lse
                return new GasMixture(volume);              ir;
        }

        bool IUse.UseEntity(UseEntityEventArgs eventArgs)
                  if (!eventArgs.User.TryGetCompo        orComponent?     ret    e;
            OpenInterface(actor.PlayerSession);
                
        }

        void IActivate.Activate(ActivateEventArgs eventArgs)                   if (!eventArgs.User.TryGe    nt(    rComponent? actor)) return;
         pe        ctor.PlayerSession);
        }

        pu        nnectToInternals()
        {
                    ted || !IsFunctional) return;
         ar internals = GetInternalsComponent();
               ernals == null) return;           ected = internals.TryConnectTank(Owner);
            Update    rf           }

        public void         omInternals(IEntity?         )
        {
            if (!IsConnected) return          IsConnected = false;
           rnalsComponent(owner)?.DisconnectTank();
            UpdateU    fa          }

        public void UpdateUserInterf        tialUpdate = false)
                  var internals = GetInternalsCompone                              tate(
                new GasTankBo                e
                {
                    TankPressure = Air?.Press                          OutputPressure = initial                ure : (float?) null,
                    InternalsConnec            d,
                CanConnectInternals = IsF         internals != null
                });

            if (internals == null) return;
         ite    ?.GrantOrUpdate(ItemActionType.ToggleInternals, IsFunctional, IsConnected);
        }
     p        UserInterfaceOnOnReceiveM        rB            eMessage message)
        {
                        Message)
            {
                        SetPres            
                    OutputPressure =                               break                se GasT        er    age                    ToggleInternals(               break;
            }
        }

        int        oggleInternals()
        {
            var user = GetInternalsComponent()?.Owne            if (user         EntitySystem.Get<        rS            user.Uid))
                                 if         )
         
                Disco    mIn    );
                return;
            }

            ConnectToInternals(              private InternalsComponent? GetI        onent(IEntity? owner = null)
        {
            if (Owner.Deleted) retu                 if (owner != null) return owner.GetComp            rnalsComponent>();
            return Owner.TryGetContainer            er)
               iner.Owner.GetComponentOrNull<InternalsC    >(              : null;
        }

        public void AssumeAir(G        ver)
        {
           ystem.Get<AtmosphereSystem    e(        
            Chec               }

           void CheckStatus()
        {
            if (Air == null)
            return;

            var atmos        = EntitySystem.Get<AtmosphereSystem>(                       Air.Pressure;

            if (pressure > TankFragm                     {
                /             c                re pressure.
                for (v             i+               {
                                .React(Air, this);
                }

                pressure = Air                       var range = (pressure -            ssure) / TankFragmentScale;

               L                on, yeah?
                i            plo                       {
                    range = MaxExplosionRange;
                }

                Owner.SpawnExplosion((int) (range * 0.            ge * 0.5f), (int) (ra            

              ner        ();
                return;
                              e > TankRupturePressu            {
                _integrity <= 0)
                {
                    var environment = atmosphereSys                wner.Transform.Coordinat                           if(environment != null)
                            reSystem.Merge(environment, Air);

                    SoundSystem.Play(Filter.Pvs(Owner), _ruptureSound.GetSound(), Owner.Transf                ioHelpers.WithVariati                                    eDe                        re                  }
                   --;
                return;
                           sure > TankLeakPressu            {
                _integrity <= 0)
                {
                    var environment = atmosphereSys                wner.Transform.Coordinate                          if                 )
                        return;

                     eakedGas = Air.RemoveRatio(0.25f);
                         eS            ronme                                          el                                 _i                    }

                                }

           (_i     < 3)
                _integrity++;
        }

       d         pped(DroppedEventArgs eventArgs)
              DisectFromInternals(tArgs.User);
    }
    }

    [UsedImplicitly]
    [DataDefinition]
    ic    oggleInternalsAction : IToggleItemAction
    {
        publ    Do        (ToggleItemActionEventArgs args)
        {
            if (!args.Item.TryGetComponent<GasT        >(out var gas        t)) return false;
            // no change
            if (gasTank        Connected == args.ToggledOn) return                gasTankComponent.ToggleInternals();
                  successfully toggle to the desired status?
                asConent.IsConnected == args.ToggledOn;
        }
    }
}
