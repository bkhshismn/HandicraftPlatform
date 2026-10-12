using System.ComponentModel.DataAnnotations;

namespace Handicraft.Api.Models;

public sealed class UpdateProfileDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
