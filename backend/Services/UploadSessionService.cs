
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UploadSessionService : IUploadSessionService
{
    private readonly ApplicationDbContext _context;

    public UploadSessionService(ApplicationDbContext context)
    {
        _context = context;
    }

    private static UploadSessionResponseDto MapToDto(
        UploadSession session)
    {
        return new UploadSessionResponseDto
        {
            Id = session.Id,
            StoredFileId = session.StoredFileId,
            Status = session.Status,
            CompletedChunks = session.CompletedChunks,
            TotalChunks = session.TotalChunks,
            ProgressPercentage = session.TotalChunks > 0
                ? Math.Round(
                    (double)session.CompletedChunks /
                    session.TotalChunks * 100, 2)
                : 0,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
        };
    }

    public async Task<List<UploadSessionResponseDto>> GetSessionsAsync(
        int userId)
    {
        var sessions = await _context.UploadSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return sessions.Select(MapToDto).ToList();
    }

    public async Task<UploadSessionResponseDto?> GetSessionByIdAsync(
        int userId,
        Guid sessionId)
    {
        var session = await _context.UploadSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == sessionId &&
                s.UserId == userId);

        return session == null ? null : MapToDto(session);
    }

    public async Task<UploadSessionResponseDto?> CreateSessionAsync(
        int userId,
        CreateUploadSessionDto dto)
    {
        var storedFile = await _context.StoredFiles
            .FirstOrDefaultAsync(f =>
                f.Id == dto.StoredFileId &&
                f.UserId == userId);

        if (storedFile == null)
            return null;

        var existingSession = await _context.UploadSessions
            .AnyAsync(s =>
                s.StoredFileId == dto.StoredFileId &&
                s.UserId == userId &&
                s.Status != "Completed" &&
                s.Status != "Cancelled" &&
                s.Status != "Failed");

        if (existingSession)
            throw new InvalidOperationException(
                "An active upload session already exists for this file.");

        var session = new UploadSession
        {
            UserId = userId,
            StoredFileId = dto.StoredFileId,
            Status = "Started",
            CompletedChunks = 0,
            TotalChunks = dto.TotalChunks,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UploadSessions.Add(session);

        storedFile.Status = "Uploading";
        storedFile.ChunkCount = dto.TotalChunks;

        await _context.SaveChangesAsync();

        return MapToDto(session);
    }

    public async Task<UploadSessionResponseDto?> UpdateProgressAsync(
        int userId,
        Guid sessionId,
        int completedChunks)
    {
        var session = await _context.UploadSessions
            .FirstOrDefaultAsync(s =>
                s.Id == sessionId &&
                s.UserId == userId);

        if (session == null)
            return null;

        if (session.Status != "Started" &&
            session.Status != "Uploading")
        {
            throw new InvalidOperationException(
                "This upload session cannot be updated.");
        }

        if (completedChunks < session.CompletedChunks ||
            completedChunks > session.TotalChunks)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedChunks),
                "Completed chunks must be between the current progress and total chunks.");
        }

        session.CompletedChunks = completedChunks;
        session.Status = completedChunks == session.TotalChunks
            ? "Completed"
            : "Uploading";

        session.UpdatedAt = DateTime.UtcNow;

        if (session.Status == "Completed")
        {
            var storedFile = await _context.StoredFiles
                .FirstOrDefaultAsync(f =>
                    f.Id == session.StoredFileId &&
                    f.UserId == userId);

            if (storedFile != null)
            {
                storedFile.Status = "Completed";
                storedFile.CompletedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return MapToDto(session);
    }

    public async Task<bool> CancelSessionAsync(
        int userId,
        Guid sessionId)
    {
        var session = await _context.UploadSessions
            .FirstOrDefaultAsync(s =>
                s.Id == sessionId &&
                s.UserId == userId);

        if (session == null)
            return false;

        if (session.Status == "Completed" ||
            session.Status == "Cancelled")
        {
            throw new InvalidOperationException(
                "This upload session cannot be cancelled.");
        }

        session.Status = "Cancelled";
        session.UpdatedAt = DateTime.UtcNow;

        var storedFile = await _context.StoredFiles
            .FirstOrDefaultAsync(f =>
                f.Id == session.StoredFileId &&
                f.UserId == userId);

        if (storedFile != null)
        {
            storedFile.Status = "Cancelled";
        }

        await _context.SaveChangesAsync();

        return true;
    }
}
