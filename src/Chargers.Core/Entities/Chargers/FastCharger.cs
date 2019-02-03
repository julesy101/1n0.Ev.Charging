
namespace Chargers.Core.Entities.Chargers
{
    public class FastCharger : ChargerBase
    {
        public FastCharger(decimal power)
            : base(CurrentType.Ac, power) { }

    }
}
