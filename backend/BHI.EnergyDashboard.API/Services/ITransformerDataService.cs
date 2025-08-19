using BHI.EnergyDashboard.API.Models;

namespace BHI.EnergyDashboard.API.Services
{
    public interface ITransformerDataService
    {
        Task<List<TransformerInfo>> GetAllTransformersAsync(double threshold = 2.0);
        Task<TransformerInfo> GetTransformerByIdAsync(string transformerId);
        Task<List<string>> GetWeeksForTransformerAsync(string transformerId);
        Task<List<EnergyConsumption>> GetTransformerConsumptionDataAsync(string transformerId, DateTime startDate, DateTime endDate);
    }
}