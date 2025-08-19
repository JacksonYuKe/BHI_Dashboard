using BHI.EnergyDashboard.API.Models;

namespace BHI.EnergyDashboard.API.Services
{
    public interface IDataService
    {
        Task<List<EnergyConsumption>> LoadAllDataAsync();
        Task<List<LocationInfo>> GetLocationsAsync(double threshold = 2.0);
        Task<WeeklyConsumption?> GetWeeklyConsumptionAsync(string locationId, DateTime weekStart, double threshold = 2.0);
        Task<List<DateTime>> GetAvailableWeeksAsync(string locationId);
    }
}