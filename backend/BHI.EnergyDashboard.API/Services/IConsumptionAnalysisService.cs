using BHI.EnergyDashboard.API.Models;

namespace BHI.EnergyDashboard.API.Services
{
    public interface IConsumptionAnalysisService
    {
        double CalculateBaseline(List<EnergyConsumption> data);
        bool CheckThresholdExceeded(double[] hourlyConsumption, double baseline, double threshold, int windowSize);
    }
}