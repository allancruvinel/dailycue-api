using System;

namespace dailycue_api.DTO.Requests;

public class LoginUserRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
