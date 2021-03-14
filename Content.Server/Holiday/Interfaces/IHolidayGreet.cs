using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Holiday.Interfaces
{
    [ImplicitDataDefinitionForInheritors]
    public interface IHolidayGreet
    {
        string Greet(HolidayPrototype holiday);
    }
}
