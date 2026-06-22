using System.Text.Json.Serialization;

namespace Sona.Application.DTOs;

public class PlaylistEditorDto
{
    [JsonPropertyName("playlist_id")]
    public required string PlaylistId { get; set; }

    [JsonPropertyName("snapshot_id")]
    public required string SnapshotId { get; set; }

    public required int Total { get; set; }
    public required List<PlaylistItemDto> Items { get; set; }
}
