using BHI.EnergyDashboard.API.Models;

namespace BHI.EnergyDashboard.API.Services
{
    public class ChargerPredictionService : IChargerPredictionService
    {
        private readonly IConsumptionAnalysisService _consumptionAnalysisService;

        public ChargerPredictionService(IConsumptionAnalysisService consumptionAnalysisService)
        {
            _consumptionAnalysisService = consumptionAnalysisService;
        }

        public async Task<ChargerPrediction> PredictChargerPresenceAsync(List<EnergyConsumption> locationData, double threshold = 2.0)
        {
            if (!locationData.Any())
                return new ChargerPrediction { HasChargers = false, Probability = 0 };

            var baseline = _consumptionAnalysisService.CalculateBaseline(locationData);
            var windowSize = 2;

            var weeklyData = GroupByWeek(locationData);
            var weeksExceeded = 0;
            var totalWeeks = weeklyData.Count;

            foreach (var week in weeklyData)
            {
                var weekExceeded = false;
                
                foreach (var day in week.Value)
                {
                    if (_consumptionAnalysisService.CheckThresholdExceeded(
                        day.HourlyConsumption, baseline, threshold, windowSize))
                    {
                        weekExceeded = true;
                        break;
                    }
                }

                if (weekExceeded)
                    weeksExceeded++;
            }

            var probability = totalWeeks > 0 ? (double)weeksExceeded / totalWeeks : 0;
            var hasChargers = probability > 0.5;

            return new ChargerPrediction 
            { 
                HasChargers = hasChargers, 
                Probability = probability 
            };
        }

        private Dictionary<DateTime, List<EnergyConsumption>> GroupByWeek(List<EnergyConsumption> data)
        {
            var weeks = new Dictionary<DateTime, List<EnergyConsumption>>();

            foreach (var record in data)
            {
                var weekStart = record.Date.AddDays(-(int)record.Date.DayOfWeek);
                
                if (!weeks.ContainsKey(weekStart))
                    weeks[weekStart] = new List<EnergyConsumption>();
                
                weeks[weekStart].Add(record);
            }

            return weeks;
        }
    }
}