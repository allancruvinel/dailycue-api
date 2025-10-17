using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dailycue_api.Entities;

public class Tip
{
    [Key]
    public long Id { get; set; }

    [ForeignKey("Chat")]
    public long ChatId { get; set; }
    public required Chat Chat { get; set; }
    public string? ContentText { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

}
