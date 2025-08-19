namespace BHI.EnergyDashboard.API.Models
{
    public class WeeklyConsumption
    {
        public string LocationId { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public List<DailyConsumption> DailyData { get; set; } = new();
        public double Baseline { get; set; }
        public double Threshold { get; set; }
        public bool HasChargers { get; set; }
        public bool IsPredicted { get; set; }
    }

    public class DailyConsumption
    {
        public DateTime Date { get; set; }
        public double[] HourlyConsumption { get; set; } = new double[24];
        public bool ExceedsThreshold { get; set; }
    }
}