using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TravelSqlClient.Exceptions;
using TravelSqlClient.Models.DTOs;

namespace TravelSqlClient.Services;

public interface IDbService
{
    public Task<IEnumerable<TripCountryDTO>> GetAllTripsAsync();
    public Task<ClientTripsDTO> GetTripsByClientIdAsync(int idClient);
}

public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Default");
    
    // 1. GET /api/trips
    // localhost:0000/trips
    public async Task<IEnumerable<TripCountryDTO>> GetAllTripsAsync()
    {
        var result = new List<TripCountryDTO>();

        await using var connection = new SqlConnection(_connectionString);
        var sql = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.IdCountry, c.Name 
                    FROM Trip t 
                    JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip 
                    JOIN Country c ON ct.IdCountry = c.IdCountry";
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var Trip = new TripGetDTO()
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = (DateTime)reader.GetValue(3),
                DateTo = (DateTime)reader.GetValue(4),
                MaxPeople = reader.GetInt32(5)
            };
            result.Add(new TripCountryDTO()
            {
                Trip = Trip,
                Countries = new List<CountryGetDTO>()
            });

            result.Last().Countries.Add(new CountryGetDTO()
            {
                IdCountry = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return result;
    }
    
    // 2. GET /api/clients/{id}/trips
    public async Task<ClientTripsDTO> GetTripsByClientIdAsync(int idClient)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT 1 FROM Client WHERE IdClient = @idClient";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idClient", idClient);
        await connection.OpenAsync();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (!reader.HasRows)
            {
                throw new NotFoundException($"Client with id {idClient} not found");
            }
        }

        var result = new ClientTripsDTO
        {
            idClient = idClient,
            Trips = new List<TripGetDTO>()
        };

        //  
        var sql2 = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, ct.IdClient FROM Trip t
                    JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
                    WHERE ct.IdClient = @idClient";
        await using var command2 = new SqlCommand(sql2, connection);
        command2.Parameters.AddWithValue("@idClient", idClient);
        await using var reader2 = await command2.ExecuteReaderAsync();

        if (!reader2.HasRows)
        {
            throw new NotFoundException($"Client with id {idClient} does not have trips");
        }
        
        while (await reader2.ReadAsync())
        {
            result.Trips.Add(new TripGetDTO
            {
                IdTrip = reader2.GetInt32(0),
                Name = reader2.GetString(1),
                Description = reader2.GetString(2),
                DateFrom = (DateTime)reader2.GetValue(3),
                DateTo = (DateTime)reader2.GetValue(4),
                MaxPeople = reader2.GetInt32(5)
            });
        }

        return result;
    }
}