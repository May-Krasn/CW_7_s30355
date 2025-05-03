using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TravelSqlClient.Exceptions;
using TravelSqlClient.Models;
using TravelSqlClient.Models.DTOs;

namespace TravelSqlClient.Services;

public interface IDbService
{
    // 1. GET /api/trips
    public Task<IEnumerable<TripCountryDTO>> GetAllTripsAsync();
    // additional GET api/clients
    public Task<IEnumerable<ClientGetDTO>> GetAllClientsAsync();
    // 2. GET /api/clients/{id}/trips
    public Task<ClientWithListOfTripsDTO> GetTripsByClientIdAsync(int idClient);
    // 3. POST /api/clients
    public Task<Client> CreateClientAsync(ClientCreateDTO client);
    // additional DELETE api/clients/{id}
    public Task RemoveClientByIdAsync(int idClient);
    // 4. PUT /api/clients/{id}/trips/{tripId}
    public Task<ClientTripsDTO> PutClientInTripAsync(int idClient, int idTrip);
    // 5. DELETE /api/clients/{id}/trips/{tripId}
    public Task DeleteClientFromTripAsync(int IdClient, int idTrip);
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

    // additional GET /api/clients
    public async Task<IEnumerable<ClientGetDTO>> GetAllClientsAsync()
    {
        var result = new List<ClientGetDTO>();

        await using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT IdClient, FirstName, LastName, Email, Telephone, Pesel FROM Client";
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new ClientGetDTO
            {
                IdCLient = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Email = reader.GetString(3),
                Telephone = reader.GetString(4),
                Pesel = reader.GetString(5)
            });
        }
        
        return result;
    }
    
    // 2. GET /api/clients/{id}/trips
    public async Task<ClientWithListOfTripsDTO> GetTripsByClientIdAsync(int idClient)
    {
        List<TripGetDTO> Trips = new List<TripGetDTO>();
        
        await using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT IdClient, FirstName, LastName, Email, Telephone, Pesel FROM Client WHERE IdClient = @idClient";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idClient", idClient);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            throw new NotFoundException($"Client with id {idClient} not found");
        }
        
        ClientGetDTO cl = new ClientGetDTO();
        while (await reader.ReadAsync())
        {
            cl = new ClientGetDTO
            {
                IdCLient = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Email = reader.GetString(3),
                Telephone = reader.GetString(4),
                Pesel = reader.GetString(5)
            };
            
        }
        await reader.CloseAsync();

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
            Trips.Add(new TripGetDTO
            {
                IdTrip = reader2.GetInt32(0),
                Name = reader2.GetString(1),
                Description = reader2.GetString(2),
                DateFrom = (DateTime)reader2.GetValue(3),
                DateTo = (DateTime)reader2.GetValue(4),
                MaxPeople = reader2.GetInt32(5)
            });
        }

        return new ClientWithListOfTripsDTO
        {
            Client = cl,
            Trips = Trips
        };
    }
    
    // 3. POST /api/clients
    public async Task<Client> CreateClientAsync(ClientCreateDTO client)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);
        await connection.OpenAsync();
        
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());

        return new Client
        {
            IdClient = id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel
        };
    }

    // additional DELETE /api/clients/{id}
    public async Task RemoveClientByIdAsync(int idClient)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql1 = "SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @idClient";
        await using var command1 = new SqlCommand(sql1, connection);
        command1.Parameters.AddWithValue("@idClient", idClient);
        await  connection.OpenAsync();
        var count = (int) await command1.ExecuteScalarAsync();
        if (count > 0)
        {
            throw new InvalidOperationException($"Client with idClient {idClient} has trips, can't remove");
        }

        const string sql2 = "DELETE FROM Client WHERE IdClient = @idClient";
        await using var command2 = new SqlCommand(sql2, connection);
        command2.Parameters.AddWithValue("@idClient", idClient);
        var numOfRows = await command2.ExecuteNonQueryAsync();

        if (numOfRows == 0)
        {
            throw new NotFoundException($"Client with id {idClient} not found");
        }
    }

    // 4. PUT /api/clients/{id}/trips/{idTrip}
    public async Task<ClientTripsDTO> PutClientInTripAsync(int idClient, int idTrip)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql1 = "SELECT 1 FROM Client WHERE IdClient = @idClient";
        await using var command1 = new SqlCommand(sql1, connection);
        command1.Parameters.AddWithValue("@idClient", idClient);
        await connection.OpenAsync();
        await using (var reader = await command1.ExecuteReaderAsync())
        {
            if (!reader.HasRows) throw new NotFoundException($"Client with id {idClient} not found");
        }

        var sql2 = "SELECT MaxPeople FROM Trip WHERE IdTrip = @idTrip";
        await using var command2 = new SqlCommand(sql2, connection);
        command2.Parameters.AddWithValue("@idTrip", idTrip);
        int maxpeople;
        await using (var reader = await command2.ExecuteReaderAsync())
        {
            if (!reader.HasRows) throw new NotFoundException($"Trip with id {idTrip} not found");
            await reader.ReadAsync();
            maxpeople = reader.GetInt32(0);
        }

        var sqlCheck = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @idTrip;";
        await using var commandCheck = new SqlCommand(sqlCheck, connection);
        commandCheck.Parameters.AddWithValue("@idTrip", idTrip);
        var count = (int) await commandCheck.ExecuteScalarAsync();
        if (count >= maxpeople)
        {
            throw new MaxPeopleReachedException($"Trip {idTrip} reached its maximum people");
        }

        int data = int.Parse(DateTime.Today.ToString("yyyyMMdd"));
        var sql3 = "INSERT INTO Client_Trip (IdTrip, IdClient, RegisteredAt, PaymentDate) VALUES (@idTrip, @idClient, @registeredAt, @paymentDate)";
        await using var command3 = new SqlCommand(sql3, connection);
        command3.Parameters.AddWithValue("@idTrip", idTrip);
        command3.Parameters.AddWithValue("@idClient", idClient);
        command3.Parameters.AddWithValue("@RegisteredAt", data);
        command3.Parameters.AddWithValue("@paymentDate", DBNull.Value);
        await command3.ExecuteNonQueryAsync();

        return new ClientTripsDTO()
        {
            idClient = idClient,
            idTrip = idTrip,
            RegisteredAt = data
        };
    }

    // 5. DELETE /api/clients/{id}/trips/{tripId}
    public async Task DeleteClientFromTripAsync(int idClient, int idTrip)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql1 = "SELECT 1 FROM Client WHERE IdClient = @idClient";
        await using var command1 = new SqlCommand(sql1, connection);
        command1.Parameters.AddWithValue("@idClient", idClient);
        await connection.OpenAsync();
        await using (var reader = await command1.ExecuteReaderAsync())
        {
            if (!reader.HasRows) throw new NotFoundException($"Client with id {idClient} not found");
        }

        var sql2 = "SELECT 1 FROM Trip WHERE IdTrip = @idTrip";
        await using var command2 = new SqlCommand(sql2, connection);
        command2.Parameters.AddWithValue("@idTrip", idTrip);
        await using (var reader = await command2.ExecuteReaderAsync())
        {
            if (!reader.HasRows) throw new NotFoundException($"Trip with id {idTrip} not found");
        }
        
        var sql3 = "DELETE FROM Client_Trip WHERE IdTrip = @idTrip AND IdClient = @idClient";
        await using var command3 = new SqlCommand(sql3, connection);
        command3.Parameters.AddWithValue("@idTrip", idTrip);
        command3.Parameters.AddWithValue("@idClient", idClient);
        var numOfRows = await command3.ExecuteNonQueryAsync();

        if (numOfRows == 0)
        {
            throw new NotFoundException($"Client with id {idClient} not registered to trip {idTrip}");
        }
    }
    
}