using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace dailycue_api.Entities;

[Index(nameof(Email), IsUnique = true)]
public class User
{
    [Key]
    public long Id { get; set; }

    [StringLength(200)]
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}