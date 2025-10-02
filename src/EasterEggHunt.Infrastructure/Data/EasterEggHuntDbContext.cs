using EasterEggHunt.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasterEggHunt.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext für das Easter Egg Hunt System
/// </summary>
public class EasterEggHuntDbContext : DbContext
{
    /// <summary>
    /// Konstruktor für Dependency Injection
    /// </summary>
    /// <param name="options">DbContext-Optionen</param>
    public EasterEggHuntDbContext(DbContextOptions<EasterEggHuntDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet für Kampagnen
    /// </summary>
    public DbSet<Campaign> Campaigns { get; set; } = null!;

    /// <summary>
    /// DbSet für QR-Codes
    /// </summary>
    public DbSet<QrCode> QrCodes { get; set; } = null!;

    /// <summary>
    /// DbSet für Benutzer
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// DbSet für Funde
    /// </summary>
    public DbSet<Find> Finds { get; set; } = null!;

    /// <summary>
    /// DbSet für Sessions
    /// </summary>
    public DbSet<Session> Sessions { get; set; } = null!;

    /// <summary>
    /// DbSet für Admin-Benutzer
    /// </summary>
    public DbSet<AdminUser> AdminUsers { get; set; } = null!;

    /// <summary>
    /// Konfiguriert die Entity-Relationships und Constraints
    /// </summary>
    /// <param name="modelBuilder">Model Builder für Entity-Konfiguration</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        // Kampagne-Konfiguration
        ConfigureCampaign(modelBuilder);

        // QR-Code-Konfiguration
        ConfigureQrCode(modelBuilder);

        // Benutzer-Konfiguration
        ConfigureUser(modelBuilder);

        // Fund-Konfiguration
        ConfigureFind(modelBuilder);

        // Session-Konfiguration
        ConfigureSession(modelBuilder);

        // Admin-Benutzer-Konfiguration
        ConfigureAdminUser(modelBuilder);
    }

    /// <summary>
    /// Konfiguriert die Kampagne-Entity
    /// </summary>
    private static void ConfigureCampaign(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000);
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.Property(e => e.UpdatedAt)
                .IsRequired();
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Index für bessere Performance
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    /// <summary>
    /// Konfiguriert die QR-Code-Entity
    /// </summary>
    private static void ConfigureQrCode(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QrCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.InternalNote)
                .IsRequired()
                .HasMaxLength(1000);
            entity.Property(e => e.UniqueUrl)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => new Uri(v));
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.Property(e => e.UpdatedAt)
                .IsRequired();
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            entity.Property(e => e.SortOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Foreign Key zu Campaign
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.QrCodes)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index für bessere Performance
            entity.HasIndex(e => e.CampaignId);
            entity.HasIndex(e => e.UniqueUrl)
                .IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.SortOrder);
        });
    }

    /// <summary>
    /// Konfiguriert die Benutzer-Entity
    /// </summary>
    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.FirstSeen)
                .IsRequired();
            entity.Property(e => e.LastSeen)
                .IsRequired();
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Index für bessere Performance
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.FirstSeen);
            entity.HasIndex(e => e.LastSeen);
        });
    }

    /// <summary>
    /// Konfiguriert die Fund-Entity
    /// </summary>
    private static void ConfigureFind(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Find>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FoundAt)
                .IsRequired();

            // Foreign Key zu QrCode
            entity.HasOne(e => e.QrCode)
                .WithMany(q => q.Finds)
                .HasForeignKey(e => e.QrCodeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign Key zu User
            entity.HasOne(e => e.User)
                .WithMany(u => u.Finds)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index für bessere Performance
            entity.HasIndex(e => e.QrCodeId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.FoundAt);

            // Composite Index für häufige Abfragen
            entity.HasIndex(e => new { e.QrCodeId, e.UserId });
        });
    }

    /// <summary>
    /// Konfiguriert die Session-Entity
    /// </summary>
    private static void ConfigureSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedNever(); // GUID wird manuell generiert
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            // Foreign Key zu User
            entity.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index für bessere Performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    /// <summary>
    /// Konfiguriert die Admin-Benutzer-Entity
    /// </summary>
    private static void ConfigureAdminUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.Property(e => e.LastLogin);

            // Index für bessere Performance
            entity.HasIndex(e => e.Username)
                .IsUnique();
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
