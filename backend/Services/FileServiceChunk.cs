
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class FileChunkService : IFileChunkService
{
    private readonly ApplicationDbContext _context;

    public FileChunkService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FileChunkResponseDto>> GetChunksAsync(
        int userId,
        Guid storedFileId)
    {
        return await _context.FileChunks
            .AsNoTracking()
            .Where(c =>
                c.StoredFileId == storedFileId &&
                c.StoredFile.UserId == userId)
            .OrderBy(c => c.SequenceNumber)
            .Select(c => new FileChunkResponseDto
            {
                Id = c.Id,
                StoredFileId = c.StoredFileId,
                SequenceNumber = c.SequenceNumber,
                OriginalSizeBytes = c.OriginalSizeBytes,
                EncryptedSizeBytes = c.EncryptedSizeBytes,
                ChunkHash = c.ChunkHash,
                RequiredReplicaCount = c.RequiredReplicaCount,
                Status = c.Status,
                ReplicaCount = c.Replicas.Count
            })
            .ToListAsync();
    }

    public async Task<FileChunkResponseDto?> GetChunkByIdAsync(
        int userId,
        Guid chunkId)
    {
        return await _context.FileChunks
            .AsNoTracking()
            .Where(c =>
                c.Id == chunkId &&
                c.StoredFile.UserId == userId)
            .Select(c => new FileChunkResponseDto
            {
                Id = c.Id,
                StoredFileId = c.StoredFileId,
                SequenceNumber = c.SequenceNumber,
                OriginalSizeBytes = c.OriginalSizeBytes,
                EncryptedSizeBytes = c.EncryptedSizeBytes,
                ChunkHash = c.ChunkHash,
                RequiredReplicaCount = c.RequiredReplicaCount,
                Status = c.Status,
                ReplicaCount = c.Replicas.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<FileChunkResponseDto?> CreateChunkAsync(
        int userId,
        CreateFileChunkDto dto)
    {
        var fileExists = await _context.StoredFiles
            .AnyAsync(f =>
                f.Id == dto.StoredFileId &&
                f.UserId == userId);

        if (!fileExists)
            return null;

        var duplicateSequence = await _context.FileChunks
            .AnyAsync(c =>
                c.StoredFileId == dto.StoredFileId &&
                c.SequenceNumber == dto.SequenceNumber);

        if (duplicateSequence)
            throw new InvalidOperationException(
                "A chunk with this sequence number already exists.");

        var chunk = new FileChunk
        {
            StoredFileId = dto.StoredFileId,
            SequenceNumber = dto.SequenceNumber,
            OriginalSizeBytes = dto.OriginalSizeBytes,
            EncryptedSizeBytes = dto.EncryptedSizeBytes,
            ChunkHash = dto.ChunkHash,
            RequiredReplicaCount = dto.RequiredReplicaCount,
            Status = "Pending"
        };

        _context.FileChunks.Add(chunk);

        await _context.SaveChangesAsync();

        return new FileChunkResponseDto
        {
            Id = chunk.Id,
            StoredFileId = chunk.StoredFileId,
            SequenceNumber = chunk.SequenceNumber,
            OriginalSizeBytes = chunk.OriginalSizeBytes,
            EncryptedSizeBytes = chunk.EncryptedSizeBytes,
            ChunkHash = chunk.ChunkHash,
            RequiredReplicaCount = chunk.RequiredReplicaCount,
            Status = chunk.Status,
            ReplicaCount = 0
        };
    }
}
