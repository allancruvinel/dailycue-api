using System.ComponentModel.DataAnnotations;

namespace dailycue_api.DTO.Requests;

public class CreateChatRequest
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(100)]
    public string? Provider { get; set; }

    [Url]
    [StringLength(2000)]
    public string? WebhookUrl { get; set; }

    public bool IsActive { get; set; } = true;
}