namespace BHI.EnergyDashboard.API.Models
{
    public class TransformerWeeklyAnalysis
    {
        public string TransformerId { get; set; }
        public double RatingKva { get; set; }
        public string FeederId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public int LocationCount { get; set; }
        
        public List<DailyTransformerLoad> DailyLoads { get; set; } = new List<DailyTransformerLoad>();
        
        public TransformerWeeklyMetrics Metrics { get; set; }
    }
    
    public class TransformerWeeklyMetrics
    {
        public double WeeklyMaxLoadKw { get; set; }
        public double WeeklyMaxLoadRate { get; set; }
        public int TotalOverloadHours { get; set; }
        public int NumberOfOverloadDays { get; set; }
        public double AverageLoadRate { get; set; }
        public string LoadRateCategory { get; set; } = "";
        public string CategoryColor { get; set; } = "";
    }
}