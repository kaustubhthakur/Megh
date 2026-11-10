
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class StoredFileService : IStoredFileService
{
    private readonly ApplicationDbContext _context;

    public StoredFileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StoredFileResponseDto>> GetFilesAsync(
        int userId)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new StoredFileResponseDto
            {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType,
                OriginalSizeBytes = f.OriginalSizeBytes,
                EncryptionKeyId = f.EncryptionKeyId,
                FileHash = f.FileHash,
                ChunkCount = f.ChunkCount,
                Status = f.Status,
                CreatedAt = f.CreatedAt,
                CompletedAt = f.CompletedAt
            })
            .ToListAsync();
    }

    public async Task<StoredFileResponseDto?> GetFileByIdAsync(
        int userId,
        Guid fileId)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f =>
                f.Id == fileId &&
                f.UserId == userId)
            .Select(f => new StoredFileResponseDto
            {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType,
                OriginalSizeBytes = f.OriginalSizeBytes,
                EncryptionKeyId = f.EncryptionKeyId,
                FileHash = f.FileHash,
                ChunkCount = f.ChunkCount,
                Status = f.Status,
                CreatedAt = f.CreatedAt,
                CompletedAt = f.CompletedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<StoredFileResponseDto> CreateFileAsync(
        int userId,
        CreateStoredFileDto dto)
    {
        var storedFile = new StoredFile
        {
            UserId = userId,
            FileName = dto.FileName,
            ContentType = dto.ContentType,
            OriginalSizeBytes = dto.OriginalSizeBytes,
            EncryptionKeyId = dto.EncryptionKeyId,
            FileHash = dto.FileHash,
            ChunkCount = 0,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.StoredFiles.Add(storedFile);

        await _context.SaveChangesAsync();

        return new StoredFileResponseDto
        {
            Id = storedFile.Id,
            FileName = storedFile.FileName,
            ContentType = storedFile.ContentType,
            OriginalSizeBytes = storedFile.OriginalSizeBytes,
            EncryptionKeyId = storedFile.EncryptionKeyId,
            FileHash = storedFile.FileHash,
            ChunkCount = storedFile.ChunkCount,
            Status = storedFile.Status,
            CreatedAt = storedFile.CreatedAt,
            CompletedAt = storedFile.CompletedAt
        };
    }
}
