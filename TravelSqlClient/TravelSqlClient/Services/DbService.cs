using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TravelSqlClient.Models.DTOs;

namespace TravelSqlClient.Services;

public interface IDbService
{
    public Task<IEnumerable<TripGetDTO>> GetAllTripsAsync();
}

public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Default");
    
    // 1. GET /api/trips
    // SELECT * FROM Trip
    public async Task<IEnumerable<TripGetDTO>> GetAllTripsAsync()
    {
        var result = new List<TripGetDTO>();

        await using var connection = new SqlConnection(_connectionString);
        var sql = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name FROM Trip t 
                    JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip 
                    JOIN Country c ON ct.IdCountry = c.IdCountry";
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new TripGetDTO
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = (DateTime)reader.GetValue(3),
                DateTo = (DateTime)reader.GetValue(4),
                MaxPeople = reader.GetInt32(5),
                Countries = new List<string>()
            });
            
            result.Last().Countries.Add(reader.GetString(6));
        }

        return result;
    }
}