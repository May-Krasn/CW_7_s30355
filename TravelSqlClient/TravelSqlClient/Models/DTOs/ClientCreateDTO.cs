using System.ComponentModel.DataAnnotations;

namespace TravelSqlClient.Models.DTOs;

public class ClientCreateDTO
{
    [MaxLength(120)]
    public string FirstName { get; set; }
    [MaxLength(120)]
    public string LastName { get; set; }
    [MaxLength(120)]
    public string Email { get; set; }
    [MaxLength(120)]
    public string Telephone { get; set; }
    [MaxLength(120)]
    public string Pesel { get; set; }
}