using BHI.EnergyDashboard.API.Models;

namespace BHI.EnergyDashboard.API.Services
{
    public class ConsumptionAnalysisService : IConsumptionAnalysisService
    {
        public double CalculateBaseline(List<EnergyConsumption> data)
        {
            if (!data.Any())
                return 0;

            var allHourlyValues = new List<double>();
            
            foreach (var record in data)
            {
                allHourlyValues.AddRange(record.HourlyConsumption);
            }

            return allHourlyValues.Average();
        }

        public bool CheckThresholdExceeded(double[] hourlyConsumption, double baseline, double threshold, int windowSize)
        {
            var thresholdValue = baseline + threshold;
            
            for (int i = 0; i <= hourlyConsumption.Length - windowSize; i++)
            {
                var allExceed = true;
                
                for (int j = i; j < i + windowSize; j++)
                {
                    if (hourlyConsumption[j] <= thresholdValue)
                    {
                        allExceed = false;
                        break;
                    }
                }

                if (allExceed)
                    return true;
            }

            return false;
        }
    }
}