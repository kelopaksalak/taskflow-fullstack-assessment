using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Configuration;

public class JwtOptions
{
    [Required]
    public string Issuer {get; set;} = string.Empty;
    
    [Required]
    public string Audience {get; set;} = string.Empty;

    [Required]
    [MinLength(32)]
    public string SigningKey {get; set;} = string.Empty;

    [Range(1, 1440)]
    public int ExpiryMinutes {get; set;}

}