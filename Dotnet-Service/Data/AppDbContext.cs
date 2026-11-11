using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdempotencyRecord>().HasKey(record => record.Key);
        modelBuilder.Entity<InventoryItem>().HasKey(item => item.Sku);
        base.OnModelCreating(modelBuilder);
    }
}