
namespace Chargers.Core.Entities.Chargers
{
    public class RapidCharger : ChargerBase
    {
        public RapidCharger(decimal power) 
            : base(CurrentType.Dc, power) { }

    }
}
