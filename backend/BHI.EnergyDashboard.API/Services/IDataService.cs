using BHI.EnergyDashboard.API.Models;

namespace BHI.EnergyDashboard.API.Services
{
    public interface IDataService
    {
        Task<List<EnergyConsumption>> LoadAllDataAsync();
        Task<List<LocationInfo>> GetLocationsAsync();
        Task<WeeklyConsumption?> GetWeeklyConsumptionAsync(string locationId, DateTime weekStart);
        Task<List<DateTime>> GetAvailableWeeksAsync(string locationId);
    }
}