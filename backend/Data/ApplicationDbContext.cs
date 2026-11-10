
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<ConnectedAccount> ConnectedAccounts => Set<ConnectedAccount>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<FileChunk> FileChunks => Set<FileChunk>();
    public DbSet<ChunkReplica> ChunkReplicas => Set<ChunkReplica>();
    public DbSet<UploadSession> UploadSessions => Set<UploadSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.StorageUsedBytes)
            .HasDefaultValue(0L);
        
        modelBuilder.Entity<UploadSession>()
            .HasIndex(s => s.StoredFileId);
            
        modelBuilder.Entity<User>()
            .Property(u => u.StorageLimitBytes)
            .HasDefaultValue(1073741824L);
        
        modelBuilder.Entity<FileChunk>()
            .HasIndex(c => new { c.StoredFileId, c.SequenceNumber })
            .IsUnique();
        modelBuilder.Entity<ConnectedAccount>()
            .HasOne(a => a.User)
            .WithMany(u => u.ConnectedAccounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConnectedAccount>()
            .HasIndex(a => new { a.UserId, a.Provider, a.Email })
            .IsUnique();

        modelBuilder.Entity<StoredFile>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FileChunk>()
            .HasOne(c => c.StoredFile)
            .WithMany(f => f.Chunks)
            .HasForeignKey(c => c.StoredFileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FileChunk>()
            .HasIndex(c => new { c.StoredFileId, c.SequenceNumber })
            .IsUnique();

        modelBuilder.Entity<ChunkReplica>()
            .HasOne(r => r.FileChunk)
            .WithMany(c => c.Replicas)
            .HasForeignKey(r => r.FileChunkId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChunkReplica>()
            .HasOne(r => r.ConnectedAccount)
            .WithMany()
            .HasForeignKey(r => r.ConnectedAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UploadSession>()
            .HasIndex(s => s.StoredFileId);
    }
}
