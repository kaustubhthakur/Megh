
using backend.DTOs;

namespace backend.Services;

public interface IStoredFileService
{
    Task<List<StoredFileResponseDto>> GetFilesAsync(int userId);

    Task<StoredFileResponseDto?> GetFileByIdAsync(
        int userId,
        Guid fileId);

    Task<StoredFileResponseDto> CreateFileAsync(
        int userId,
        CreateStoredFileDto dto);
}
