using UOGTransport.API.Enums;

namespace UOGTransport.API.DTOs;

public class RegisterRequest
{
    // Common fields (all roles)
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? PhoneNumber { get; set; }

    // Student-specific (required only if Role == Student)
    public string? RollNumber { get; set; }
    public string? Batch { get; set; }
    public string? Department { get; set; }
    public int? Semester { get; set; }

    // Driver-specific (required only if Role == Driver)
    public string? LicenseNumber { get; set; }
}