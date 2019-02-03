using System;
using System.Collections.Generic;
using Chargers.Core.Entities.Chargers;

namespace Chargers.Core.Entities.Vehicles
{
    public class Vehicle
    {
        private const int HourInSeconds = 3600;

        public Vehicle(string id, IDictionary<CurrentType, ChargingCurve> chargingCurves, int batteryCapacity, string manufacturer, string model, int year, int ratedWhMile)
        {
            ChargingCurves = chargingCurves;
            BatteryCapacity = batteryCapacity;
            Manufacturer = manufacturer;
            Model = model;
            Year = year;
            RatedWhMile = ratedWhMile;
            Id = id;
        }

        public string Id { get; }
        public IDictionary<CurrentType, ChargingCurve> ChargingCurves { get; }
        public int BatteryCapacity { get; }
        public int RatedWhMile { get; }
        public string Manufacturer { get; }
        public string Model { get; }
        public int Year { get; set; }

        public int MilesChargedInTimeSpan(ChargerBase charger, int soc, TimeSpan duration, int? ratedWhMiles = null)
        {
            if(ratedWhMiles != null && ratedWhMiles <= 0)
                throw new ArgumentOutOfRangeException(nameof(ratedWhMiles), "rated consumption must be greater than 0");

            int rwhMiles = ratedWhMiles ?? RatedWhMile;
            if (soc < 0 || soc > 100)
                throw new ArgumentOutOfRangeException(nameof(soc));

            if (!ChargingCurves.ContainsKey(charger.CurrentType))
                throw new ArgumentException("this type of current is not compatible", nameof(charger));

            ChargingCurve curve = ChargingCurves[charger.CurrentType];
            int totalSeconds = 0;
            decimal totalKwh = 0m;
            decimal kwhPct = BatteryCapacity / 100m;

            for (int i = soc; i < 100; i++)
            {
                decimal maxPower = curve.GetMaxChargeRateForSOC(i);
                if (maxPower > charger.MaxPower)
                    maxPower = charger.MaxPower;

                int secondsToCharge = (int)(kwhPct / maxPower * HourInSeconds);
                if (totalSeconds + secondsToCharge > duration.TotalSeconds)
                {
                    break;
                }

                totalKwh += kwhPct;
                totalSeconds += secondsToCharge;
            }

            return (int) (totalKwh / (rwhMiles / 1000m));
        }

        public TimeSpan TimeToChargeToGivenStateOfCharge(ChargerBase charger, int soc, int chargeLevel = 90)
        {
            if (soc < 0 || soc > 100)
                throw new ArgumentOutOfRangeException(nameof(soc));
            if (chargeLevel < 0 || chargeLevel > 100 || chargeLevel < soc)
                throw new ArgumentOutOfRangeException(nameof(soc));


            if (!ChargingCurves.ContainsKey(charger.CurrentType))
                throw new ArgumentException("this type of current is not compatible", nameof(charger));

            ChargingCurve curve = ChargingCurves[charger.CurrentType];
            decimal kwhPct = BatteryCapacity / 100m;
            int totalSeconds = 0;

            for (int i = soc; i < chargeLevel; i++)
            {
                decimal maxPower = curve.GetMaxChargeRateForSOC(i);
                if (maxPower > charger.MaxPower)
                    maxPower = charger.MaxPower;

                totalSeconds += (int) (kwhPct / maxPower * HourInSeconds);
            }

            return TimeSpan.FromSeconds(totalSeconds);
        }

        public TimeSpan TimeToChargeGivenMileage(ChargerBase charger, int soc, int miles, int? ratedWhMiles = null)
        {
            if (ratedWhMiles != null && ratedWhMiles <= 0)
                throw new ArgumentOutOfRangeException(nameof(ratedWhMiles), "rated consumption must be greater than 0");

            if(miles <= 0)
                throw new ArgumentOutOfRangeException(nameof(miles), "miles must be greater than 0");

            if (soc < 0 || soc > 100)
                throw new ArgumentOutOfRangeException(nameof(soc));

            if (!ChargingCurves.ContainsKey(charger.CurrentType))
                throw new ArgumentException("this type of current is not compatible", nameof(charger));

            ChargingCurve curve = ChargingCurves[charger.CurrentType];
            decimal rkwhMiles = (ratedWhMiles ?? RatedWhMile) / 1000m;
            decimal kwhPct = BatteryCapacity / 100m;
            
            decimal totalKwh = 0m;
            decimal totalSeconds = 0;
            int currentMiles = 0;
            for (int i = soc; i < 100; i++)
            {
                decimal maxPower = curve.GetMaxChargeRateForSOC(i);
                if (maxPower > charger.MaxPower)
                    maxPower = charger.MaxPower;

                totalSeconds += kwhPct / maxPower * HourInSeconds;
                totalKwh += kwhPct;
                currentMiles += (int)(kwhPct / rkwhMiles);
                if (currentMiles >= miles)
                    break;
            }

            return TimeSpan.FromSeconds((double)totalSeconds);
        }
    }
}
