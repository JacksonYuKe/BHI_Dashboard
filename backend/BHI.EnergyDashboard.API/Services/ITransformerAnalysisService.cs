using BHI.EnergyDashboard.API.Models;

namespace BHI.EnergyDashboard.API.Services
{
    public interface ITransformerAnalysisService
    {
        Task<TransformerWeeklyAnalysis> AnalyzeWeeklyLoadAsync(string transformerId, DateTime weekStartDate);
    }
}