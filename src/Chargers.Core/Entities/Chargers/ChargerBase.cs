
namespace Chargers.Core.Entities.Chargers
{
    public enum CurrentType
    {
        Ac,
        Dc
    }

    public class ChargerBase
    {
        protected ChargerBase(CurrentType currentType, decimal power)
        {
            CurrentType = currentType;
            MaxPower = power;
        }

        public CurrentType CurrentType { get; }
        public decimal MaxPower { get; }


    }
}
