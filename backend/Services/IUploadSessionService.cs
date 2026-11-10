
using backend.DTOs;

namespace backend.Services;

public interface IUploadSessionService
{
    Task<List<UploadSessionResponseDto>> GetSessionsAsync(int userId);

    Task<UploadSessionResponseDto?> GetSessionByIdAsync(
        int userId,
        Guid sessionId);

    Task<UploadSessionResponseDto?> CreateSessionAsync(
        int userId,
        CreateUploadSessionDto dto);

    Task<UploadSessionResponseDto?> UpdateProgressAsync(
        int userId,
        Guid sessionId,
        int completedChunks);

    Task<bool> CancelSessionAsync(
        int userId,
        Guid sessionId);
}
