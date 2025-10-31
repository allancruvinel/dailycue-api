using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dailycue_api.Entities;

public class TipSchedule
{
    [Key]
    public long Id { get; set; }

    [ForeignKey("Tip")]
    public long TipId { get; set; }
    public required Tip Tip { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    [Required]
    public required string Status { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime SentAt { get; set; }
}