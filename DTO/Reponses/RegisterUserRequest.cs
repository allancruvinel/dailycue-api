using System;

namespace dailycue_api.DTO.Reponses;

public class RegisterUserRequest
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
