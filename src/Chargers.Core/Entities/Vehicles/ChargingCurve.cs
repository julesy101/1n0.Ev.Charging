using System;
using System.Linq;

namespace Chargers.Core.Entities.Vehicles
{
    public class ChargingCurve
    {
        public ChargingCurve(ChargingCurvePoint[] points)
        {
            Points = points;
        }

        public ChargingCurvePoint[] Points { get; }

        public decimal GetMaxChargeRateForSOC(int soc)
        {
            if (Points.Any())
            {
                ChargingCurvePoint point = Points.FirstOrDefault(x => x.StateOfCharge == soc);
                if (point != null)
                    return point.MaxKw;

                throw new Exception("invalid state of charge for curve");
            }

            throw new Exception("no charging curve points defined");
        }
    }

    public class ChargingCurvePoint
    {
        public ChargingCurvePoint(int stateOfCharge, decimal maxKw)
        {
            StateOfCharge = stateOfCharge;
            MaxKw = maxKw;
        }

        public int StateOfCharge { get; }
        public decimal MaxKw { get; }
    }
}
