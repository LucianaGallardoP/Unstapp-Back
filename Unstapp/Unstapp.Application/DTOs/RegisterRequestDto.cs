using System.ComponentModel.DataAnnotations;

public class RegisterRequestDto
{
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DNI { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    [Url(ErrorMessage = "El Avatar debe ser una URL válida")]
    public string? AvatarUrl { get; set; }
}