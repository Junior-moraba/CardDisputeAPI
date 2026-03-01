using CardDisputePortal.Core.Entities;
using CardDisputePortal.Core.Enums;
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
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Transaction)
                  .WithOne(t => t.Dispute)
                  .HasForeignKey<Dispute>(d => d.TransactionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Disputes)
                  .HasForeignKey(e => e.UserId)
                  // Use NoAction to produce SQL "ON DELETE NO ACTION" (or Restrict if preferred)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Evidence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Dispute)
                  .WithMany(d => d.EvidenceFiles)
                  .HasForeignKey(e => e.DisputeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

      
    var userId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
    var transactionIds = new[]
    {
        Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
        Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
        Guid.Parse("550e8400-e29b-41d4-a716-446655440003"),
        Guid.Parse("550e8400-e29b-41d4-a716-446655440004"),
        Guid.Parse("550e8400-e29b-41d4-a716-446655440005"),
        Guid.Parse("550e8400-e29b-41d4-a716-446655440006"),
        Guid.Parse("550e8400-e29b-41d4-a716-446655440007"),
        Guid.Parse("550e8400-e29b-41d4-a716-446655440008"),
        Guid.Parse("550e8400-e29b-41d4-a716-446655440009"),
        Guid.Parse("550e8400-e29b-41d4-a716-44665544000a"),
        Guid.Parse("550e8400-e29b-41d4-a716-44665544000b"),
        Guid.Parse("550e8400-e29b-41d4-a716-44665544000c"),
        Guid.Parse("550e8400-e29b-41d4-a716-44665544000d"),
        Guid.Parse("550e8400-e29b-41d4-a716-44665544000e"),
        Guid.Parse("550e8400-e29b-41d4-a716-44665544000f")
    };

    modelBuilder.Entity<User>().HasData(
        new User { Id = userId, PhoneNumber = "0648952191", Name = "Junior Moraba", CreatedAt = DateTime.UtcNow.AddDays(-30) }
    );

    modelBuilder.Entity<Transaction>().HasData(
        new Transaction { Id = transactionIds[0], UserId = userId, Amount = 1299.99m, MerchantName = "Takealot", Date = DateTime.UtcNow.AddDays(-15), MerchantCategory = "Online Retail", Status = TransactionStatus.Completed, Reference = "TXN001" },
        new Transaction { Id = transactionIds[1], UserId = userId, Amount = 65.50m, MerchantName = "Seattle Coffee Co", Date = DateTime.UtcNow.AddDays(-14), MerchantCategory = "Food & Beverage", Status = TransactionStatus.Completed, Reference = "TXN002" },
        new Transaction { Id = transactionIds[2], UserId = userId, Amount = 850.00m, MerchantName = "Engen", Date = DateTime.UtcNow.AddDays(-13), MerchantCategory = "Fuel", Status = TransactionStatus.Completed, Reference = "TXN003" },
        new Transaction { Id = transactionIds[3], UserId = userId, Amount = 89.90m, MerchantName = "Steers", Date = DateTime.UtcNow.AddDays(-12), MerchantCategory = "Food & Beverage", Status = TransactionStatus.Completed, Reference = "TXN004" },
        new Transaction { Id = transactionIds[4], UserId = userId, Amount = 2499.99m, MerchantName = "Mr Price Home", Date = DateTime.UtcNow.AddDays(-11), MerchantCategory = "Retail", Status = TransactionStatus.Completed, Reference = "TXN005" },
        new Transaction { Id = transactionIds[5], UserId = userId, Amount = 125.50m, MerchantName = "KFC", Date = DateTime.UtcNow.AddDays(-10), MerchantCategory = "Food & Beverage", Status = TransactionStatus.Completed, Reference = "TXN006" },
        new Transaction { Id = transactionIds[6], UserId = userId, Amount = 4999.99m, MerchantName = "Incredible Connection", Date = DateTime.UtcNow.AddDays(-9), MerchantCategory = "Electronics", Status = TransactionStatus.Disputed, Reference = "TXN007" },
        new Transaction { Id = transactionIds[7], UserId = userId, Amount = 567.89m, MerchantName = "Pick n Pay", Date = DateTime.UtcNow.AddDays(-8), MerchantCategory = "Groceries", Status = TransactionStatus.Completed, Reference = "TXN008" },
        new Transaction { Id = transactionIds[8], UserId = userId, Amount = 199.00m, MerchantName = "Netflix", Date = DateTime.UtcNow.AddDays(-7), MerchantCategory = "Entertainment", Status = TransactionStatus.Completed, Reference = "TXN009" },
        new Transaction { Id = transactionIds[9], UserId = userId, Amount = 1899.99m, MerchantName = "Sportsmans Warehouse", Date = DateTime.UtcNow.AddDays(-6), MerchantCategory = "Retail", Status = TransactionStatus.Disputed, Reference = "TXN010" },
        new Transaction { Id = transactionIds[10], UserId = userId, Amount = 234.50m, MerchantName = "Nandos", Date = DateTime.UtcNow.AddDays(-5), MerchantCategory = "Food & Beverage", Status = TransactionStatus.Completed, Reference = "TXN011" },
        new Transaction { Id = transactionIds[11], UserId = userId, Amount = 178.99m, MerchantName = "Clicks", Date = DateTime.UtcNow.AddDays(-4), MerchantCategory = "Health & Beauty", Status = TransactionStatus.Completed, Reference = "TXN012" },
        new Transaction { Id = transactionIds[12], UserId = userId, Amount = 725.00m, MerchantName = "Shell", Date = DateTime.UtcNow.AddDays(-3), MerchantCategory = "Fuel", Status = TransactionStatus.Completed, Reference = "TXN013" },
        new Transaction { Id = transactionIds[13], UserId = userId, Amount = 899.99m, MerchantName = "Game", Date = DateTime.UtcNow.AddDays(-2), MerchantCategory = "Entertainment", Status = TransactionStatus.Completed, Reference = "TXN014" },
        new Transaction { Id = transactionIds[14], UserId = userId, Amount = 189.50m, MerchantName = "Uber", Date = DateTime.UtcNow.AddDays(-1), MerchantCategory = "Transport", Status = TransactionStatus.Completed, Reference = "TXN015" }
    );

    modelBuilder.Entity<Dispute>().HasData(
        new Dispute { Id = Guid.Parse("660e8400-e29b-41d4-a716-446655440001"), TransactionId = transactionIds[0], UserId = userId, ReasonCode = DisputeReason.Unauthorized, Details = "Did not make this purchase", Status = DisputeStatus.Pending, SubmittedAt = DateTime.UtcNow.AddDays(-10), EstimatedResolutionDate = DateTime.UtcNow.AddDays(5) },
        new Dispute { Id = Guid.Parse("660e8400-e29b-41d4-a716-446655440002"), TransactionId = transactionIds[6], UserId = userId, ReasonCode = DisputeReason.NotReceived, Details = "Item never delivered", Status = DisputeStatus.UnderReview, SubmittedAt = DateTime.UtcNow.AddDays(-5), EstimatedResolutionDate = DateTime.UtcNow.AddDays(10) },
        new Dispute { Id = Guid.Parse("660e8400-e29b-41d4-a716-446655440003"), TransactionId = transactionIds[9], UserId = userId, ReasonCode = DisputeReason.Duplicate, Details = "Charged twice for same item", Status = DisputeStatus.Resolved, SubmittedAt = DateTime.UtcNow.AddDays(-2), EstimatedResolutionDate = DateTime.UtcNow.AddDays(-1) }
    );
}
}