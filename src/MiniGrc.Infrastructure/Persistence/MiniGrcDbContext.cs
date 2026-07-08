using Microsoft.EntityFrameworkCore;
using MiniGrc.Domain.Entities;

namespace MiniGrc.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the GRC domain. This is the only place in the solution that references
/// a specific database provider (Npgsql/PostgreSQL). The Application and Domain layers never see
/// this type; they depend only on the <c>IUnitOfWork</c> port.
/// </summary>
public sealed class MiniGrcDbContext : DbContext
{
    /// <summary>Controls.</summary>
    public DbSet<Control> Controls => Set<Control>();

    /// <summary>Evidence artifacts (child of Control).</summary>
    public DbSet<Evidence> Evidence => Set<Evidence>();

    /// <summary>Findings produced by the agent.</summary>
    public DbSet<Finding> Findings => Set<Finding>();

    /// <summary>Remediation tasks (child of Finding).</summary>
    public DbSet<RemediationTask> RemediationTasks => Set<RemediationTask>();

    /// <summary>Risk register entries.</summary>
    public DbSet<Risk> Risks => Set<Risk>();

    /// <summary>Constructs the context with the supplied options (connection string, provider).</summary>
    public MiniGrcDbContext(DbContextOptions<MiniGrcDbContext> options) : base(options)
    {
    }

    /// <summary>Configures the model: keys, relationships, value conversions, and auditing.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Control aggregate
        modelBuilder.Entity<Control>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
            builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Description).HasMaxLength(2000);
            builder.Property(c => c.Owner).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Framework)
                   .HasConversion<int>();
            builder.Property(c => c.Status)
                   .HasConversion<int>();
            builder.HasMany(c => c.Evidence)
                   .WithOne()
                   .HasForeignKey(e => e.ControlId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(c => c.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(c => c.Code).IsUnique();
        });

        // Evidence child entity
        modelBuilder.Entity<Evidence>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            builder.Property(e => e.ContentType).HasMaxLength(200);
            builder.Property(e => e.UploadedBy).IsRequired().HasMaxLength(200);
            builder.Property(e => e.Status).HasConversion<int>();
        });

        // Finding aggregate
        modelBuilder.Entity<Finding>(builder =>
        {
            builder.HasKey(f => f.Id);
            builder.Property(f => f.Title).IsRequired().HasMaxLength(500);
            builder.Property(f => f.Source).IsRequired().HasMaxLength(200);
            builder.Property(f => f.ExternalId).IsRequired().HasMaxLength(200);
            builder.Property(f => f.Severity).HasConversion<int>();
            builder.HasMany(f => f.RemediationTasks)
                   .WithOne()
                   .HasForeignKey(t => t.FindingId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(f => f.ExternalId);
        });

        modelBuilder.Entity<RemediationTask>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Title).IsRequired().HasMaxLength(500);
            builder.Property(t => t.Priority).HasConversion<int>();
        });

        // Risk aggregate
        modelBuilder.Entity<Risk>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
            builder.Ignore(r => r.Severity);
        });
    }
}
