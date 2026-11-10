
using backend.DTOs;

namespace backend.Services;

public interface IFileChunkService
{
    Task<List<FileChunkResponseDto>> GetChunksAsync(
        int userId,
        Guid storedFileId);

    Task<FileChunkResponseDto?> GetChunkByIdAsync(
        int userId,
        Guid chunkId);

    Task<FileChunkResponseDto?> CreateChunkAsync(
        int userId,
        CreateFileChunkDto dto);
}
