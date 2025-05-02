namespace TravelSqlClient.Models.DTOs;

public class TripCountryDTO
{
    public TripGetDTO Trip { get; set; }
    public List<CountryGetDTO> Countries { get; set; } = new();
}