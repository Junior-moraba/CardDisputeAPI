using CardDisputePortal.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CardDisputePortal.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Dispute> Disputes { get; set; }
    public DbSet<Evidence> EvidenceFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Transactions)
                  .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Transaction)
                  .WithOne(t => t.Dispute)
                  .HasForeignKey<Dispute>(d => d.TransactionId);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Disputes)
                  .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<Evidence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Dispute)
                  .WithMany(d => d.EvidenceFiles)
                  .HasForeignKey(e => e.DisputeId);
        });
    }
}
