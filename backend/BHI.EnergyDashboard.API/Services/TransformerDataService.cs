using BHI.EnergyDashboard.API.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace BHI.EnergyDashboard.API.Services
{
    public class TransformerDataService : ITransformerDataService
    {
        private readonly IDataService _dataService;
        private readonly IChargerPredictionService _chargerPredictionService;
        private readonly IConsumptionAnalysisService _consumptionAnalysisService;
        private readonly string _transformerFilePath;
        private List<TransformerInfo>? _transformerCache;
        private readonly object _lock = new object();
        private readonly ILogger<TransformerDataService> _logger;

        public TransformerDataService(
            IDataService dataService, 
            IChargerPredictionService chargerPredictionService,
            IConsumptionAnalysisService consumptionAnalysisService,
            IConfiguration configuration, 
            ILogger<TransformerDataService> logger)
        {
            _dataService = dataService;
            _chargerPredictionService = chargerPredictionService;
            _consumptionAnalysisService = consumptionAnalysisService;
            _logger = logger;
            _transformerFilePath = configuration["DataFiles:TransformerFilePath"] ?? 
                "/Users/jackson/Project/Project_WIth_BHI/Data/Transformer/Transformer_Location_Relationships_20240226.csv";
        }

        public async Task<List<TransformerInfo>> GetAllTransformersAsync(double threshold = 2.0)
        {
            if (_transformerCache == null)
            {
                lock (_lock)
                {
                    if (_transformerCache == null)
                    {
                        _transformerCache = LoadTransformerData();
                    }
                }
            }
            
            _logger.LogInformation("Filtering transformers with threshold {Threshold}", threshold);
            
            // Get locations with charging activity using the existing service (much more efficient)
            var locationsWithChargers = await _dataService.GetLocationsAsync(threshold);
            var chargingLocationIds = locationsWithChargers.Select(l => l.LocationId).ToHashSet();
            
            _logger.LogInformation("Found {Count} locations with charging activity", chargingLocationIds.Count);
            
            // Filter transformers that have at least one location with charging activity
            var filteredTransformers = _transformerCache.Where(transformer => 
                transformer.Locations.Any(location => chargingLocationIds.Contains(location))
            ).ToList();
            
            _logger.LogInformation("Filtered {FilteredCount} transformers from {TotalCount} based on charging activity", 
                filteredTransformers.Count, _transformerCache.Count);
            
            return filteredTransformers;
        }

        public async Task<TransformerInfo> GetTransformerByIdAsync(string transformerId)
        {
            // When getting a specific transformer, don't apply the threshold filter
            if (_transformerCache == null)
            {
                lock (_lock)
                {
                    if (_transformerCache == null)
                    {
                        _transformerCache = LoadTransformerData();
                    }
                }
            }
            return _transformerCache.FirstOrDefault(t => t.TransformerId == transformerId);
        }

        public async Task<List<string>> GetWeeksForTransformerAsync(string transformerId)
        {
            var transformer = await GetTransformerByIdAsync(transformerId);
            if (transformer == null || !transformer.Locations.Any())
            {
                return new List<string>();
            }

            var allWeeks = new HashSet<string>();
            foreach (var location in transformer.Locations)
            {
                var locationWeeks = await _dataService.GetAvailableWeeksAsync(location);
                foreach (var week in locationWeeks)
                {
                    allWeeks.Add(week.ToString("yyyy-MM-dd"));
                }
            }

            return allWeeks.OrderByDescending(w => w).ToList();
        }

        public async Task<List<EnergyConsumption>> GetTransformerConsumptionDataAsync(string transformerId, DateTime startDate, DateTime endDate)
        {
            var transformer = await GetTransformerByIdAsync(transformerId);
            if (transformer == null || !transformer.Locations.Any())
            {
                return new List<EnergyConsumption>();
            }

            var allConsumptionData = new List<EnergyConsumption>();
            
            // Load all data and filter by location and date range
            var allData = await _dataService.LoadAllDataAsync();
            
            foreach (var location in transformer.Locations)
            {
                var locationData = allData
                    .Where(c => c.Location == location && c.Date >= startDate && c.Date <= endDate)
                    .ToList();
                allConsumptionData.AddRange(locationData);
            }

            // Aggregate consumption data by date for all locations under this transformer
            var aggregatedData = allConsumptionData
                .GroupBy(c => c.Date.Date)
                .Select(g => new EnergyConsumption
                {
                    Location = transformerId,
                    Date = g.Key,
                    RateClassDesc = "Aggregated",
                    NumberOfChargers = null,
                    HourlyConsumption = new double[24]
                })
                .OrderBy(c => c.Date)
                .ToList();

            // Fill in the hourly consumption values for each day
            foreach (var dailyRecord in aggregatedData)
            {
                var dayData = allConsumptionData
                    .Where(c => c.Date.Date == dailyRecord.Date.Date)
                    .ToList();
                
                // Sum up consumption for each hour across all locations
                for (int hour = 0; hour < 24; hour++)
                {
                    double hourlySum = 0;
                    foreach (var data in dayData)
                    {
                        if (hour < data.HourlyConsumption.Length)
                        {
                            hourlySum += data.HourlyConsumption[hour];
                        }
                    }
                    dailyRecord.HourlyConsumption[hour] = hourlySum;
                }
            }

            return aggregatedData;
        }

        private List<TransformerInfo> LoadTransformerData()
        {
            try
            {
                _logger.LogInformation("Loading transformer data from: {FilePath}", _transformerFilePath);
                
                if (!File.Exists(_transformerFilePath))
                {
                    _logger.LogError("Transformer CSV file not found at: {FilePath}", _transformerFilePath);
                    return new List<TransformerInfo>();
                }

                var transformerDict = new Dictionary<string, TransformerInfo>();

                using (var reader = new StreamReader(_transformerFilePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim
                }))
                {
                    csv.Read();
                    csv.ReadHeader();
                    
                    while (csv.Read())
                    {
                        try
                        {
                            var location = csv.GetField<string>("Location");
                            var transformerId = csv.GetField<string>("Transfomer ID"); // Note: typo in CSV header
                            var ratingStr = csv.GetField<string>("Rating (kVA)");
                            var feederId = csv.GetField<string>("Feeder ID");

                            if (string.IsNullOrEmpty(transformerId))
                            {
                                _logger.LogWarning("Skipping row with empty transformer ID");
                                continue;
                            }

                            if (!double.TryParse(ratingStr, out double rating) || rating <= 0)
                            {
                                _logger.LogWarning("Skipping transformer {TransformerId} with invalid rating: {Rating}", transformerId, ratingStr);
                                continue;
                            }

                            if (!transformerDict.ContainsKey(transformerId))
                            {
                                transformerDict[transformerId] = new TransformerInfo
                                {
                                    TransformerId = transformerId,
                                    RatingKva = rating,
                                    FeederId = feederId ?? ""
                                };
                            }

                            if (!string.IsNullOrEmpty(location))
                            {
                                transformerDict[transformerId].Locations.Add(location);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error parsing CSV row, skipping");
                            continue;
                        }
                    }
                }

                _logger.LogInformation("Loaded {Count} transformers", transformerDict.Count);
                return transformerDict.Values.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transformer data from {FilePath}", _transformerFilePath);
                return new List<TransformerInfo>();
            }
        }
    }
}