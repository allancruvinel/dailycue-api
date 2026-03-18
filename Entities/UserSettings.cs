using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace dailycue_api.Entities;

public class UserSettings
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public required User User { get; set; }
    public string SettingsJson { get; set; } = "{}";
    public SettingsModel GetSettings()
    {
        return System.Text.Json.JsonSerializer.Deserialize<SettingsModel>(SettingsJson) ?? new SettingsModel();
    }
}

public class SettingsModel
{
    public bool NotificatonsEnabled { get; set; } = true;
    public string TimeZone { get; set; } = "UTC";
}