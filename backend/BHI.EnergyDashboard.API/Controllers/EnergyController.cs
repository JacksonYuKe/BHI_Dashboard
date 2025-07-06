using BHI.EnergyDashboard.API.Models;
using BHI.EnergyDashboard.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BHI.EnergyDashboard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnergyController : ControllerBase
    {
        private readonly IDataService _dataService;

        public EnergyController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet("locations")]
        public async Task<ActionResult<List<LocationInfo>>> GetLocations()
        {
            try
            {
                var locations = await _dataService.GetLocationsAsync();
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving locations: {ex.Message}");
            }
        }

        [HttpGet("locations/{locationId}/weeks")]
        public async Task<ActionResult<List<DateTime>>> GetAvailableWeeks(string locationId)
        {
            try
            {
                var weeks = await _dataService.GetAvailableWeeksAsync(locationId);
                return Ok(weeks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving weeks for location {locationId}: {ex.Message}");
            }
        }

        [HttpGet("locations/{locationId}/consumption")]
        public async Task<ActionResult<WeeklyConsumption>> GetWeeklyConsumption(
            string locationId, 
            [FromQuery] DateTime weekStart)
        {
            try
            {
                var consumption = await _dataService.GetWeeklyConsumptionAsync(locationId, weekStart);
                
                if (consumption == null)
                {
                    return NotFound($"No consumption data found for location {locationId} starting {weekStart:yyyy-MM-dd}");
                }

                return Ok(consumption);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving consumption data: {ex.Message}");
            }
        }
    }
}