using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class InventoryReservationService : IInventoryReservationSevice
{
    private readonly AppDbContext _db;

    public InventoryReservationService(AppDbContext db) => _db = db;

    public async Task<ReservationResult> ReserveAsync(string sku, int quantity, string idempotencyKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sku) || quantity <= 0 || string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return new ReservationResult(ReservationStatus.InvalidInput, "Invalid SKU, Quantity, or idempotency key.", null);
        }

        var existingKey = await _db.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == idempotencyKey, ct);

        if (existingKey != null)
        {
            return new ReservationResult(existingKey.Status, "Duplicate request recognized.", existingKey.ReservationId);
        }

        using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        try
        {
            int rowsAffected = await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE InventoryItems SET AvailableStock = AvailableStock - {quantity} WHERE Sku = {sku} AND AvailableStock >= {quantity}", ct);

            if (rowsAffected == 0)
            {
                await tx.RollbackAsync(ct);
                return new ReservationResult(ReservationStatus.InsufficientStock, "Stock unavailable.", null);
            }

            var reservationId = Guid.NewGuid().ToString();

            _db.Reservations.Add(new Reservation
            {
                Id = reservationId,
                Sku = sku,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow
            });

            _db.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = idempotencyKey,
                Status = ReservationStatus.Success,
                ReservationId = reservationId
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new ReservationResult(ReservationStatus.Success, "Reservation confirmed.", reservationId);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            throw new ApplicationException("Database transaction failed: " + ex.Message, ex);
        }
    }
}
