using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TravelSqlClient.Models;
using TravelSqlClient.Models.DTOs;
using TravelSqlClient.Services;

namespace TravelSqlClient.Controllers;

[ApiController]
[Route("trips")]
public class TripControllers(IDbService dbService) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAllTrips()
    {
        return Ok(await dbService.GetAllTripsAsync());
    }
     
}