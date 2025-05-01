using System.ComponentModel.DataAnnotations;

namespace TravelSqlClient.Models.DTOs;

public class TripCreateDTO
{
    [MaxLength(120)]
    public required string Name { get; set; }
    [MaxLength(220)]
    public required string Description { get; set; }
    public required DateTime DateFrom { get; set; }
    public required DateTime DateTo { get; set; }
    public required int MaxPeople { get; set; }
}