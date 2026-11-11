using System.Threading;
using System.Threading.Tasks;

public interface IInventoryReservationSevice
{
    Task<ReservationResult> ReserveAsync(string sku, int quantity, string idempotencyKey, CancellationToken ct);
}