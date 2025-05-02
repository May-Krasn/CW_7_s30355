using Microsoft.AspNetCore.Mvc;
using TravelSqlClient.Exceptions;
using TravelSqlClient.Services;

namespace TravelSqlClient.Controllers;

[ApiController]
[Route("clients")]
public class ClientController(IDbService dbService) : ControllerBase
{
    
    // 2. GET /api/clients/{id}/trips
    [HttpGet("{idClient}/trips")]
    public async Task<IActionResult> GetTripsByClientId([FromRoute] int idClient)
    {
        try
        {
            return Ok(await dbService.GetTripsByClientIdAsync(idClient));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        
    }
}