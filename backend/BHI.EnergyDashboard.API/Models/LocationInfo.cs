namespace BHI.EnergyDashboard.API.Models
{
    public class LocationInfo
    {
        public string LocationId { get; set; } = string.Empty;
        public bool HasConfirmedChargers { get; set; }
        public bool HasPredictedChargers { get; set; }
        public double ChargerPredictionProbability { get; set; }
        public double Baseline { get; set; }
        public List<DateTime> AvailableWeeks { get; set; } = new();
    }
}