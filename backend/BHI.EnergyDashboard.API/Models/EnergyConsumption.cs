namespace BHI.EnergyDashboard.API.Models
{
    public class EnergyConsumption
    {
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public string RateClassDesc { get; set; } = string.Empty;
        public string? NumberOfChargers { get; set; }
        public double[] HourlyConsumption { get; set; } = new double[24];
        public bool HasChargers => !string.IsNullOrEmpty(NumberOfChargers) && NumberOfChargers != "N/A";
    }
}