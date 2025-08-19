namespace BHI.EnergyDashboard.API.Models
{
    public class TransformerInfo
    {
        public string TransformerId { get; set; }
        public double RatingKva { get; set; }
        public string FeederId { get; set; }
        public List<string> Locations { get; set; } = new List<string>();
        
        public string DisplayName => $"{TransformerId} ({RatingKva}kVA) - {FeederId}";
    }
}