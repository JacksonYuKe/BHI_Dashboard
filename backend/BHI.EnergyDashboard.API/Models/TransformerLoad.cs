namespace BHI.EnergyDashboard.API.Models
{
    public class TransformerLoad
    {
        public DateTime Timestamp { get; set; }
        public double LoadKw { get; set; }
        public double LoadRate { get; set; }
        public bool IsOverload { get; set; }
    }
    
    public class DailyTransformerLoad
    {
        public DateTime Date { get; set; }
        public string DayOfWeek { get; set; }
        public List<TransformerLoad> HourlyLoads { get; set; } = new List<TransformerLoad>();
        public double MaxLoadKw { get; set; }
        public int OverloadHours { get; set; }
        public bool HasOverload { get; set; }
    }
}