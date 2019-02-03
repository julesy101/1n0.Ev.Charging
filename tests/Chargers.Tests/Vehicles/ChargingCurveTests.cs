using System;
using System.Collections.Generic;
using System.Linq;
using Chargers.Core.Entities.Chargers;
using Chargers.Core.Entities.Vehicles;
using Xunit;

namespace Chargers.Tests.Vehicles
{
    public class ChargingCurveTests
    {
        [Fact]
        public void TimeToChargeRejectsInvalidStateOfChargeParameters()
        {
            Vehicle testVehicle = BuildSimpleVehicle();
            var acCharger = Build22KwCharger();

            Assert.Throws<ArgumentOutOfRangeException>(() => testVehicle.TimeToChargeToGivenStateOfCharge(acCharger, 0, 110));
            Assert.Throws<ArgumentOutOfRangeException>(() => testVehicle.TimeToChargeToGivenStateOfCharge(acCharger, -20, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => testVehicle.TimeToChargeToGivenStateOfCharge(acCharger, -20, 110));
            Assert.Throws<ArgumentOutOfRangeException>(() => testVehicle.TimeToChargeToGivenStateOfCharge(acCharger, 50, 20));
        }

        [Fact]
        public void FlatCurveTakesPredictedTimeToCharge()
        {
            Vehicle testVehicle = BuildSimpleVehicle();

            var acCharger = Build22KwCharger();
            var ts = testVehicle.TimeToChargeToGivenStateOfCharge(acCharger, 0, 100);

            Assert.Equal(9, ts.Hours);
            Assert.Equal(5, ts.Minutes);
        }

        [Fact]
        public void SteppedCurveBetweenTwoPointsTakesPredictedTimeToCharge()
        {
            Vehicle testVehicle = BuildSimpleVehicle();

            var dcCharger = Build100KwCharger();
            var ts = testVehicle.TimeToChargeToGivenStateOfCharge(dcCharger, 0, 50);

            // should be 100kw => 30% and then 85kw to 50%
            // 0 - 30 is 30kwh at 100kw so 60(minutes) / (100 / 30) = 18mins
            // 30 - 50 is 20kwh at 85kw so 60(minutes) / (85 / 20) = 14mins

            Assert.Equal(32, ts.Minutes);
        }

        [Fact]
        public void MilesChargedInTimeSpanRejectsInvalidStateOfChargeParameters()
        {
            Vehicle testVehicle = BuildSimpleVehicle();
            var acCharger = Build22KwCharger();

            Assert.Throws<ArgumentOutOfRangeException>(() => testVehicle.MilesChargedInTimeSpan(acCharger, -10, TimeSpan.FromHours(1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => testVehicle.MilesChargedInTimeSpan(acCharger, 110, TimeSpan.FromHours(1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => testVehicle.MilesChargedInTimeSpan(acCharger, 10, TimeSpan.FromHours(1), -200));
        }

        [Fact]
        public void FlatCurveYieldsPredicatbleMilesPerCharge()
        {
            Vehicle testVehicle = BuildSimpleVehicle();
            var acCharger = Build22KwCharger();

            var res = testVehicle.MilesChargedInTimeSpan(acCharger, 10, TimeSpan.FromHours(1), 250);
            // charge curve for ac is 11kw max, so 1 hour will be 11kwh dispensed * 0.25kwhMiles (250 wh miles)
            Assert.Equal(44, res);
        }

        [Fact]
        public void SteppedCurveBetweenTwoPointsYieldsPredicatbleMilesPerCharge()
        {
            Vehicle testVehicle = BuildSimpleVehicle();
            var dcCharger = Build100KwCharger();

            var ts = testVehicle.MilesChargedInTimeSpan(dcCharger, 0, TimeSpan.FromMinutes(32), 250);
            Assert.Equal(200, ts);
        }

        [Fact]
        public void TimeToChargeGivenMileageRejectsInvalidStateOfChargeParameters()
        {
            Vehicle testVehicle = BuildSimpleVehicle();
            var acCharger = Build22KwCharger();

            Assert.Throws<ArgumentOutOfRangeException>(() => testVehicle.TimeToChargeGivenMileage(acCharger, -10, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => testVehicle.TimeToChargeGivenMileage(acCharger, 10, 100, -200));
            Assert.Throws<ArgumentOutOfRangeException>(() => testVehicle.TimeToChargeGivenMileage(acCharger, 10, -100, 250));
        }

        [Fact]
        public void FlatCurveYieldsPredicatbleTimeToChargeGivenMileage()
        {
            Vehicle testVehicle = BuildSimpleVehicle();
            var acCharger = Build22KwCharger();

            var ts = testVehicle.TimeToChargeGivenMileage(acCharger, 10, 44, 250);
            Assert.Equal(1, ts.Hours);
        }

        [Fact]
        public void SteppedCurveBetweenTwoPointsYieldsPredicatbleTimeToChargeGivenMileage()
        {
            Vehicle testVehicle = BuildSimpleVehicle();
            var dcCharger = Build100KwCharger();

            var ts = testVehicle.TimeToChargeGivenMileage(dcCharger, 0, 200, 250);
            Assert.Equal(32, ts.Minutes);
        }

        public Vehicle BuildSimpleVehicle()
        {
            Vehicle testVehicle = new Vehicle("id-test", new Dictionary<CurrentType, ChargingCurve>
            {
                [CurrentType.Ac] = BuildFlatCurve(11m),
                [CurrentType.Dc] = BuildSteppedCurve(100m, 15, 30, 50, 70, 80)
            }, 100, "Test", "Car", 2018, 250);

            return testVehicle;
        }

        public RapidCharger Build100KwCharger()
        {
            return new RapidCharger(100m);
        }

        public FastCharger Build22KwCharger()
        {
            return new FastCharger(22m);
        }

        public ChargingCurve BuildFlatCurve(decimal power)
        {
            List<ChargingCurvePoint> points = new List<ChargingCurvePoint>();
            for (int i = 0; i < 100; i++)
            {
                points.Add(new ChargingCurvePoint(i, power));
            }

            return new ChargingCurve(points.ToArray());
        }

        public ChargingCurve BuildSteppedCurve(decimal power, decimal step, params int[] stepPoints)
        {
            List<ChargingCurvePoint> points = new List<ChargingCurvePoint>();
            decimal currentOutput = power;
            for (int i = 0; i < 100; i++)
            {
                if (stepPoints.Contains(i))
                {
                    currentOutput = currentOutput - step;
                    if(currentOutput < 0)
                        throw new Exception("invalid step vs point relationship");
                }              

                points.Add(new ChargingCurvePoint(i, currentOutput));
            }

            return new ChargingCurve(points.ToArray());
        }
    }
}
