using BHI.EnergyDashboard.API.Models;

namespace BHI.EnergyDashboard.API.Services
{
    public class TransformerAnalysisService : ITransformerAnalysisService
    {
        private readonly ITransformerDataService _transformerDataService;

        public TransformerAnalysisService(ITransformerDataService transformerDataService)
        {
            _transformerDataService = transformerDataService;
        }

        public async Task<TransformerWeeklyAnalysis> AnalyzeWeeklyLoadAsync(string transformerId, DateTime weekStartDate)
        {
            // Get transformer info
            var transformer = await _transformerDataService.GetTransformerByIdAsync(transformerId);
            if (transformer == null)
            {
                throw new ArgumentException($"Transformer {transformerId} not found");
            }

            // Ensure weekStartDate is a Monday
            while (weekStartDate.DayOfWeek != DayOfWeek.Monday)
            {
                weekStartDate = weekStartDate.AddDays(-1);
            }
            
            var weekEndDate = weekStartDate.AddDays(6).Date.AddDays(1).AddSeconds(-1);

            // Get consumption data for the week
            var consumptionData = await _transformerDataService.GetTransformerConsumptionDataAsync(
                transformerId, weekStartDate, weekEndDate);

            // Create analysis result
            var analysis = new TransformerWeeklyAnalysis
            {
                TransformerId = transformer.TransformerId,
                RatingKva = transformer.RatingKva,
                FeederId = transformer.FeederId,
                WeekStartDate = weekStartDate,
                WeekEndDate = weekEndDate,
                LocationCount = transformer.Locations.Count
            };

            // Process daily loads
            for (int dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                var currentDate = weekStartDate.AddDays(dayOffset).Date;
                var dailyLoad = ProcessDailyLoad(currentDate, consumptionData, transformer.RatingKva);
                analysis.DailyLoads.Add(dailyLoad);
            }

            // Calculate weekly metrics
            analysis.Metrics = CalculateWeeklyMetrics(analysis.DailyLoads, transformer.RatingKva);

            return analysis;
        }

        private DailyTransformerLoad ProcessDailyLoad(DateTime date, List<EnergyConsumption> consumptionData, double ratingKva)
        {
            var dailyLoad = new DailyTransformerLoad
            {
                Date = date,
                DayOfWeek = date.ToString("dddd")
            };

            // Find the consumption data for this specific day
            var dayData = consumptionData
                .FirstOrDefault(c => c.Date.Date == date.Date);

            double maxLoad = 0;
            int overloadCount = 0;

            // Process each hour
            for (int hour = 0; hour < 24; hour++)
            {
                var hourStart = date.AddHours(hour);
                
                // Get hourly consumption for this hour
                double hourlyConsumption = 0;
                if (dayData != null && hour < dayData.HourlyConsumption.Length)
                {
                    hourlyConsumption = dayData.HourlyConsumption[hour];
                }

                // Load Rate Formula: (Actual Load kW / Transformer Rating kVA) × 100%
                // hourlyConsumption is already in kW, so use directly
                double loadKw = hourlyConsumption;
                double loadRate = (loadKw / ratingKva) * 100;
                bool isOverload = loadRate > 100;

                var hourlyLoad = new TransformerLoad
                {
                    Timestamp = hourStart,
                    LoadKw = Math.Round(loadKw, 2),
                    LoadRate = Math.Round(loadRate, 1),
                    IsOverload = isOverload
                };

                dailyLoad.HourlyLoads.Add(hourlyLoad);

                if (loadKw > maxLoad)
                {
                    maxLoad = loadKw;
                }

                if (isOverload)
                {
                    overloadCount++;
                }
            }

            dailyLoad.MaxLoadKw = Math.Round(maxLoad, 2);
            dailyLoad.OverloadHours = overloadCount;
            dailyLoad.HasOverload = overloadCount > 0;

            return dailyLoad;
        }

        private TransformerWeeklyMetrics CalculateWeeklyMetrics(List<DailyTransformerLoad> dailyLoads, double ratingKva)
        {
            var metrics = new TransformerWeeklyMetrics();

            // Find maximum load across the week
            double weeklyMaxLoad = 0;
            double totalLoad = 0;
            int totalHours = 0;

            foreach (var day in dailyLoads)
            {
                if (day.MaxLoadKw > weeklyMaxLoad)
                {
                    weeklyMaxLoad = day.MaxLoadKw;
                }

                metrics.TotalOverloadHours += day.OverloadHours;
                
                if (day.HasOverload)
                {
                    metrics.NumberOfOverloadDays++;
                }

                // Sum all hourly loads for average calculation
                foreach (var hourlyLoad in day.HourlyLoads)
                {
                    totalLoad += hourlyLoad.LoadKw;
                    totalHours++;
                }
            }

            metrics.WeeklyMaxLoadKw = Math.Round(weeklyMaxLoad, 2);
            metrics.WeeklyMaxLoadRate = Math.Round((weeklyMaxLoad / ratingKva) * 100, 1);

            // Calculate average load rate: (Average Load kW / Rating kVA) × 100%
            if (totalHours > 0)
            {
                double averageLoad = totalLoad / totalHours;
                metrics.AverageLoadRate = Math.Round((averageLoad / ratingKva) * 100, 1);
            }

            // Determine load rate category and color
            var maxLoadRate = metrics.WeeklyMaxLoadRate;
            if (maxLoadRate < 80)
            {
                metrics.LoadRateCategory = "Normal";
                metrics.CategoryColor = "success";
            }
            else if (maxLoadRate < 95)
            {
                metrics.LoadRateCategory = "Near Capacity";
                metrics.CategoryColor = "warning";
            }
            else if (maxLoadRate < 110)
            {
                metrics.LoadRateCategory = "Light Overload";
                metrics.CategoryColor = "danger";
            }
            else
            {
                metrics.LoadRateCategory = "Severe Overload";
                metrics.CategoryColor = "danger";
            }

            return metrics;
        }
    }
}