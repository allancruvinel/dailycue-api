using System;
using System.ComponentModel.DataAnnotations;

namespace dailycue_api.DTO.Requests;

public class TokenRequest
{
    public required string Token { get; set; }
}