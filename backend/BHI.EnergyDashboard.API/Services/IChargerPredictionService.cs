using BHI.EnergyDashboard.API.Models;

namespace BHI.EnergyDashboard.API.Services
{
    public interface IChargerPredictionService
    {
        Task<ChargerPrediction> PredictChargerPresenceAsync(List<EnergyConsumption> locationData, double threshold = 2.0);
    }

    public class ChargerPrediction
    {
        public bool HasChargers { get; set; }
        public double Probability { get; set; }
    }
}