using BHI.EnergyDashboard.API.Models;
using CsvHelper;
using System.Globalization;

namespace BHI.EnergyDashboard.API.Services
{
    public class DataService : IDataService
    {
        private readonly IConfiguration _configuration;
        private readonly IChargerPredictionService _chargerPredictionService;
        private readonly IConsumptionAnalysisService _consumptionAnalysisService;
        private readonly ILogger<DataService> _logger;
        private List<EnergyConsumption>? _cachedData;

        public DataService(IConfiguration configuration, 
            IChargerPredictionService chargerPredictionService,
            IConsumptionAnalysisService consumptionAnalysisService,
            ILogger<DataService> logger)
        {
            _configuration = configuration;
            _chargerPredictionService = chargerPredictionService;
            _consumptionAnalysisService = consumptionAnalysisService;
            _logger = logger;
        }

        public async Task<List<EnergyConsumption>> LoadAllDataAsync()
        {
            if (_cachedData != null)
            {
                _logger.LogInformation("Returning cached data with {Count} records", _cachedData.Count);
                return _cachedData;
            }

            _logger.LogInformation("Starting to load CSV data...");
            var dataPath = "/Users/jackson/Project/Project_WIth_BHI/Data/Data_By_Month_CSV_Sample";
            var csvFiles = Directory.GetFiles(dataPath, "*.csv");
            var allData = new List<EnergyConsumption>();

            _logger.LogInformation("Found {FileCount} CSV files", csvFiles.Length);

            foreach (var file in csvFiles)
            {
                _logger.LogInformation("Loading file: {FileName}", Path.GetFileName(file));
                var data = await LoadCsvFileAsync(file);
                allData.AddRange(data);
                _logger.LogInformation("Loaded {RecordCount} records from {FileName}", data.Count, Path.GetFileName(file));
            }

            _logger.LogInformation("Total records loaded: {TotalCount}", allData.Count);
            _cachedData = allData;
            return allData;
        }

        private async Task<List<EnergyConsumption>> LoadCsvFileAsync(string filePath)
        {
            var records = new List<EnergyConsumption>();

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var record = new EnergyConsumption
                {
                    Date = DateTime.ParseExact(csv.GetField("YYYYMMDD")!, "yyyyMMdd", CultureInfo.InvariantCulture),
                    Location = csv.GetField("LOCATION")!,
                    RateClassDesc = csv.GetField("RATECLASS_DESC")!,
                    NumberOfChargers = csv.GetField("# of Chargers")
                };

                for (int i = 0; i < 24; i++)
                {
                    var fieldName = $"R{i + 1}";
                    if (double.TryParse(csv.GetField(fieldName), out double value))
                    {
                        record.HourlyConsumption[i] = value;
                    }
                }

                records.Add(record);
            }

            return records;
        }

        public async Task<List<LocationInfo>> GetLocationsAsync()
        {
            var allData = await LoadAllDataAsync();
            var locationGroups = allData.GroupBy(x => x.Location);
            var locations = new List<LocationInfo>();

            foreach (var group in locationGroups)
            {
                var locationData = group.ToList();
                var hasConfirmedChargers = locationData.Any(x => x.HasChargers);
                var baseline = _consumptionAnalysisService.CalculateBaseline(locationData);
                
                var locationInfo = new LocationInfo
                {
                    LocationId = group.Key,
                    HasConfirmedChargers = hasConfirmedChargers,
                    Baseline = baseline,
                    AvailableWeeks = GetWeekStartDates(locationData)
                };

                if (!hasConfirmedChargers)
                {
                    var prediction = await _chargerPredictionService.PredictChargerPresenceAsync(locationData);
                    locationInfo.HasPredictedChargers = prediction.HasChargers;
                    locationInfo.ChargerPredictionProbability = prediction.Probability;
                }

                if (hasConfirmedChargers || locationInfo.HasPredictedChargers)
                {
                    locations.Add(locationInfo);
                }
            }

            return locations;
        }

        public async Task<WeeklyConsumption?> GetWeeklyConsumptionAsync(string locationId, DateTime weekStart)
        {
            var allData = await LoadAllDataAsync();
            var locationData = allData.Where(x => x.Location == locationId).ToList();
            
            if (!locationData.Any())
                return null;

            var weekEnd = weekStart.AddDays(6);
            var weekData = locationData.Where(x => x.Date >= weekStart && x.Date <= weekEnd).ToList();
            
            if (!weekData.Any())
                return null;

            var baseline = _consumptionAnalysisService.CalculateBaseline(locationData);
            var threshold = 2.0;
            var hasConfirmedChargers = locationData.Any(x => x.HasChargers);
            var hasPredictedChargers = false;

            if (!hasConfirmedChargers)
            {
                var prediction = await _chargerPredictionService.PredictChargerPresenceAsync(locationData);
                hasPredictedChargers = prediction.HasChargers;
            }

            var dailyData = new List<DailyConsumption>();
            
            for (int i = 0; i < 7; i++)
            {
                var currentDate = weekStart.AddDays(i);
                var dayData = weekData.FirstOrDefault(x => x.Date.Date == currentDate.Date);
                
                if (dayData != null)
                {
                    var exceedsThreshold = _consumptionAnalysisService.CheckThresholdExceeded(
                        dayData.HourlyConsumption, baseline, 2, 2);
                    
                    dailyData.Add(new DailyConsumption
                    {
                        Date = currentDate,
                        HourlyConsumption = dayData.HourlyConsumption,
                        ExceedsThreshold = exceedsThreshold
                    });
                }
                else
                {
                    dailyData.Add(new DailyConsumption
                    {
                        Date = currentDate,
                        HourlyConsumption = new double[24],
                        ExceedsThreshold = false
                    });
                }
            }

            return new WeeklyConsumption
            {
                LocationId = locationId,
                WeekStart = weekStart,
                DailyData = dailyData,
                Baseline = baseline,
                Threshold = threshold,
                HasChargers = hasConfirmedChargers || hasPredictedChargers,
                IsPredicted = !hasConfirmedChargers && hasPredictedChargers
            };
        }

        public async Task<List<DateTime>> GetAvailableWeeksAsync(string locationId)
        {
            var allData = await LoadAllDataAsync();
            var locationData = allData.Where(x => x.Location == locationId).ToList();
            
            return GetWeekStartDates(locationData);
        }

        private List<DateTime> GetWeekStartDates(List<EnergyConsumption> data)
        {
            var weekStarts = new HashSet<DateTime>();
            
            foreach (var record in data)
            {
                var weekStart = record.Date.AddDays(-(int)record.Date.DayOfWeek);
                weekStarts.Add(weekStart.Date);
            }
            
            return weekStarts.OrderBy(x => x).ToList();
        }
    }
}