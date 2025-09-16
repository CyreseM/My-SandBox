using System;
using System.ComponentModel.DataAnnotations;

namespace StatusAPI.Models;



public class Status
{
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    public string UserName { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
}

public class CreateStatusRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    public string UserName { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
}