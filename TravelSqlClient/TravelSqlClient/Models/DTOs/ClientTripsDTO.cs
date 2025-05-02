namespace TravelSqlClient.Models.DTOs;

public class ClientTripsDTO
{
    public int idClient { get; set; }
    public List<TripGetDTO> Trips { get; set; }
}