using System.Diagnostics.CodeAnalysis;

namespace TravelSqlClient.Models.DTOs;

public class ClientTripsDTO
{
    public int idClient { get; set; }
    public int idTrip { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}