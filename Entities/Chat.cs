using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dailycue_api.Entities;

public class Chat
{
    [Key]
    public long Id { get; set; }

    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public required User User { get; set; }
    public string? Name { get; set; }
    public string? Provider { get; set; }
    public string? WebhookUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}