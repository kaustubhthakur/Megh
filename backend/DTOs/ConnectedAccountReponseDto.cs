
namespace backend.DTOs;

public class ConnectedAccountResponseDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public long? TotalBytes { get; set; }

    public long? UsedBytes { get; set; }

    public long? AvailableBytes { get; set; }

    public bool IsConnected { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
