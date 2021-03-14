using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Holiday.Interfaces
{
    [ImplicitDataDefinitionForInheritors]
    public interface IHolidayCelebrate
    {
        /// <summary>
        ///     This method is called before a round starts.
        ///     Use it to do any fun festive modifications.
        /// </summary>
        void Celebrate(HolidayPrototype holiday);
    }
}
