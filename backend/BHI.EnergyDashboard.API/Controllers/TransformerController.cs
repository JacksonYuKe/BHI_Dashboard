using Microsoft.AspNetCore.Mvc;
using BHI.EnergyDashboard.API.Models;
using BHI.EnergyDashboard.API.Services;

namespace BHI.EnergyDashboard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransformerController : ControllerBase
    {
        private readonly ITransformerDataService _transformerDataService;
        private readonly ITransformerAnalysisService _transformerAnalysisService;
        private readonly ILogger<TransformerController> _logger;

        public TransformerController(
            ITransformerDataService transformerDataService,
            ITransformerAnalysisService transformerAnalysisService,
            ILogger<TransformerController> logger)
        {
            _transformerDataService = transformerDataService;
            _transformerAnalysisService = transformerAnalysisService;
            _logger = logger;
        }

        [HttpGet("list")]
        public async Task<ActionResult<List<TransformerInfo>>> GetTransformers([FromQuery] double threshold = 2.0)
        {
            try
            {
                _logger.LogInformation("Getting transformer list with threshold {Threshold}", threshold);
                var transformers = await _transformerDataService.GetAllTransformersAsync(threshold);
                _logger.LogInformation("Found {Count} transformers with charging activity", transformers.Count);
                return Ok(transformers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transformer list");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{transformerId}/weeks")]
        public async Task<ActionResult<List<string>>> GetWeeksForTransformer(string transformerId)
        {
            try
            {
                var weeks = await _transformerDataService.GetWeeksForTransformerAsync(transformerId);
                if (!weeks.Any())
                {
                    return NotFound($"No data found for transformer {transformerId}");
                }
                return Ok(weeks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting weeks for transformer {transformerId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{transformerId}/weekly-analysis")]
        public async Task<ActionResult<TransformerWeeklyAnalysis>> GetWeeklyAnalysis(
            string transformerId, 
            [FromQuery] DateTime week,
            [FromQuery] double threshold = 2.0)
        {
            try
            {
                var analysis = await _transformerAnalysisService.AnalyzeWeeklyLoadAsync(transformerId, week);
                return Ok(analysis);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error analyzing transformer {transformerId} for week {week}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}