using Microsoft.AspNetCore.Mvc;
using TravelSqlClient.Exceptions;
using TravelSqlClient.Models;
using TravelSqlClient.Models.DTOs;
using TravelSqlClient.Services;

namespace TravelSqlClient.Controllers;

[ApiController]
[Route("clients")]
public class ClientController(IDbService dbService) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAllClients()
    {
        return Ok(await dbService.GetAllClientsAsync());
    }
    
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
    
    // 3. POST /api/clients
    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] ClientCreateDTO body)
    {
        var client = await dbService.CreateClientAsync(body);
        return Created($"clients/{client.IdClient}", client);
    }
    
    // additional Delete /api/clients/{id}
    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient([FromRoute] int idClient)
    {
        try
        {
            await dbService.RemoveClientByIdAsync(idClient);
            return NoContent();
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    
    // 4. PUT /api/clients/{id}/trips/{tripId}
    [HttpPut("{idClient}/trips/{IdTrip}")]
    public async Task<IActionResult> PutClientInTrip([FromRoute] int idClient, [FromRoute] int idTrip)
    {
        try
        {
            var result = await dbService.PutClientInTripAsync(idClient, idTrip);
            return Created("", result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (MaxPeopleReachedException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    // 5. DELETE /api/clients/{id}/trips/{tripId}
    [HttpDelete("{idClient}/trips/{IdTrip}")]
    public async Task<IActionResult> DeleteClientFromTrip([FromRoute] int idClient, [FromRoute] int idTrip)
    {
        try
        {
            await dbService.DeleteClientFromTripAsync(idClient, idTrip);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}