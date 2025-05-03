namespace TravelSqlClient.Models.DTOs;

public class ClientWithListOfTripsDTO
{
    public ClientGetDTO Client { get; set; }
    public List<TripGetDTO> Trips { get; set; }
}