using System.Text.Json.Serialization;

namespace Sona.Application.DTOs;

public class CurrentUserProfileDto
{
    public required string Id { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    public required List<ImageDto> Images { get; set; }
}
