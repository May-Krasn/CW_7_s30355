using System.ComponentModel.DataAnnotations;

namespace TravelSqlClient.Models.DTOs;

public class CountryCreateDTO
{
    public int IdCountry { get; set; }
    [MaxLength(120)]
    public string Name { get; set; }
}